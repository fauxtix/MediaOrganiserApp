using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MediaOrganizerApp.Application.Factories
{
    public static class ConnectionManager
    {
        private static IConfiguration _config;
        public static IDbConnection GetConnection(string connName = "SqlServerCon")
        {
            IDbConnection connection;
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string connectionString = configuration.GetConnectionString(connName);
            connection = new SqlConnection(connectionString);

            return connection;
        }
    }
}
