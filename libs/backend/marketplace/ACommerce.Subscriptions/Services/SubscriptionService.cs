using ACommerce.Subscriptions.DTOs;
using ACommerce.Subscriptions.Entities;
using ACommerce.Subscriptions.Enums;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Subscriptions.Services;

/// <summary>
/// تنفيذ خدمة إدارة الاشتراكات
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly DbContext _context;

    public SubscriptionService(DbContext context)
    {
        _context = context;
    }

    #region Plans - الباقات

    public async Task<List<SubscriptionPlanDto>> GetPlansAsync(CancellationToken ct = default)
    {
        var plans = await _context.Set<SubscriptionPlan>()
            .Where(p => p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        return plans.Select(MapToPlanDto).ToList();
    }

    public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, ct);

        return plan != null ? MapToPlanDto(plan) : null;
    }

    public async Task<SubscriptionPlanDto?> GetPlanBySlugAsync(string slug, CancellationToken ct = default)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted, ct);

        return plan != null ? MapToPlanDto(plan) : null;
    }

    public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto, CancellationToken ct = default)
    {
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            NameEn = dto.NameEn,
            Slug = dto.Slug,
            Description = dto.Description,
            DescriptionEn = dto.DescriptionEn,
            Icon = dto.Icon,
            Color = dto.Color,
            SortOrder = dto.SortOrder,
            IsDefault = dto.IsDefault,
            IsRecommended = dto.IsRecommended,
            IsActive = true,

            MonthlyPrice = dto.MonthlyPrice,
            QuarterlyPrice = dto.QuarterlyPrice,
            SemiAnnualPrice = dto.SemiAnnualPrice,
            AnnualPrice = dto.AnnualPrice,
            Currency = dto.Currency,
            TrialDays = dto.TrialDays,
            GracePeriodDays = dto.GracePeriodDays,

            MaxListings = dto.MaxListings,
            MaxImagesPerListing = dto.MaxImagesPerListing,
            MaxFeaturedListings = dto.MaxFeaturedListings,
            StorageLimitMB = dto.StorageLimitMB,
            MaxTeamMembers = dto.MaxTeamMembers,
            MaxMonthlyMessages = dto.MaxMonthlyMessages,
            MaxMonthlyApiCalls = dto.MaxMonthlyApiCalls,
            ListingDurationDays = dto.ListingDurationDays,

            CommissionType = dto.CommissionType,
            CommissionPercentage = dto.CommissionPercentage,
            CommissionFixedAmount = dto.CommissionFixedAmount,
            MinCommission = dto.MinCommission,
            MaxCommission = dto.MaxCommission,

            HasVerifiedBadge = dto.HasVerifiedBadge,
            SearchPriorityBoost = dto.SearchPriorityBoost,
            AnalyticsLevel = dto.AnalyticsLevel,
            SupportLevel = dto.SupportLevel,
            AllowDirectMessages = dto.AllowDirectMessages,
            AllowApiAccess = dto.AllowApiAccess,
            AllowCustomStorePage = dto.AllowCustomStorePage,
            AllowPromotionalTools = dto.AllowPromotionalTools,
            AllowDataExport = dto.AllowDataExport,
            RemoveBranding = dto.RemoveBranding,
            EmailReports = dto.EmailReports,
            PushNotifications = dto.PushNotifications,

            CreatedAt = DateTime.UtcNow
        };

        _context.Set<SubscriptionPlan>().Add(plan);
        await _context.SaveChangesAsync(ct);

        return MapToPlanDto(plan);
    }

    public async Task<SubscriptionPlanDto?> UpdatePlanAsync(UpdateSubscriptionPlanDto dto, CancellationToken ct = default)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == dto.Id && !p.IsDeleted, ct);

        if (plan == null) return null;

        plan.Name = dto.Name;
        plan.NameEn = dto.NameEn;
        plan.Slug = dto.Slug;
        plan.Description = dto.Description;
        plan.DescriptionEn = dto.DescriptionEn;
        plan.Icon = dto.Icon;
        plan.Color = dto.Color;
        plan.SortOrder = dto.SortOrder;
        plan.IsDefault = dto.IsDefault;
        plan.IsRecommended = dto.IsRecommended;
        plan.IsActive = dto.IsActive;

        plan.MonthlyPrice = dto.MonthlyPrice;
        plan.QuarterlyPrice = dto.QuarterlyPrice;
        plan.SemiAnnualPrice = dto.SemiAnnualPrice;
        plan.AnnualPrice = dto.AnnualPrice;
        plan.Currency = dto.Currency;
        plan.TrialDays = dto.TrialDays;
        plan.GracePeriodDays = dto.GracePeriodDays;

        plan.MaxListings = dto.MaxListings;
        plan.MaxImagesPerListing = dto.MaxImagesPerListing;
        plan.MaxFeaturedListings = dto.MaxFeaturedListings;
        plan.StorageLimitMB = dto.StorageLimitMB;
        plan.MaxTeamMembers = dto.MaxTeamMembers;
        plan.MaxMonthlyMessages = dto.MaxMonthlyMessages;
        plan.MaxMonthlyApiCalls = dto.MaxMonthlyApiCalls;
        plan.ListingDurationDays = dto.ListingDurationDays;

        plan.CommissionType = dto.CommissionType;
        plan.CommissionPercentage = dto.CommissionPercentage;
        plan.CommissionFixedAmount = dto.CommissionFixedAmount;
        plan.MinCommission = dto.MinCommission;
        plan.MaxCommission = dto.MaxCommission;

        plan.HasVerifiedBadge = dto.HasVerifiedBadge;
        plan.SearchPriorityBoost = dto.SearchPriorityBoost;
        plan.AnalyticsLevel = dto.AnalyticsLevel;
        plan.SupportLevel = dto.SupportLevel;
        plan.AllowDirectMessages = dto.AllowDirectMessages;
        plan.AllowApiAccess = dto.AllowApiAccess;
        plan.AllowCustomStorePage = dto.AllowCustomStorePage;
        plan.AllowPromotionalTools = dto.AllowPromotionalTools;
        plan.AllowDataExport = dto.AllowDataExport;
        plan.RemoveBranding = dto.RemoveBranding;
        plan.EmailReports = dto.EmailReports;
        plan.PushNotifications = dto.PushNotifications;

        plan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return MapToPlanDto(plan);
    }

    public async Task<bool> DeletePlanAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == planId && !p.IsDeleted, ct);

        if (plan == null) return false;

        plan.IsDeleted = true;
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    #endregion

    #region Subscriptions - الاشتراكات

    public async Task<SubscriptionDto?> GetVendorSubscriptionAsync(Guid vendorId, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        return subscription != null ? MapToSubscriptionDto(subscription) : null;
    }

    public async Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, ct);

        return subscription != null ? MapToSubscriptionDto(subscription) : null;
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto dto, CancellationToken ct = default)
    {
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == dto.PlanId && !p.IsDeleted, ct)
            ?? throw new InvalidOperationException("Plan not found");

        var now = DateTime.UtcNow;
        var trialEnd = plan.TrialDays > 0 ? now.AddDays(plan.TrialDays) : (DateTime?)null;
        var periodEnd = trialEnd ?? now.AddMonths((int)dto.BillingCycle);
        var price = plan.GetPrice(dto.BillingCycle);

        // تحديد حالة الاشتراك:
        // - Trial: إذا كان هناك فترة تجريبية
        // - Active: إذا كانت الباقة مجانية
        // - PastDue: إذا كان الدفع مطلوباً (ينتظر إتمام الدفع)
        var status = plan.TrialDays > 0 ? SubscriptionStatus.Trial
                   : price > 0 ? SubscriptionStatus.PastDue
                   : SubscriptionStatus.Active;

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            VendorId = dto.VendorId,
            PlanId = dto.PlanId,
            Status = status,
            BillingCycle = dto.BillingCycle,
            StartDate = now,
            CurrentPeriodEnd = periodEnd,
            TrialEndDate = trialEnd,
            NextPaymentDate = trialEnd ?? periodEnd,

            Price = plan.GetPrice(dto.BillingCycle),
            Currency = plan.Currency,
            CommissionType = plan.CommissionType,
            CommissionPercentage = plan.CommissionPercentage,
            CommissionFixedAmount = plan.CommissionFixedAmount,

            MaxListings = plan.MaxListings,
            MaxImagesPerListing = plan.MaxImagesPerListing,
            MaxFeaturedListings = plan.MaxFeaturedListings,
            StorageLimitMB = plan.StorageLimitMB,

            AutoRenew = dto.AutoRenew,
            PaymentMethodId = dto.PaymentMethodId,
            BillingEmail = dto.BillingEmail,
            CouponCode = dto.CouponCode,

            CreatedAt = now
        };

        _context.Set<Subscription>().Add(subscription);

        // Add creation event
        var evt = SubscriptionEvent.Created(subscription.Id, dto.VendorId, dto.PlanId);
        _context.Set<SubscriptionEvent>().Add(evt);

        await _context.SaveChangesAsync(ct);

        subscription.Plan = plan;
        return MapToSubscriptionDto(subscription);
    }

    public async Task<SubscriptionDto?> ChangePlanAsync(Guid subscriptionId, ChangePlanDto dto, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, ct);

        if (subscription == null) return null;

        var newPlan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == dto.NewPlanId && !p.IsDeleted, ct);

        if (newPlan == null) return null;

        var oldPlanId = subscription.PlanId;
        var isUpgrade = newPlan.MonthlyPrice > (subscription.Plan?.MonthlyPrice ?? 0);

        subscription.PlanId = dto.NewPlanId;
        subscription.BillingCycle = dto.NewBillingCycle ?? subscription.BillingCycle;
        subscription.Price = newPlan.GetPrice(subscription.BillingCycle);
        subscription.CommissionType = newPlan.CommissionType;
        subscription.CommissionPercentage = newPlan.CommissionPercentage;
        subscription.CommissionFixedAmount = newPlan.CommissionFixedAmount;
        subscription.MaxListings = newPlan.MaxListings;
        subscription.MaxImagesPerListing = newPlan.MaxImagesPerListing;
        subscription.MaxFeaturedListings = newPlan.MaxFeaturedListings;
        subscription.StorageLimitMB = newPlan.StorageLimitMB;
        subscription.UpdatedAt = DateTime.UtcNow;

        // Add event
        var evt = isUpgrade
            ? SubscriptionEvent.Upgraded(subscriptionId, subscription.VendorId, oldPlanId, dto.NewPlanId)
            : SubscriptionEvent.Downgraded(subscriptionId, subscription.VendorId, oldPlanId, dto.NewPlanId);
        _context.Set<SubscriptionEvent>().Add(evt);

        await _context.SaveChangesAsync(ct);

        subscription.Plan = newPlan;
        return MapToSubscriptionDto(subscription);
    }

    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, CancelSubscriptionDto dto, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, ct);

        if (subscription == null) return false;

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.CancellationReason = dto.Reason;
        subscription.AutoRenew = false;
        subscription.UpdatedAt = DateTime.UtcNow;

        var evt = SubscriptionEvent.Cancelled(subscriptionId, subscription.VendorId, dto.Reason);
        _context.Set<SubscriptionEvent>().Add(evt);

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SubscriptionDto?> RenewSubscriptionAsync(Guid subscriptionId, RenewSubscriptionDto dto, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, ct);

        if (subscription == null) return null;

        var now = DateTime.UtcNow;
        var billingCycle = dto.BillingCycle ?? subscription.BillingCycle;
        var newPeriodEnd = now.AddMonths((int)billingCycle);

        subscription.Status = SubscriptionStatus.Active;
        subscription.BillingCycle = billingCycle;
        subscription.CurrentPeriodEnd = newPeriodEnd;
        subscription.NextPaymentDate = newPeriodEnd;
        subscription.LastPaymentDate = now;
        subscription.CancelledAt = null;
        subscription.CancellationReason = null;
        subscription.UpdatedAt = now;

        if (!string.IsNullOrEmpty(dto.CouponCode))
            subscription.CouponCode = dto.CouponCode;

        if (!string.IsNullOrEmpty(dto.PaymentMethodId))
            subscription.PaymentMethodId = dto.PaymentMethodId;

        var evt = SubscriptionEvent.Renewed(subscriptionId, subscription.VendorId, subscription.Price);
        _context.Set<SubscriptionEvent>().Add(evt);

        await _context.SaveChangesAsync(ct);
        return MapToSubscriptionDto(subscription);
    }

    public async Task<SubscriptionDto?> ActivateSubscriptionAsync(Guid subscriptionId, string? paymentId = null, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && !s.IsDeleted, ct);

        if (subscription == null) return null;

        // لا تفعّل إذا كان الاشتراك مُفعّل بالفعل أو ملغي
        if (subscription.Status == SubscriptionStatus.Active)
            return MapToSubscriptionDto(subscription);

        if (subscription.Status == SubscriptionStatus.Cancelled)
            return null;

        var now = DateTime.UtcNow;

        subscription.Status = SubscriptionStatus.Active;
        subscription.LastPaymentDate = now;
        subscription.PaymentMethodId = paymentId ?? subscription.PaymentMethodId;
        subscription.UpdatedAt = now;

        // تحديث نهاية الفترة بناءً على دورة الفوترة
        if (subscription.CurrentPeriodEnd <= now)
        {
            subscription.CurrentPeriodEnd = now.AddMonths((int)subscription.BillingCycle);
            subscription.NextPaymentDate = subscription.CurrentPeriodEnd;
        }

        var evt = SubscriptionEvent.Activated(subscriptionId, subscription.VendorId, subscription.Price, paymentId);
        _context.Set<SubscriptionEvent>().Add(evt);

        await _context.SaveChangesAsync(ct);
        return MapToSubscriptionDto(subscription);
    }

    public async Task<SubscriptionSummaryDto?> GetSubscriptionSummaryAsync(Guid vendorId, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        if (subscription == null) return null;

        return new SubscriptionSummaryDto
        {
            Id = subscription.Id,
            PlanName = subscription.Plan?.Name,
            PlanSlug = subscription.Plan?.Slug,
            Status = subscription.Status,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            DaysRemaining = subscription.DaysRemaining,
            ListingsUsed = subscription.CurrentListingsCount,
            ListingsLimit = subscription.MaxListings,
            IsUnlimitedListings = subscription.MaxListings == -1,
            CommissionPercentage = subscription.CommissionPercentage,
            CanUpgrade = subscription.Plan?.SortOrder < 4,
            IsExpiringSoon = subscription.DaysRemaining <= 7
        };
    }

    #endregion

    #region Usage - الاستخدام

    public async Task<CanAddListingResult> CanAddListingAsync(Guid vendorId, CancellationToken ct = default)
    {
        // أولاً: تحقق من وجود باقات في النظام
        // إذا لم توجد باقات، يسمح للجميع بإنشاء العروض (للتطبيقات بدون اشتراكات)
        var hasPlans = await _context.Set<SubscriptionPlan>()
            .AnyAsync(p => p.IsActive && !p.IsDeleted, ct);

        if (!hasPlans)
        {
            return new CanAddListingResult
            {
                CanAdd = true,
                Reason = null,
                CurrentCount = 0,
                MaxAllowed = -1, // غير محدود
                ShouldUpgrade = false,
                SubscriptionRequired = false
            };
        }

        // توجد باقات في النظام - يجب التحقق من اشتراك المستخدم
        var subscription = await _context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        if (subscription == null)
        {
            return new CanAddListingResult
            {
                CanAdd = false,
                Reason = "لا يوجد اشتراك نشط",
                CurrentCount = 0,
                MaxAllowed = 0,
                ShouldUpgrade = true,
                SubscriptionRequired = true
            };
        }

        if (!subscription.IsActive)
        {
            return new CanAddListingResult
            {
                CanAdd = false,
                Reason = "الاشتراك غير نشط أو منتهي",
                CurrentCount = subscription.CurrentListingsCount,
                MaxAllowed = subscription.MaxListings,
                ShouldUpgrade = true,
                SubscriptionRequired = true
            };
        }

        if (subscription.HasReachedListingsLimit)
        {
            return new CanAddListingResult
            {
                CanAdd = false,
                Reason = "وصلت للحد الأقصى من العروض المسموحة في باقتك",
                CurrentCount = subscription.CurrentListingsCount,
                MaxAllowed = subscription.MaxListings,
                ShouldUpgrade = true,
                SubscriptionRequired = false
            };
        }

        return new CanAddListingResult
        {
            CanAdd = true,
            CurrentCount = subscription.CurrentListingsCount,
            MaxAllowed = subscription.MaxListings,
            ShouldUpgrade = false,
            SubscriptionRequired = false
        };
    }

    public async Task<bool> IncrementListingsCountAsync(Guid vendorId, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        if (subscription == null || !subscription.CanAddListing()) return false;

        subscription.CurrentListingsCount++;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DecrementListingsCountAsync(Guid vendorId, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        if (subscription == null) return false;

        if (subscription.CurrentListingsCount > 0)
            subscription.CurrentListingsCount--;

        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateStorageUsageAsync(Guid vendorId, decimal storageMB, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        if (subscription == null) return false;

        subscription.CurrentStorageUsedMB = storageMB;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<VendorUsageStatsDto?> GetUsageStatsAsync(Guid vendorId, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        if (subscription == null) return null;

        return new VendorUsageStatsDto
        {
            VendorId = vendorId,
            SubscriptionId = subscription.Id,
            PlanName = subscription.Plan?.Name,
            ListingsUsed = subscription.CurrentListingsCount,
            ListingsLimit = subscription.MaxListings,
            ListingsUsagePercentage = subscription.ListingsUsagePercentage,
            FeaturedUsed = subscription.CurrentFeaturedListingsCount,
            FeaturedLimit = subscription.MaxFeaturedListings,
            StorageUsedMB = subscription.CurrentStorageUsedMB,
            StorageLimitMB = subscription.StorageLimitMB,
            StorageUsagePercentage = subscription.StorageLimitMB <= 0 ? 0 :
                Math.Min(100, subscription.CurrentStorageUsedMB / subscription.StorageLimitMB * 100),
            MessagesThisMonth = subscription.CurrentMonthMessages,
            MessagesLimit = subscription.Plan?.MaxMonthlyMessages ?? -1,
            ApiCallsThisMonth = subscription.CurrentMonthApiCalls,
            ApiCallsLimit = subscription.Plan?.MaxMonthlyApiCalls ?? 0,
            PeriodStart = subscription.StartDate,
            PeriodEnd = subscription.CurrentPeriodEnd,
            LastResetDate = subscription.LastUsageResetDate
        };
    }

    public async Task<CommissionCalculationDto> CalculateCommissionAsync(Guid vendorId, decimal amount, CancellationToken ct = default)
    {
        var subscription = await _context.Set<Subscription>()
            .FirstOrDefaultAsync(s => s.VendorId == vendorId && !s.IsDeleted, ct);

        if (subscription == null)
        {
            // Default commission if no subscription
            var commission = amount * 0.15m;
            return new CommissionCalculationDto
            {
                TransactionAmount = amount,
                CommissionAmount = commission,
                VendorAmount = amount - commission,
                CommissionType = CommissionType.Percentage,
                CommissionPercentage = 15,
                CommissionFixedAmount = 0
            };
        }

        var commissionAmount = subscription.CalculateCommission(amount);
        return new CommissionCalculationDto
        {
            TransactionAmount = amount,
            CommissionAmount = commissionAmount,
            VendorAmount = amount - commissionAmount,
            CommissionType = subscription.CommissionType,
            CommissionPercentage = subscription.CommissionPercentage,
            CommissionFixedAmount = subscription.CommissionFixedAmount
        };
    }

    #endregion

    #region Invoices - الفواتير

    public async Task<List<InvoiceSummaryDto>> GetVendorInvoicesAsync(Guid vendorId, CancellationToken ct = default)
    {
        var invoices = await _context.Set<SubscriptionInvoice>()
            .Where(i => i.VendorId == vendorId && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

        return invoices.Select(i => new InvoiceSummaryDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            Status = i.Status,
            DueDate = i.DueDate,
            Total = i.Total,
            Currency = i.Currency,
            IsPaid = i.IsPaid,
            IsOverdue = i.IsOverdue,
            CreatedAt = i.CreatedAt
        }).ToList();
    }

    public async Task<SubscriptionInvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _context.Set<SubscriptionInvoice>()
            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted, ct);

        if (invoice == null) return null;

        return new SubscriptionInvoiceDto
        {
            Id = invoice.Id,
            SubscriptionId = invoice.SubscriptionId,
            VendorId = invoice.VendorId,
            InvoiceNumber = invoice.InvoiceNumber,
            Status = invoice.Status,
            BillingCycle = invoice.BillingCycle,
            PeriodStart = invoice.PeriodStart,
            PeriodEnd = invoice.PeriodEnd,
            DueDate = invoice.DueDate,
            CreatedAt = invoice.CreatedAt,
            Subtotal = invoice.Subtotal,
            DiscountAmount = invoice.DiscountAmount,
            DiscountCode = invoice.DiscountCode,
            TaxRate = invoice.TaxRate,
            TaxAmount = invoice.TaxAmount,
            Total = invoice.Total,
            AmountPaid = invoice.AmountPaid,
            AmountDue = invoice.AmountDue,
            Currency = invoice.Currency,
            PaymentMethod = invoice.PaymentMethod,
            PaidAt = invoice.PaidAt,
            PlanName = invoice.PlanName,
            LineItemDescription = invoice.LineItemDescription,
            CustomerName = invoice.CustomerName,
            CustomerEmail = invoice.CustomerEmail,
            IsPaid = invoice.IsPaid,
            IsOverdue = invoice.IsOverdue,
            PdfUrl = invoice.PdfUrl
        };
    }

    #endregion

    #region Mapping Helpers

    private static SubscriptionPlanDto MapToPlanDto(SubscriptionPlan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        NameEn = plan.NameEn,
        Slug = plan.Slug,
        Description = plan.Description,
        DescriptionEn = plan.DescriptionEn,
        Icon = plan.Icon,
        Color = plan.Color,
        SortOrder = plan.SortOrder,
        IsDefault = plan.IsDefault,
        IsRecommended = plan.IsRecommended,
        MonthlyPrice = plan.MonthlyPrice,
        QuarterlyPrice = plan.QuarterlyPrice,
        SemiAnnualPrice = plan.SemiAnnualPrice,
        AnnualPrice = plan.AnnualPrice,
        Currency = plan.Currency,
        TrialDays = plan.TrialDays,
        MaxListings = plan.MaxListings,
        MaxImagesPerListing = plan.MaxImagesPerListing,
        MaxFeaturedListings = plan.MaxFeaturedListings,
        StorageLimitMB = plan.StorageLimitMB,
        MaxTeamMembers = plan.MaxTeamMembers,
        ListingDurationDays = plan.ListingDurationDays,
        CommissionType = plan.CommissionType,
        CommissionPercentage = plan.CommissionPercentage,
        CommissionFixedAmount = plan.CommissionFixedAmount,
        HasVerifiedBadge = plan.HasVerifiedBadge,
        SearchPriorityBoost = plan.SearchPriorityBoost,
        AnalyticsLevel = plan.AnalyticsLevel,
        SupportLevel = plan.SupportLevel,
        AllowDirectMessages = plan.AllowDirectMessages,
        AllowApiAccess = plan.AllowApiAccess,
        AllowCustomStorePage = plan.AllowCustomStorePage,
        AllowPromotionalTools = plan.AllowPromotionalTools,
        AllowDataExport = plan.AllowDataExport,
        RemoveBranding = plan.RemoveBranding
    };

    private static SubscriptionDto MapToSubscriptionDto(Subscription sub) => new()
    {
        Id = sub.Id,
        VendorId = sub.VendorId,
        PlanId = sub.PlanId,
        Plan = sub.Plan != null ? new SubscriptionPlanSummaryDto
        {
            Id = sub.Plan.Id,
            Name = sub.Plan.Name,
            NameEn = sub.Plan.NameEn,
            Slug = sub.Plan.Slug,
            Icon = sub.Plan.Icon,
            Color = sub.Plan.Color,
            MonthlyPrice = sub.Plan.MonthlyPrice,
            Currency = sub.Plan.Currency,
            MaxListings = sub.Plan.MaxListings,
            CommissionPercentage = sub.Plan.CommissionPercentage,
            IsRecommended = sub.Plan.IsRecommended,
            IsDefault = sub.Plan.IsDefault
        } : null,
        Status = sub.Status,
        BillingCycle = sub.BillingCycle,
        StartDate = sub.StartDate,
        CurrentPeriodEnd = sub.CurrentPeriodEnd,
        TrialEndDate = sub.TrialEndDate,
        CancelledAt = sub.CancelledAt,
        NextPaymentDate = sub.NextPaymentDate,
        Price = sub.Price,
        Currency = sub.Currency,
        CommissionPercentage = sub.CommissionPercentage,
        CommissionFixedAmount = sub.CommissionFixedAmount,
        CommissionType = sub.CommissionType,
        MaxListings = sub.MaxListings,
        MaxImagesPerListing = sub.MaxImagesPerListing,
        MaxFeaturedListings = sub.MaxFeaturedListings,
        StorageLimitMB = sub.StorageLimitMB,
        CurrentListingsCount = sub.CurrentListingsCount,
        CurrentFeaturedListingsCount = sub.CurrentFeaturedListingsCount,
        CurrentStorageUsedMB = sub.CurrentStorageUsedMB,
        AutoRenew = sub.AutoRenew,
        IsActive = sub.IsActive,
        IsInTrial = sub.IsInTrial,
        DaysRemaining = sub.DaysRemaining,
        ListingsUsagePercentage = sub.ListingsUsagePercentage,
        HasReachedListingsLimit = sub.HasReachedListingsLimit,
        CouponCode = sub.CouponCode,
        DiscountPercentage = sub.DiscountPercentage,
        DiscountEndDate = sub.DiscountEndDate
    };

    #endregion
}
