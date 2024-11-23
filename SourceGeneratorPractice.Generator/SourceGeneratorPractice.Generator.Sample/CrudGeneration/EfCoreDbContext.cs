using Microsoft.EntityFrameworkCore;
using SourceGeneratorPractice.Generator.Sample.Models;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration
{
    public class EfCoreDbContext: DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Student> Students { get; set; }
    }


    public class Test
    {
        public void Method()
        {
            var context = new EfCoreDbContext();
            var students = context.Students;
            var people = context.People;
        }
    }
    
}
