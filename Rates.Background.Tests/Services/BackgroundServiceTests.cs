using LinqToDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rates.Background.Data;
using Rates.Background.Services;
using Rates.Domain.Entities;
using Xunit;

namespace Rates.Background.Tests.Services;

public class BackgroundServiceTests(WebApplicationFactory<Startup> factory) : TestClassBase(factory)
{
    [Fact]
    public async Task Should_Update_Rate_Of_One_Record()
    {
        ConfigureFactory();
        MigrateUp();

        var service = Factory.Services.GetRequiredService<IBackgroundService>();
        var dataContext = Factory.Services.GetRequiredService<BackgroundDataContext>();
        await dataContext.Currencies.DeleteAsync();

        await dataContext.Currencies.InsertAsync(() => new Currency
        {
            Name = "TEST",
            Rate = 2
        });

        await service.ProcessNewRecords([("TEST", 3d)], default);

        var record = await dataContext.Currencies.SingleOrDefaultAsync(x => x.Name == "TEST");
        Assert.NotNull(record);
        Assert.Equal(3m, record.Rate);
    }
}