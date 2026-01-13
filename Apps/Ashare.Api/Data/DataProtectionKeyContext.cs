using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ashare.Api.Data;

/// <summary>
/// DbContext for storing Data Protection keys in the database
/// هذا السياق مخصص لتخزين مفاتيح حماية البيانات في قاعدة البيانات
/// </summary>
public class DataProtectionKeyContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionKeyContext(DbContextOptions<DataProtectionKeyContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Data Protection Keys table
    /// جدول مفاتيح حماية البيانات
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}
