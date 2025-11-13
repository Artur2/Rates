using FluentMigrator;
using Rates.Domain.Entities;

namespace Rates.Migration.Data.Migrations;

[Migration(2025_11_05_003)]
public class AddLoginItemMigration : FluentMigrator.Migration
{
    public override void Up()
    {
        Create.Table(nameof(LoginItem))
            .WithColumn(nameof(LoginItem.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(LoginItem.Identifier)).AsString().NotNullable()
            .WithColumn(nameof(LoginItem.UserId)).AsInt32()
            .ForeignKey(nameof(LoginItem.UserId), nameof(User), nameof(User.Id))
            .NotNullable()
            .WithColumn(nameof(LoginItem.IsRevoked)).AsBoolean().WithDefaultValue(false)
            .WithColumn(nameof(LoginItem.RefreshToken)).AsString().NotNullable();
    }

    public override void Down()
    {
    }
}