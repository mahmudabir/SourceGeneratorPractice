using System;
using System.Collections.Generic;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration
{
    public class ServiceOptions
    {
        internal List<ServiceRegistration> ServiceRegistrations { get; } = new();

        public void AddService<TViewModel, TEntity, TKey>()
        {
            ServiceRegistrations.Add(new ServiceRegistration
            {
                ViewModelType = typeof(TViewModel),
                EntityType = typeof(TEntity),
                KeyType = typeof(TKey)
            });
        }
    }

    public class ServiceRegistration
    {
        public Type ViewModelType { get; set; }
        public Type EntityType { get; set; }
        public Type KeyType { get; set; }
    }
}
