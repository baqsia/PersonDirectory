using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Task.PersonDirectory.Domain;

namespace Task.PersonDirectory.Infrastructure.Configurations;

public class RelatedPersonConfiguration : IEntityTypeConfiguration<RelatedPerson>
{
    public void Configure(EntityTypeBuilder<RelatedPerson> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ConnectionType)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(x => x.Person)
            .WithMany(p => p.RelatedPersons)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Cascade); // ok to cascade

        builder.HasOne(x => x.RelatedTo)
            .WithMany()
            .HasForeignKey(x => x.RelatedToId)
            .OnDelete(DeleteBehavior.Restrict); // prevent multiple cascade path
 
        builder.HasIndex(rp => rp.PersonId);
        builder.HasIndex(rp => rp.RelatedToId);
    }
}
