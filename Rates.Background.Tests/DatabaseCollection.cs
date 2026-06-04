using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Rates.Background.Tests;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<WebApplicationFactory<Startup>>
{
}