using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Rates.Shared.Data;

public class SharedDataContext : DataConnection
{
    public SharedDataContext(IDataOptionsProvider dataOptionsProvider) : base(dataOptionsProvider.GetDataOptions())
    {
        InlineParameters = true;
        var mappingScheme = new FluentMappingBuilder(MappingSchema);

        mappingScheme.Entity<Domain.Entities.LoginItem>()
            .HasTableName(nameof(Domain.Entities.LoginItem))
            .HasPrimaryKey(x => x.Id)
            .HasIdentity(x => x.Id);

        mappingScheme.Build();
    }

    public ITable<Domain.Entities.LoginItem> LoginItems => this.GetTable<Domain.Entities.LoginItem>();
}