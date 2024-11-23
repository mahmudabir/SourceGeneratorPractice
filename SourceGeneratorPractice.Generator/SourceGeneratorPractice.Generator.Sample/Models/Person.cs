using System;
using SourceGeneratorPractice.Generator.Sample.Attributes;

namespace SourceGeneratorPractice.Generator.Sample.Models
{
    [HasViewModel]
    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
    }
}
