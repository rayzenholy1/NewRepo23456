namespace DogShelter.Models;

public class Adopter
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public List<Dog> Dogs { get; set; } = new();

    public override string ToString() => $"[{Id}] {Name}, {Phone}";
}
