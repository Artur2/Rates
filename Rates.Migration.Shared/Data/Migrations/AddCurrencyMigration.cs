using FluentMigrator;
using Rates.Domain.Entities;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_002)]
public class AddCurrencyMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        Create.Table(nameof(Currency))
            .WithColumn(nameof(Currency.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(Currency.Rate)).AsDecimal().NotNullable()
            .WithColumn(nameof(Currency.Name)).AsString().NotNullable();
    }

    public override void Down()
    {
    }
}