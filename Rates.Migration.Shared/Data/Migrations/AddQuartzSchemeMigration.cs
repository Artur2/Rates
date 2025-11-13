using FluentMigrator;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_005)]
public class AddQuartzSchemeMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        Execute.Script(
            $"Data{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}Assets{Path.DirectorySeparatorChar}quartz_scheme.sql");
    }

    public override void Down()
    {
    }
}