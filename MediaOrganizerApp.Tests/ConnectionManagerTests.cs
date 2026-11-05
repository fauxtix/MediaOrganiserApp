using MediaOrganizerApp.Application.Factories;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MediaOrganizerApp.Tests;

/// <summary>
/// Garante que o ConnectionManager respeita a IConfiguration do host (env vars,
/// appsettings.{Environment}.json) quando Configure() é chamado — o que resolve
/// a ligação à BD dentro do Docker (ConnectionStrings__SqlServerCon=Server=sql,1433).
/// </summary>
public class ConnectionManagerTests
{
    [Fact]
    public void GetConnection_UsesConfiguredConfiguration_OverAppSettingsJson()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SqlServerCon"] =
                    "Server=sql,1433;Database=DockerDb;User Id=sa;Password=secret;Encrypt=False;"
            })
            .Build();

        ConnectionManager.Configure(config);

        using var connection = ConnectionManager.GetConnection();
        var sqlConnection = Assert.IsType<System.Data.SqlClient.SqlConnection>(connection);

        Assert.Equal("sql,1433", sqlConnection.DataSource);
        Assert.Equal("DockerDb", sqlConnection.Database);
    }

    [Fact]
    public void GetConnection_FallsBackToAppSettingsJson_WhenConfiguredValueIsMissing()
    {
        // Configuração sem a chave -> deve usar o fallback original (appsettings.json)
        var config = new ConfigurationBuilder().Build();
        ConnectionManager.Configure(config);

        using var connection = ConnectionManager.GetConnection();

        Assert.False(string.IsNullOrWhiteSpace(connection.ConnectionString));
        Assert.Contains("Publicacoes_Prod", connection.ConnectionString);
    }
}
