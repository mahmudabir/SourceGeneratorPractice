using SourceGeneratorPractice.Generator.Sample;

namespace SourceGeneratorPractice.ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!\n");
        
        var example = new Examples();
        var entities = example.CreateEntities();
        entities.Select(x=> x.GetType().Name).ToList().ForEach(Console.WriteLine);
        
        Console.WriteLine();
        
       var report = example.CreateEntityReport(new SampleEntity());
       report.ToList().ForEach(Console.WriteLine);
       
       Console.WriteLine();
    }
}
