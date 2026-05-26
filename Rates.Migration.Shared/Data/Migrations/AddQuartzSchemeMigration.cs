using FluentMigrator;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_005)]
public class AddQuartzSchemeMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        var isTestEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";
        var fileName = !isTestEnv ? "quartz_scheme.sql" : "sqlite_quartz_scheme.sql";
        Execute.Script(
            $"Data{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}Assets{Path.DirectorySeparatorChar}{fileName}");
    }

    public override void Down()
    {
    }
}