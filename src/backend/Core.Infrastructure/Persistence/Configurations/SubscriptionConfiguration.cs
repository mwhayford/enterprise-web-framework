using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.PlanId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Amount)
            .IsRequired()
            .HasConversion(
                v => new { Amount = v.Amount, Currency = v.Currency },
                v => Core.Domain.ValueObjects.Money.Create(v.Amount, v.Currency));

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.StripeSubscriptionId)
            .HasMaxLength(100);

        builder.Property(s => s.StripeCustomerId)
            .HasMaxLength(100);

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.StripeSubscriptionId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CreatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
