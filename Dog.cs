namespace DogShelter.Models;

public class Dog
{
    public int Id { get; set; }
    public string Nickname { get; set; } = null!;
    public int Age { get; set; }
    public string Breed { get; set; } = null!;
    public bool IsAdopted { get; set; }
    public int? AdopterId { get; set; }
    public Adopter? Adopter { get; set; }

    public override string ToString()
    {
        var status = !IsAdopted
            ? "в притулку"
            : Adopter is null ? "забрали" : $"забрав(ла) {Adopter.Name} ({Adopter.Phone})";
        return $"[{Id}] {Nickname}, {Age} р., {Breed}, {status}";
    }
}
