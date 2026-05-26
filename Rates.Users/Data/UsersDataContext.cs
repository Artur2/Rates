using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using Rates.Domain.Entities;
using Rates.Shared.Data;

namespace Rates.Users.Data;

public class UsersDataContext : DataConnection
{
    public UsersDataContext(IDataOptionsProvider dataOptionsProvider) : base(dataOptionsProvider.GetDataOptions())
    {
        InlineParameters = true;
        var mappingScheme = new FluentMappingBuilder(MappingSchema);
        mappingScheme.Entity<User>()
            .HasTableName(nameof(User))
            .HasPrimaryKey(x => x.Id)
            .HasIdentity(x => x.Id);

        mappingScheme.Entity<LoginItem>()
            .HasTableName(nameof(LoginItem))
            .HasPrimaryKey(x => x.Id)
            .HasIdentity(x => x.Id);

        mappingScheme.Build();
    }

    public ITable<User> Users => this.GetTable<User>();

    public ITable<LoginItem> LoginItems => this.GetTable<LoginItem>();
}