using FluentMigrator;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_006)]
public class AddCurrencyBaseMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        string[] codes =
        [
            "AUD", "AZN", "DZD", "GBP", "AMD", "BHD", "BYN", "BGN", "BOB", "BRL", "HUF", "VND", "HKD", "GEL", "DKK",
            "AED", "USD", "EUR", "EGP", "INR", "IDR", "IRR", "KZT", "CAD", "QAR", "KGS", "CNY", "CUP", "MDL", "MNT",
            "NGN", "NZD", "NOK", "OMR", "PLN", "SAR", "RON", "XDR", "SGD", "TJS", "THB", "BDT", "TRY", "TMT", "UZS",
            "UAH", "CZK", "SEK", "CHF", "ETB", "RSD", "ZAR", "KRW", "JPY", "MMK"
        ];

        foreach (var code in codes)
        {
            Insert.IntoTable("Currency")
                .Row(new
                {
                    Name = code,
                    Rate = 0
                });
        }
    }

    public override void Down()
    {
    }
}