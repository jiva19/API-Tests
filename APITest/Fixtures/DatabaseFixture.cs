//using AutoMapper;
using ClassLibrary1;


    using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.MsSql;


namespace AWMS2.IntegrationTest.Fixtures {
    public class DatabaseFixture : IAsyncDisposable
    {
        private MsSqlContainer _container = null!;
        public ExampleContext  DbContext { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            _container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Password123!")
                .Build();

            await _container.StartAsync();
        
            var connectionString =
                _container.GetConnectionString() + ";Database=pistola_tests";

            var options = new DbContextOptionsBuilder<ExampleContext>()
                .UseSqlServer(connectionString)
                .Options;
        

        
        
        
            DbContext = new ExampleContext(options);
  
        
        
            await DbContext.Database.EnsureCreatedAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _container.DisposeAsync();
        }
    }
}