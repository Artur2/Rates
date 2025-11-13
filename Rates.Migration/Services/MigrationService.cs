using FluentMigrator.Runner;
using Grpc.Core;

namespace Rates.Migration.Services;

public class MigrationService(IMigrationRunner migrationRunner) : Migration.MigrationService.MigrationServiceBase
{
    public override Task<MigrateResponse> Migrate(MigrateRequest request, ServerCallContext context)
    {
        migrationRunner.MigrateUp();
        return Task.FromResult(new MigrateResponse());
    }
}