using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MediaOrganizerApp.Application.Factories
{
    public static class ConnectionManager
    {
        private static IConfiguration _config;

        /// <summary>
        /// Define a configuração a ser usada por GetConnection().
        /// Se não for chamada, o método cai no comportamento original (appsettings.json apenas).
        /// Deve ser chamada no arranque de cada host (WebApi/Program.cs e Web.Blazor/Program.cs),
        /// antes de qualquer chamada a GetConnection().
        /// </summary>
        public static void Configure(IConfiguration configuration)
        {
            _config = configuration;
        }

        public static IDbConnection GetConnection(string connName = "SqlServerCon")
        {
            IDbConnection connection;

            // Se houver configuração injetada (env vars, appsettings.Production, etc.), usa-a
            if (_config != null)
            {
                string? connectionString = _config.GetConnectionString(connName);
                if (connectionString != null)
                {
                    connection = new SqlConnection(connectionString);
                    return connection;
                }
            }

            // Fallback: comportamento original (apenas appsettings.json)
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            string fallbackConnectionString = configurationRoot.GetConnectionString(connName);
            connection = new SqlConnection(fallbackConnectionString!);

            return connection;
        }
    }
}
