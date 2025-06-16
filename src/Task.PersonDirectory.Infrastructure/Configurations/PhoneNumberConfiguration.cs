using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;

namespace Task.PersonDirectory.Infrastructure.Configurations;

public class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Number)
            .IsRequired()
            .HasMaxLength(50);
    }
}