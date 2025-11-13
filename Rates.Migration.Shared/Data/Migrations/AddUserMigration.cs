using FluentMigrator;
using Rates.Domain.Entities;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_001)]
public class AddUserMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        Create.Table(nameof(User))
            .WithColumn(nameof(User.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(User.PasswordHash)).AsBinary().NotNullable()
            .WithColumn(nameof(User.Name)).AsString().NotNullable().Unique();
    }

    public override void Down()
    {
    }
}