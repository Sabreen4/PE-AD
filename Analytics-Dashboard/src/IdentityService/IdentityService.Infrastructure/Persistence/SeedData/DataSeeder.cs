using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// Seeds initial reference data (roles, features, role-feature mappings) from embedded CSV resources.
/// </summary>
public class DataSeeder
{
    private readonly IdentityDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private static readonly Guid SystemActorId = Guid.Parse("ffcb041f-f454-4499-b3e3-75bcea089b16");

    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeeder"/> class.
    /// </summary>
    public DataSeeder(IdentityDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds roles, features, and role-feature mappings into the database.
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("========== DATA SEEDING STARTED ==========");

        await SeedRolesAsync();
        await SeedFeaturesAsync();
        await SeedRoleFeaturesAsync();

        _logger.LogInformation("========== DATA SEEDING COMPLETED ==========");
    }

    /// <summary>
    /// Reads a CSV embedded resource and returns its rows as string arrays.
    /// </summary>
    private async Task<List<string[]>> ReadCsvAsync(string resourceName)
    {
        Assembly assembly = typeof(DataSeeder).Assembly;
        string fullName = $"IdentityService.Infrastructure.Persistence.SeedData.{resourceName}";
        using Stream? stream = assembly.GetManifestResourceStream(fullName);
        if (stream is null)
        {
            _logger.LogWarning("{Resource} NOT FOUND as embedded resource.", resourceName);
            return [];
        }
        using StreamReader reader = new StreamReader(stream);
        string content = await reader.ReadToEndAsync();

        return content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(line => line.TrimEnd('\r').Split(','))
            .ToList();
    }

    /// <summary>
    /// Seeds roles from the Roles.csv resource.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        List<string[]> rows = await ReadCsvAsync("Roles.csv");

        foreach (var parts in rows)
        {
            if (parts.Length < 2)
                continue;

            Guid id = Guid.Parse(parts[0].Trim());
            string name = parts[1].Trim();

            if (await _context.Roles.AnyAsync(x => x.Id == id))
                continue;

            _context.Roles.Add(
                new Role
                {
                    Id = id,
                    Name = name,
                    CreatedAt = SeedDate,
                    UpdatedAt = SeedDate,
                    CreatedBy = SystemActorId,
                    UpdatedBy = SystemActorId,
                    IsActive = true,
                }
            );
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Roles Count : {Count}", await _context.Roles.IgnoreQueryFilters().CountAsync()
        );
    }

    /// <summary>
    /// Seeds features from the Features.csv resource.
    /// </summary>
    private async Task SeedFeaturesAsync()
    {
        List<string[]> rows = await ReadCsvAsync("Features.csv");

        foreach (var parts in rows)
        {
            if (parts.Length < 3)
                continue;

            Guid id = Guid.Parse(parts[0].Trim());
            string name = parts[1].Trim();
            string key = parts[2].Trim();

            if (await _context.Features.AnyAsync(x => x.Id == id))
                continue;

            _context.Features.Add(
                new Feature
                {
                    Id = id,
                    Name = name,
                    Key = key,
                    CreatedAt = SeedDate,
                    UpdatedAt = SeedDate,
                    CreatedBy = SystemActorId,
                    UpdatedBy = SystemActorId,
                    IsActive = true,
                }
            );
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Features Count : {Count}", await _context.Features.IgnoreQueryFilters().CountAsync()
        );
    }

    /// <summary>
    /// Seeds role-feature mappings from the RoleFeature.csv resource.
    /// </summary>
    private async Task SeedRoleFeaturesAsync()
    {
        List<string[]> rows = await ReadCsvAsync("RoleFeature.csv");

        foreach (var parts in rows)
        {
            if (parts.Length < 3)
                continue;

            Guid roleId = Guid.Parse(parts[1].Trim());
            Guid featureId = Guid.Parse(parts[2].Trim());

            if (
                await _context.RoleFeatures.AnyAsync(x =>
                    x.RoleId == roleId && x.FeatureId == featureId
                )
            )
                continue;

            _context.RoleFeatures.Add(
                new RoleFeature
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    FeatureId = featureId,
                    CreatedAt = SeedDate,
                    UpdatedAt = SeedDate,
                    CreatedBy = SystemActorId,
                    UpdatedBy = SystemActorId,
                    IsActive = true,
                }
            );
        }

        int affected = await _context.SaveChangesAsync();
        _logger.LogInformation("Rows inserted: {Affected}", affected);

        _logger.LogInformation(
            "RoleFeatures Count : {Count}", await _context.RoleFeatures.IgnoreQueryFilters().CountAsync()
        );
    }
}
