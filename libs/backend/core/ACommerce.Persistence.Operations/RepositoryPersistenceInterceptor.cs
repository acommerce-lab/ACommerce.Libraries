using System.Reflection;
using ACommerce.OperationEngine.Core;
using ACommerce.OperationEngine.Interceptors;
using ACommerce.SharedKernel.Abstractions.Entities;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ACommerce.Persistence.Operations;

/// <summary>
/// معترض حفظ عام - يحفظ كيانات من ctx.Items تلقائياً عبر IBaseAsyncRepository&lt;T&gt;.
///
/// يعمل بعد التنفيذ (Post phase). يقرأ العلامات:
///   - persist_entity           = "true"    → لتفعيل المعترض
///   - persist_entity_type      = "Listing" → اسم نوع الكيان (للبحث عنه في ctx)
///   - persist_context_key      = "entity"  → مفتاح اختياري في ctx.Items (افتراضي: entity)
///   - persist_mode             = "add"     → "add" (افتراضي) أو "update" أو "soft_delete"
///
/// المتحكم يضع الكيان في ctx.Items["entity"] داخل Execute، والمعترض يستخرجه
/// عبر الانعكاس ويستدعي IBaseAsyncRepository&lt;T&gt; المناسب.
///
/// لا يحتاج المطوّر لأي تسجيل لكل نوع على حدة - يعمل لأي كيان يطبّق IBaseEntity.
/// </summary>
public class RepositoryPersistenceInterceptor : IOperationInterceptor
{
    private readonly IRepositoryFactory _factory;
    private readonly ILogger<RepositoryPersistenceInterceptor> _logger;

    // cache للـ generic method info لتفادي الانعكاس المتكرر
    private static readonly MethodInfo CreateRepoMethod =
        typeof(IRepositoryFactory).GetMethod(nameof(IRepositoryFactory.CreateRepository))!;

    public string Name => "RepositoryPersistenceInterceptor";
    public InterceptorPhase Phase => InterceptorPhase.Post;

    public RepositoryPersistenceInterceptor(
        IRepositoryFactory factory,
        ILogger<RepositoryPersistenceInterceptor> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public bool AppliesTo(Operation op) => op.HasTag(PersistenceTags.PersistEntity.Name);

    public async Task<AnalyzerResult> InterceptAsync(OperationContext context, OperationResult? result = null)
    {
        var op = context.Operation;
        var entityTypeName = op.GetTagValue(PersistenceTags.EntityType.Name);
        if (string.IsNullOrEmpty(entityTypeName))
            return AnalyzerResult.Fail("persist_entity_type_missing");

        var contextKey = op.GetTagValue(PersistenceTags.ContextKey.Name) ?? "entity";
        var mode = op.GetTagValue(PersistenceTags.Mode.Name) ?? "add";

        if (!context.Items.TryGetValue(contextKey, out var entityObj) || entityObj == null)
            return AnalyzerResult.Warning($"no_entity_in_context_key:{contextKey}");

        var entityType = entityObj.GetType();
        if (!typeof(IBaseEntity).IsAssignableFrom(entityType))
            return AnalyzerResult.Fail($"entity_not_IBaseEntity:{entityType.Name}");

        // نُنشئ IBaseAsyncRepository<T> ديناميكياً عبر الانعكاس
        var genericMethod = CreateRepoMethod.MakeGenericMethod(entityType);
        var repo = genericMethod.Invoke(_factory, null);
        if (repo == null)
            return AnalyzerResult.Fail("repository_resolve_failed");

        var repoType = typeof(IBaseAsyncRepository<>).MakeGenericType(entityType);

        try
        {
            switch (mode.ToLowerInvariant())
            {
                case "add":
                    await InvokeAsync(repoType, repo, nameof(IBaseAsyncRepository<IBaseEntity>.AddAsync), entityObj, context.CancellationToken);
                    break;

                case "update":
                    await InvokeAsync(repoType, repo, nameof(IBaseAsyncRepository<IBaseEntity>.UpdateAsync), entityObj, context.CancellationToken);
                    break;

                case "soft_delete":
                    var softDelete = repoType.GetMethod(nameof(IBaseAsyncRepository<IBaseEntity>.SoftDeleteAsync),
                        new[] { entityType, typeof(CancellationToken) })!;
                    await (Task)softDelete.Invoke(repo, new[] { entityObj, context.CancellationToken })!;
                    break;

                default:
                    return AnalyzerResult.Fail($"unknown_persist_mode:{mode}");
            }

            _logger.LogDebug("[Persistence] {Mode} {Type} via interceptor", mode, entityType.Name);

            return new AnalyzerResult
            {
                Passed = true,
                Message = $"persisted_{mode}_{entityType.Name}",
                Data = new Dictionary<string, object>
                {
                    ["entity_type"] = entityType.Name,
                    ["mode"] = mode
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Persistence] Failed to {Mode} {Type}", mode, entityType.Name);
            return AnalyzerResult.Fail($"persistence_failed:{ex.Message}");
        }
    }

    private static async Task InvokeAsync(Type repoType, object repo, string methodName, object entity, CancellationToken ct)
    {
        var method = repoType.GetMethods()
            .First(m => m.Name == methodName
                     && m.GetParameters().Length == 2
                     && m.GetParameters()[1].ParameterType == typeof(CancellationToken));

        var task = (Task)method.Invoke(repo, new[] { entity, ct })!;
        await task;
    }
}
