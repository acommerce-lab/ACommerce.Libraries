using Microsoft.EntityFrameworkCore;
using ACommerce.SharedKernel.Abstractions.Entities;
using System.Reflection;
using System.Diagnostics;

namespace ACommerce.SharedKernel.Infrastructure.EFCores.Context;

/// <summary>
/// ApplicationDbContext موحد مع Auto-Discovery للـ Entities
/// يكتشف تلقائياً جميع IBaseEntity من جميع المكتبات المحملة
/// </summary>
public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// 1. Auto-discover جميع IBaseEntity types من جميع Assemblies
		var entityTypes = DiscoverEntityTypes();
		Console.WriteLine(entityTypes.Count());
        foreach (var entityType in entityTypes)
		{
			Console.Write(entityType);
            if (string.IsNullOrWhiteSpace(entityType.Name))
            {
                throw new Exception($"❌ Entity type has empty name: {entityType.FullName}");
            }

            modelBuilder.Entity(entityType);
		}

		// 2. تطبيق Configurations إذا وجدت
		ApplyConfigurationsFromAssemblies(modelBuilder);
	}

	/// <summary>
	/// اكتشاف جميع الـ Types التي تنفذ IBaseEntity
	/// </summary>
	private IEnumerable<Type> DiscoverEntityTypes()
	{
		// الحصول على جميع Assemblies المحملة التي تبدأ بـ ACommerce
		var assemblies = AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => a.FullName?.StartsWith("ACommerce") == true)
			.ToList();

		var entityTypes = new List<Type>();

		foreach (var assembly in assemblies)
		{
			try
			{
				var types = assembly.GetTypes()
					.Where(t => typeof(IBaseEntity).IsAssignableFrom(t)
						&& t.IsClass
						&& !t.IsAbstract
						&& !t.IsGenericType
						&& t != typeof(IBaseEntity))
					.ToList();

				entityTypes.AddRange(types);
			}
			catch (ReflectionTypeLoadException)
			{
				// تجاهل assemblies التي لا يمكن تحميلها
				continue;
			}
		}

		return entityTypes.Distinct();
	}

	/// <summary>
	/// تطبيق IEntityTypeConfiguration من جميع Assemblies
	/// </summary>
	private void ApplyConfigurationsFromAssemblies(ModelBuilder modelBuilder)
	{
		var assemblies = AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => a.FullName?.StartsWith("ACommerce") == true);

		foreach (var assembly in assemblies)
		{
			try
			{
				modelBuilder.ApplyConfigurationsFromAssembly(assembly);
			}
			catch
			{
				// تجاهل إذا لم توجد configurations
				continue;
			}
		}
	}
}
