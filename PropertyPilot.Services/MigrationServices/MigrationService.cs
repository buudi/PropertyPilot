using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using PropertyPilot.Services.MigrationServices.Models;
using System.Globalization;

namespace PropertyPilot.Services.MigrationServices;

public class MigrationService
{
    public async Task<List<MigrationRecord>> ParseCsvAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            Encoding = System.Text.Encoding.UTF8
        });

        csv.Context.RegisterClassMap<MigrationRecordMap>();

        var records = new List<MigrationRecord>();
        await foreach (var record in csv.GetRecordsAsync<MigrationRecord>())
        {
            records.Add(record);
        }

        return records;
    }
}