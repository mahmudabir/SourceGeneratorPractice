using System.Text.Json;
using Generated;
using SourceGeneratorPractice.Generator.Sample;
using SourceGeneratorPractice.Generator.Sample.Models;

namespace SourceGeneratorPractice.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!\n");

        var example = new Examples();
        var entities = example.CreateEntities();
        entities.Select(x => x.GetType().Name).ToList().ForEach(Console.WriteLine);

        Console.WriteLine();

        var report = example.CreateEntityReport(new SampleEntity());
        report.ToList().ForEach(Console.WriteLine);

        Console.WriteLine();
        
        // IEnumerable<>

        var student = new Student
        {
            Id = 11,
            ClassNo = "IV",
            RollNo = 999,
            Person = new Person
            {
                 Id = 22,
                 Age = 25,
                 Name = "Abir Mahmud",
                 Gender = Gender.Male
            }
        };
        
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            IndentSize = 2
        };

        Console.WriteLine("ToDto:");
        Console.WriteLine(JsonSerializer.Serialize(student.ToDto(), jsonSerializerOptions));
        
        Console.WriteLine();
        
        Console.WriteLine("ToViewModel:");
        Console.WriteLine(JsonSerializer.Serialize(student.ToViewModel(), jsonSerializerOptions));

        var personController = new PersonController(null);
    }
}
