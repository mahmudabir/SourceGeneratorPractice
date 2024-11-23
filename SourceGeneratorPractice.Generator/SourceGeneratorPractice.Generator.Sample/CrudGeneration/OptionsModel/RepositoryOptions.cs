using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration
{
    public class RepositoryOptions
    {
        internal List<RepositoryRegistration> RepositoryRegistrations { get; } = new();

        public void AddRepository<TDbContext, TEntity, TKey>()
            where TDbContext : DbContext
        {
            RepositoryRegistrations.Add(new RepositoryRegistration
            {
                DbContextType = typeof(TDbContext),
                EntityType = typeof(TEntity),
                KeyType = typeof(TKey)
            });
        }
    }
    
    public class RepositoryRegistration
    {
        public Type DbContextType { get; set; }
        public Type EntityType { get; set; }
        public Type KeyType { get; set; }
    }
}
