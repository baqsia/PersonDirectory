using Task.PersonDirectory.Domain;
using Task.PersonDirectory.Domain.Aggregates;
using Task.PersonDirectory.Infrastructure.Context;

namespace Task.PersonDirectory.Infrastructure.Seed;

public static class CitySeeder
{
    public static void SeedAsync(PersonDirectoryContext context)
    {
        if (context.Set<City>().Any())
            return;

        var cities = new List<City>
        {
            new() { Name = "Tbilisi" },
            new() { Name = "Batumi" },
            new() { Name = "Kutaisi" },
        };

        context.Set<City>().AddRange(cities);
        context.SaveChanges();
    }
}