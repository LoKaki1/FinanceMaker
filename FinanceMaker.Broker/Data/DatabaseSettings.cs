using System;

namespace FinanceMaker.Broker.Data;

public class DatabaseSettings
{
    public string Provider { get; set; } = "Sqlite";
    public string ConnectionString { get; set; } = "Data Source=broker.db";
}

public static class DatabaseProvider
{
    public const string Sqlite = "Sqlite";
    public const string Postgres = "Postgres";
}
