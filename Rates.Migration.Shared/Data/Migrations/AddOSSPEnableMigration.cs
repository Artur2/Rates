using FluentMigrator;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_000)]
public class AddOSSPEnableMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
    }

    public override void Down()
    {
    }
}