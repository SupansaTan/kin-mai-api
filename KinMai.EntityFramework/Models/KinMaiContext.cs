using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using KinMai.Common.Resolver;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KinMai.EntityFramework.Models;

public partial class KinMaiContext : DbContext
{
    public KinMaiContext()
    {
    }

    public KinMaiContext(DbContextOptions<KinMaiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BusinessHour> BusinessHours { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<FavoriteRestaurant> FavoriteRestaurants { get; set; }

    public virtual DbSet<Related> Relateds { get; set; }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<SocialContact> SocialContacts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(ConnectionResolver.KinMaiConnection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessHour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("business_hours_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Restaurant).WithMany(p => p.BusinessHours)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("restaurant_fk");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<FavoriteRestaurant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("favoriterestaurant_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Restaurant).WithMany(p => p.FavoriteRestaurants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("favoriterestaurant_fk_1");

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteRestaurants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("favoriterestaurant_fk");
        });

        modelBuilder.Entity<Related>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("related_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Categories).WithMany(p => p.Relateds)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("categories_fk");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Relateds)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("restaurant_fk");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurant_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.DeliveryType).Metadata.SetValueComparer(
                    new ValueComparer<int[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()
                    ));

            entity.Property(e => e.PaymentMethod).Metadata.SetValueComparer(
                    new ValueComparer<int[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()
                    ));

            entity.Property(e => e.ImageLink).Metadata.SetValueComparer(
                    new ValueComparer<string[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()
                    ));

            entity.HasOne(d => d.Owner).WithMany(p => p.Restaurants)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_fk");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reviewer_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.ImageLink).Metadata.SetValueComparer(
                    new ValueComparer<string[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()
                    ));

            entity.Property(e => e.FoodRecommendList).Metadata.SetValueComparer(
                    new ValueComparer<string[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()
                    ));

            entity.Property(e => e.ReviewLabelRecommend).Metadata.SetValueComparer(
                    new ValueComparer<int[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()
                    ));

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("restaurant_fk");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("user_fk");
        });

        modelBuilder.Entity<SocialContact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("social_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Restaurant).WithMany(p => p.SocialContacts)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("restaurant_fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pk");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserType).HasDefaultValueSql("1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
