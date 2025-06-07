using CsvHelper.Configuration;
using PropertyPilot.Services.MigrationServices.Models;

namespace PropertyPilot.Services.MigrationServices;

public sealed class MigrationRecordMap : ClassMap<MigrationRecord>
{
    public MigrationRecordMap()
    {
        Map(m => m.RoomIdentifier).Name("RoomIdentifier");

        Map(m => m.TenantName)
            .Name("TenantName")
            .Convert(args =>
            {
                var value = args.Row.GetField("TenantName");
                return string.IsNullOrWhiteSpace(value) ? null : value;
            });
        Map(m => m.Date).Name("Date").TypeConverterOption.Format("dd/MM/yyyy");

        Map(m => m.AssignedRent).Name("AssignedRent");

        Map(m => m.Bank).Name("Bank");

        Map(m => m.Cash).Name("Cash");

        Map(m => m.Remaining).Name("Remaining");
    }
}
