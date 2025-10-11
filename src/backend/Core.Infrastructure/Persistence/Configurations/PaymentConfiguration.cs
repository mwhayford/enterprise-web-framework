using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;

namespace Core.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasConversion(
                v => new { Amount = v.Amount, Currency = v.Currency },
                v => Core.Domain.ValueObjects.Money.Create(v.Amount, v.Currency));

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.PaymentMethodType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.StripePaymentIntentId)
            .HasMaxLength(100);

        builder.Property(p => p.StripeChargeId)
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.StripePaymentIntentId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
