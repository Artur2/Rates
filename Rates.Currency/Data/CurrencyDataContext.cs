using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Rates.Currency.Data;

public class CurrencyDataContext : DataConnection
{
    public CurrencyDataContext()
    {
        InlineParameters = true;
        var mappingScheme = new FluentMappingBuilder(MappingSchema);
        mappingScheme.Entity<Domain.Entities.Currency>()
            .HasTableName(nameof(Domain.Entities.Currency))
            .HasPrimaryKey(x => x.Id)
            .HasIdentity(x => x.Id);

        mappingScheme.Entity<Domain.Entities.FavoriteCurrency>()
            .HasTableName(nameof(Domain.Entities.FavoriteCurrency));
        
        mappingScheme.Entity<Domain.Entities.User>()
            .HasTableName(nameof(Domain.Entities.User))
            .HasPrimaryKey(x => x.Id)
            .HasIdentity(x => x.Id);

        mappingScheme.Build();
    }

    public ITable<Domain.Entities.Currency> Currencies => this.GetTable<Domain.Entities.Currency>();

    public ITable<Domain.Entities.FavoriteCurrency> FavoriteCurrencies =>
        this.GetTable<Domain.Entities.FavoriteCurrency>();

    public ITable<Domain.Entities.User> Users => this.GetTable<Domain.Entities.User>();
}