using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Rates.Domain.Entities;

namespace Rates.Background.Data;

public class BackgroundDataContext : DataConnection
{
    public BackgroundDataContext()
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