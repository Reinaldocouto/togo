using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Togo.Infrastructure.Persistence;

namespace Togo.Infrastructure.Tests.Support;

public static class SqliteAppDbContextFactory
{
    public static AppDbContext CreateContext(out SqliteConnection connection)
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
