using System.Text.RegularExpressions;
using DogShelter.Data;
using DogShelter.Models;

Database.Initialize();
var dogs = new DogRepository();
var adopters = new AdopterRepository();

string Ask(string label)
{
    Console.Write(label + ": ");
    return Console.ReadLine()?.Trim() ?? "";
}

string AskRequired(string label, int max = 50)
{
    while (true)
    {
        var s = Ask(label);
        if (s.Length > 0 && s.Length <= max) return s;
        Console.WriteLine($"Значення не може бути порожнім або довшим за {max} символів.");
    }
}

int AskInt(string label, int min = 0)
{
    while (true)
    {
        if (int.TryParse(Ask(label), out var n) && n >= min) return n;
        Console.WriteLine($"Введіть ціле число не менше {min}.");
    }
}

string AskPhone()
{
    while (true)
    {
        var phone = Regex.Replace(Ask("Номер телефону"), @"[\s\-()]", "");
        if (Regex.IsMatch(phone, @"^\+?\d{10,15}$")) return phone;
        Console.WriteLine("Некоректний номер. Приклад: +380501234567");
    }
}

void Print(List<Dog> list)
{
    if (list.Count == 0) { Console.WriteLine("Нікого не знайдено."); return; }
    foreach (var d in list) Console.WriteLine(d);
}

void PrintAdopters(List<Adopter> list)
{
    if (list.Count == 0) { Console.WriteLine("Опікунів немає."); return; }
    foreach (var a in list)
    {
        Console.WriteLine(a);
        if (a.Dogs.Count == 0) Console.WriteLine("   собак немає");
        foreach (var d in a.Dogs) Console.WriteLine($"   - [{d.Id}] {d.Nickname}, {d.Breed}, {d.Age} р.");
    }
}

void AddDog()
{
    var dog = new Dog
    {
        Nickname = AskRequired("Кличка"),
        Age = AskInt("Вік"),
        Breed = AskRequired("Порода")
    };
    dog.Id = dogs.Add(dog);
    Console.WriteLine($"Собаку додано, Id={dog.Id}");
}

void Search()
{
    Console.WriteLine("Шукати за: 1 - кличкою, 2 - Id, 3 - породою");
    switch (Ask("Вибір"))
    {
        case "1":
            Print(dogs.FindByNickname(AskRequired("Кличка")));
            break;
        case "2":
            var dog = dogs.FindById(AskInt("Id", 1));
            if (dog is null) Console.WriteLine("Нікого не знайдено.");
            else Console.WriteLine(dog);
            break;
        case "3":
            Print(dogs.FindByBreed(AskRequired("Порода")));
            break;
        default:
            Console.WriteLine("Невідомий вибір.");
            break;
    }
}

void UpdateDog()
{
    var dog = dogs.FindById(AskInt("Id", 1));
    if (dog is null) { Console.WriteLine("Собаку не знайдено."); return; }

    Console.WriteLine($"Поточні дані: {dog}");
    Console.WriteLine("Порожній ввід залишає значення без змін.");

    var nickname = Ask("Нова кличка");
    if (nickname.Length is > 0 and <= 50) dog.Nickname = nickname;

    var age = Ask("Новий вік");
    if (int.TryParse(age, out var a) && a >= 0) dog.Age = a;

    var breed = Ask("Нова порода");
    if (breed.Length is > 0 and <= 50) dog.Breed = breed;

    Console.WriteLine(dogs.Update(dog) ? "Дані оновлено." : "Не вдалося оновити.");
}

void AddAdopter()
{
    var adopter = new Adopter
    {
        Name = AskRequired("Ім'я", 100),
        Phone = AskPhone()
    };
    adopter.Id = adopters.Add(adopter);
    Console.WriteLine($"Опікуна додано, Id={adopter.Id}");
}

void AdoptDog()
{
    var inShelter = dogs.GetByAdopted(false);
    if (inShelter.Count == 0) { Console.WriteLine("У притулку немає собак."); return; }

    var all = adopters.GetAllWithDogs();
    if (all.Count == 0) { Console.WriteLine("Спочатку додайте опікуна."); return; }

    Console.WriteLine("Собаки в притулку:");
    Print(inShelter);
    var dog = dogs.FindById(AskInt("Id собаки", 1));
    if (dog is null) { Console.WriteLine("Собаку не знайдено."); return; }
    if (dog.IsAdopted) { Console.WriteLine("Цю собаку вже забрали."); return; }

    Console.WriteLine("Опікуни:");
    foreach (var a in all) Console.WriteLine(a);
    var adopter = adopters.FindByIdWithDogs(AskInt("Id опікуна", 1));
    if (adopter is null) { Console.WriteLine("Опікуна не знайдено."); return; }

    Console.WriteLine(dogs.Adopt(dog.Id, adopter.Id)
        ? $"{dog.Nickname} тепер живе в {adopter.Name}."
        : "Не вдалося оформити адопцію: собаку вже забрали.");
}

while (true)
{
    Console.WriteLine(@"
=== ПРИТУЛОК ДЛЯ СОБАК ===
1. Додати собаку
2. Усі собаки
3. Собаки в притулку
4. Собаки, яких забрали
5. Пошук собаки
6. Оновити дані собаки
7. Додати опікуна
8. Список опікунів та їхніх собак
9. Віддати собаку опікуну
0. Вихід");

    try
    {
        switch (Ask("Вибір"))
        {
            case "1": AddDog(); break;
            case "2": Print(dogs.GetAll()); break;
            case "3": Print(dogs.GetByAdopted(false)); break;
            case "4": Print(dogs.GetByAdopted(true)); break;
            case "5": Search(); break;
            case "6": UpdateDog(); break;
            case "7": AddAdopter(); break;
            case "8": PrintAdopters(adopters.GetAllWithDogs()); break;
            case "9": AdoptDog(); break;
            case "0": return;
            default: Console.WriteLine("Невідома команда."); break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Помилка: " + ex.Message);
    }
}
