using LinqToDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rates.Background.Data;
using Rates.Background.Models;
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

        await service.ProcessNewRecords([new CbrEntry("TEST", 3d)], default);

        var record = await dataContext.Currencies.SingleOrDefaultAsync(x => x.Name == "TEST");
        Assert.NotNull(record);
        Assert.Equal(3m, record.Rate);
    }

    [Fact]
    public async Task Should_Not_Update_Different_Currency()
    {
        ConfigureFactory();
        MigrateUp();

        var service = Factory.Services.GetRequiredService<IBackgroundService>();
        var dataContext = Factory.Services.GetRequiredService<BackgroundDataContext>();
        await dataContext.Currencies.DeleteAsync();
        
        await dataContext.Currencies.InsertAsync(() => new Currency
        {
            Name = "RUB",
            Rate = 2
        });

        await service.ProcessNewRecords([new CbrEntry("TEST", 3d)], default);

        var record = await dataContext.Currencies.SingleOrDefaultAsync(x => x.Name == "RUB");
        Assert.NotNull(record);
        Assert.Equal(2m, record.Rate);
    }

    [Fact]
    public async Task One_Record_Should_Be_Updated_Other_Skipped()
    {
        ConfigureFactory();
        MigrateUp();

        var service = Factory.Services.GetRequiredService<IBackgroundService>();
        var dataContext = Factory.Services.GetRequiredService<BackgroundDataContext>();
        await dataContext.Currencies.DeleteAsync();

        await dataContext.Currencies.InsertAsync(() => new Currency
        {
            Name = "RUB",
            Rate = 2
        });

        await dataContext.Currencies.InsertAsync(() => new Currency
        {
            Name = "USD",
            Rate = 20
        });

        await service.ProcessNewRecords([new CbrEntry("USD", 30d)], default);

        var record = await dataContext.Currencies.SingleOrDefaultAsync(x => x.Name == "USD");
        Assert.NotNull(record);
        Assert.Equal(30m, record.Rate);
    }

    [Fact]
    public async Task Should_Correctly_Parse_Input_Stream()
    {
        await using var fs = new FileStream("Meta/XML_daily.asp.xml", FileMode.Open);
        
        ConfigureFactory();
        MigrateUp();

        var service = Factory.Services.GetRequiredService<IBackgroundService>();
        var items = await service.ParseCbrRecords(fs, default);
        Assert.NotNull(items);
        Assert.Equal(2, items.Length);
    }
}