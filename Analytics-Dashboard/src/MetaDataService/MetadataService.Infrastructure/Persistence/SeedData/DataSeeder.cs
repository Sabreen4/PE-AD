using MetaDataService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaDataService.Infrastructure.Persistence;

/// <summary>
/// Seeds the database with reference data from CSV files.
/// </summary>
public class DataSeeder
{
    private readonly MetaDataDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private static readonly Guid SystemActorId = Guid.Parse("ffcb041f-f454-4499-b3e3-75bcea089b16");

    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeeder"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public DataSeeder(MetaDataDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Executes the full seeding process: RefSets, RefTerms, and SetRefTerms.
    /// </summary>
    /// <param name="seedDataPath">The directory path containing the CSV seed files.</param>
    public async Task SeedAsync(string seedDataPath)
    {
        _logger.LogInformation("========== DATA SEEDING STARTED ==========");

        await SeedRefSetsAsync(seedDataPath);
        await SeedRefTermsAsync(seedDataPath);
        await SeedSetRefTermsAsync(seedDataPath);

        _logger.LogInformation("========== DATA SEEDING COMPLETED ==========");
    }

    private async Task SeedRefSetsAsync(string seedDataPath)
    {
        string csvPath = Path.Combine(seedDataPath, "RefSets.csv");

        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("RefSets.csv NOT FOUND at {Path}", csvPath);
            return;
        }

        _logger.LogInformation("Loading RefSets.csv");

        string[] lines = await File.ReadAllLinesAsync(csvPath);

        int inserted = 0;
        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(',');

            if (parts.Length < 3)
                continue;

            Guid id = Guid.Parse(parts[0].Trim());
            string name = parts[1].Trim();
            string description = parts[2].Trim();

            if (await _context.RefSets.AnyAsync(x => x.Id == id))
                continue;

            _context.RefSets.Add(
                new RefSet
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    CreatedAt = SeedDate,
                    UpdatedAt = SeedDate,
                    CreatedBy = SystemActorId,
                    UpdatedBy = SystemActorId,
                    IsActive = true,
                }
            );
            inserted++;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Inserted {Inserted} RefSets", inserted);
    }

    private async Task SeedRefTermsAsync(string seedDataPath)
    {
        string csvPath = Path.Combine(seedDataPath, "RefTerms.csv");

        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("RefTerms.csv NOT FOUND at {Path}", csvPath);
            return;
        }

        _logger.LogInformation("Loading RefTerms.csv");

        string[] lines = await File.ReadAllLinesAsync(csvPath);

        int inserted = 0;
        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(',');

            if (parts.Length < 3)
                continue;

            Guid id = Guid.Parse(parts[0].Trim());
            string name = parts[1].Trim();
            string description = parts[2].Trim();

            RefTerm? existing = await _context.RefTerms.FirstOrDefaultAsync(x => x.Id == id);
            if (existing is not null)
            {
                if (existing.Name != name || existing.Description != description)
                {
                    existing.Name = name;
                    existing.Description = description;
                    existing.UpdatedAt = SeedDate;
                    existing.UpdatedBy = SystemActorId;
                    inserted++;
                }
                continue;
            }

            _context.RefTerms.Add(
                new RefTerm
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    CreatedAt = SeedDate,
                    UpdatedAt = SeedDate,
                    CreatedBy = SystemActorId,
                    UpdatedBy = SystemActorId,
                    IsActive = true,
                }
            );
            inserted++;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Inserted {Inserted} RefTerms", inserted);
    }

    private async Task SeedSetRefTermsAsync(string seedDataPath)
    {
        string csvPath = Path.Combine(seedDataPath, "SetRefTerms.csv");

        if (!File.Exists(csvPath))
        {
            _logger.LogWarning("SetRefTerms.csv NOT FOUND at {Path}", csvPath);
            return;
        }

        _logger.LogInformation("Loading SetRefTerms.csv");

        string[] lines = await File.ReadAllLinesAsync(csvPath);

        int inserted = 0;
        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(',');

            if (parts.Length < 3)
                continue;

            Guid refSetId = Guid.Parse(parts[1].Trim());
            Guid refTermId = Guid.Parse(parts[2].Trim());

            if (await _context.SetRefTerms.AnyAsync(x => x.RefSetId == refSetId && x.RefTermId == refTermId))
                continue;

            _context.SetRefTerms.Add(
                new SetRefTerm
                {
                    Id = Guid.NewGuid(),
                    RefSetId = refSetId,
                    RefTermId = refTermId,
                    CreatedAt = SeedDate,
                    UpdatedAt = SeedDate,
                    CreatedBy = SystemActorId,
                    UpdatedBy = SystemActorId,
                    IsActive = true,
                }
            );
            inserted++;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Inserted {Inserted} mappings", inserted);
    }
}
