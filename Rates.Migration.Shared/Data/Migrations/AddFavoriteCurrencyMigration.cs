using FluentMigrator;
using Rates.Domain.Entities;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_004)]
public class AddFavoriteCurrencyMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        Create.Table(nameof(FavoriteCurrency))
            .WithColumn(nameof(FavoriteCurrency.CurrencyId)).AsInt32().ForeignKey(
                nameof(FavoriteCurrency.CurrencyId), nameof(Currency), nameof(Currency.Id))
            .WithColumn(nameof(FavoriteCurrency.UserId)).AsInt32().ForeignKey(
                nameof(FavoriteCurrency.UserId), nameof(User), nameof(User.Id));
    }

    public override void Down()
    {
    }
}