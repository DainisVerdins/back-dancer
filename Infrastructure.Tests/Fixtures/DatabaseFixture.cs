using Infrastructure.Persistance.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public DbContextOptions<DataContext> Options { get; }

    public DatabaseFixture()
    {
        // creating connection in memory
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        Options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .Options;

        // creating schema in db
        using var context = new DataContext(Options);
        context.Database.EnsureCreated();
    }

    public DataContext CreateContext() => new DataContext(Options);

    public void Dispose() => _connection.Dispose();
}
