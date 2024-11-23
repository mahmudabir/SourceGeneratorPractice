using System.Collections.Generic;
using Entities;
using Microsoft.Extensions.DependencyInjection;
using SourceGeneratorPractice.Generator.Sample.CrudGeneration;

namespace SourceGeneratorPractice.Generator.Sample;

public class Examples
{
    // Create generated entities, based on DDD.UbiquitousLanguageRegistry.txt
    public object[] CreateEntities()
    {
        return new object[]
        {
            new Customer(), new Employee(), new Product(), new Shop(), new Stock(),
            new Cart(), new Order()
        };
    }

    // Execute generated method Report
    public IEnumerable<string> CreateEntityReport(SampleEntity entity)
    {
        // IServiceCollection services = new ServiceCollection();
        // services.AddCrud();
        
        
        return entity.Report();
    }
}
