using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Rates.Domain.Entities;
using Rates.Shared.Data;

namespace Rates.Background.Data;

public class BackgroundDataContext : DataConnection
{
    public BackgroundDataContext(IDataOptionsProvider dataOptionsProvider) : base(dataOptionsProvider.GetDataOptions())
    {
        InlineParameters = true;
        var mappingScheme = new FluentMappingBuilder(MappingSchema);
        mappingScheme.Entity<Currency>()
            .HasTableName(nameof(Currency))
            .HasPrimaryKey(x => x.Id)
            .HasIdentity(x => x.Id);

        mappingScheme.Build();
    }

    public ITable<Currency> Currencies => this.GetTable<Currency>();
}