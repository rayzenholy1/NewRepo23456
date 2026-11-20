using Dapper;
using DogShelter.Models;
using Microsoft.Data.SqlClient;

namespace DogShelter.Data;

public class AdopterRepository
{
    private const string SelectWithDogs = @"
        SELECT a.Id, a.Name, a.Phone,
               d.Id, d.Nickname, d.Age, d.Breed, d.IsAdopted, d.AdopterId
        FROM Adopters a
        LEFT JOIN Dogs d ON d.AdopterId = a.Id";

    private static SqlConnection Open() => new(Database.ConnectionString);

    private static List<Adopter> QueryWithDogs(string where = "", object? param = null)
    {
        var adopters = new Dictionary<int, Adopter>();
        using var c = Open();
        c.Query<Adopter, Dog, Adopter>(
            $"{SelectWithDogs} {where} ORDER BY a.Id, d.Id",
            (a, d) =>
            {
                if (!adopters.TryGetValue(a.Id, out var adopter))
                {
                    adopter = a;
                    adopters.Add(a.Id, adopter);
                }
                if (d is not null) adopter.Dogs.Add(d);
                return adopter;
            },
            param,
            splitOn: "Id");
        return adopters.Values.ToList();
    }

    public int Add(Adopter adopter)
    {
        using var c = Open();
        return c.ExecuteScalar<int>(@"
            INSERT INTO Adopters (Name, Phone)
            VALUES (@Name, @Phone);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", adopter);
    }

    public List<Adopter> GetAllWithDogs() => QueryWithDogs();

    public Adopter? FindByIdWithDogs(int id) =>
        QueryWithDogs("WHERE a.Id = @id", new { id }).FirstOrDefault();
}
