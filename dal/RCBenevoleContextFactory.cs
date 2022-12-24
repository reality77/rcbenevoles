using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace dal
{
    public class RCBenevoleContextFactory : IDesignTimeDbContextFactory<RCBenevoleContext>
    {
        public const string CONNECTION_STRING = "User ID=rcbenevoles;Password={1};Host={0};Port={2};Database=rcbenevoles;Pooling=true;";

        public RCBenevoleContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<RCBenevoleContext>();
            builder.UseNpgsql(GetConnectionString());

            var context = new RCBenevoleContext(builder.Options);
            return context;
        }


        public static string GetConnectionString()
        {
            var servername = Environment.GetEnvironmentVariable("APP_DB_SERVER");
            var passwordname = Environment.GetEnvironmentVariable("APP_DB_PASSWORD");
            var port = Environment.GetEnvironmentVariable("APP_DB_PORT");

            if (string.IsNullOrEmpty(servername))
                servername = "localhost";

            if (string.IsNullOrEmpty(passwordname))
                passwordname = "rcbenevoles";

            if (string.IsNullOrEmpty(port))
                port = "5432";

            return string.Format(CONNECTION_STRING, servername, passwordname, port);
        }
    }
}

