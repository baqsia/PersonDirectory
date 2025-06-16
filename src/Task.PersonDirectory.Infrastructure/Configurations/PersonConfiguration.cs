using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;

namespace Task.PersonDirectory.Infrastructure.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Gender)
            .IsRequired();

        builder.Property(p => p.PersonalNumber)
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(p => p.DateOfBirth)
            .IsRequired();

        builder.Property(p => p.ImagePath)
            .HasMaxLength(255);

        builder.HasOne(p => p.City)
            .WithMany()
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Restrict);
 
        builder.HasMany(p => p.RelatedPersons)
            .WithOne(rp => rp.Person)
            .HasForeignKey(rp => rp.PersonId);
        
        builder.HasIndex(p => new { p.Gender, p.CityId, p.DateOfBirth });
        builder.HasIndex(p => p.PersonalNumber);
    }
}