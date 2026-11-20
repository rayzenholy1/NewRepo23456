using Dapper;
using Microsoft.Data.SqlClient;

namespace DogShelter.Data;

public static class Database
{
    private const string Server = "Server=(localdb)\\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";
    private const string Name = "DogShelterDb";

    public static string ConnectionString => $"{Server};Database={Name}";

    public static void Initialize()
    {
        using (var master = new SqlConnection(Server))
        {
            master.Execute($"IF DB_ID('{Name}') IS NULL CREATE DATABASE {Name}");
        }

        using var c = new SqlConnection(ConnectionString);

        c.Execute(@"
IF OBJECT_ID('dbo.Adopters', 'U') IS NULL
CREATE TABLE dbo.Adopters
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL UNIQUE
)");

        c.Execute(@"
IF OBJECT_ID('dbo.Dogs', 'U') IS NULL
CREATE TABLE dbo.Dogs
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nickname NVARCHAR(50) NOT NULL,
    Age INT NOT NULL CHECK (Age >= 0),
    Breed NVARCHAR(50) NOT NULL,
    IsAdopted BIT NOT NULL DEFAULT 0,
    AdopterId INT NULL CONSTRAINT FK_Dogs_Adopters REFERENCES dbo.Adopters(Id)
)");

        c.Execute(@"
IF COL_LENGTH('dbo.Dogs', 'AdopterId') IS NULL
ALTER TABLE dbo.Dogs ADD AdopterId INT NULL CONSTRAINT FK_Dogs_Adopters REFERENCES dbo.Adopters(Id)");
    }
}
