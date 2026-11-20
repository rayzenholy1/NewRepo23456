using Dapper;
using DogShelter.Models;
using Microsoft.Data.SqlClient;

namespace DogShelter.Data;

public class DogRepository
{
    private const string SelectDogs = @"
        SELECT d.Id, d.Nickname, d.Age, d.Breed, d.IsAdopted, d.AdopterId,
               a.Id, a.Name, a.Phone
        FROM Dogs d
        LEFT JOIN Adopters a ON a.Id = d.AdopterId";

    private static SqlConnection Open() => new(Database.ConnectionString);

    private static List<Dog> Query(string where = "", object? param = null)
    {
        using var c = Open();
        return c.Query<Dog, Adopter, Dog>(
            $"{SelectDogs} {where} ORDER BY d.Id",
            (d, a) => { d.Adopter = a; return d; },
            param,
            splitOn: "Id").ToList();
    }

    public int Add(Dog dog)
    {
        using var c = Open();
        return c.ExecuteScalar<int>(@"
            INSERT INTO Dogs (Nickname, Age, Breed)
            VALUES (@Nickname, @Age, @Breed);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", dog);
    }

    public List<Dog> GetAll() => Query();

    public List<Dog> GetByAdopted(bool isAdopted) =>
        Query("WHERE d.IsAdopted = @isAdopted", new { isAdopted });

    public Dog? FindById(int id) =>
        Query("WHERE d.Id = @id", new { id }).FirstOrDefault();

    public List<Dog> FindByNickname(string nickname) =>
        Query("WHERE d.Nickname LIKE @pattern", new { pattern = $"%{nickname}%" });

    public List<Dog> FindByBreed(string breed) =>
        Query("WHERE d.Breed LIKE @pattern", new { pattern = $"%{breed}%" });

    public bool Update(Dog dog)
    {
        using var c = Open();
        return c.Execute(@"
            UPDATE Dogs
            SET Nickname = @Nickname, Age = @Age, Breed = @Breed
            WHERE Id = @Id", dog) > 0;
    }

    public bool Adopt(int dogId, int adopterId)
    {
        using var c = Open();
        return c.Execute(@"
            UPDATE Dogs
            SET IsAdopted = 1, AdopterId = @adopterId
            WHERE Id = @dogId AND IsAdopted = 0", new { dogId, adopterId }) > 0;
    }
}
