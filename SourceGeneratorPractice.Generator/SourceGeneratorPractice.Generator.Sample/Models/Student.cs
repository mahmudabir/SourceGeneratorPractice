using System;
using SourceGeneratorPractice.Generator.Sample.Attributes;

namespace SourceGeneratorPractice.Generator.Sample.Models
{
    [HasViewModel]
    public class Student
    {
        public int Id { get; set; }

        public Person Person { get; set; }
        
        public string ClassNo { get; set; }
        public int RollNo { get; set; }
    }
}
