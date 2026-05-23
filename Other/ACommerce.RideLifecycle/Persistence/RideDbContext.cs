using Microsoft.EntityFrameworkCore;
using System;

namespace ACommerce.RideLifecycle.Persistence
{
    public class RideDbContext : DbContext
    {
        public RideDbContext(DbContextOptions<RideDbContext> opts) : base(opts) { }

        public DbSet<RideEntity> Rides { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RideEntity>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.RiderId).HasMaxLength(200);
                b.Property(x => x.AssignedDriverId).HasMaxLength(200);
                b.Property(x => x.RatingFeedback).HasMaxLength(1000);
                b.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
