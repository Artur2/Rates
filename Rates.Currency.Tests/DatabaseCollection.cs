using Microsoft.AspNetCore.Mvc.Testing;
using Rates.Currency;
using Xunit;

namespace Rates.Users.Tests;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<WebApplicationFactory<Startup>>
{
}