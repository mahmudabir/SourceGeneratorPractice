using System;
using System.Collections.Generic;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration
{
    public class ControllerOptions
    {
        internal List<ControllerRegistration> ControllerRegistrations { get; } = new();

        public void AddController<TCreateRequest, TUpdateRequest, TViewModel, TEntity, TKey>(string route)
        {
            ControllerRegistrations.Add(new ControllerRegistration
            {
                CreateRequestType = typeof(TCreateRequest),
                UpdateRequestType = typeof(TUpdateRequest),
                ViewModelType = typeof(TViewModel),
                EntityType = typeof(TEntity),
                KeyType = typeof(TKey),
                Route = route
            });
        }
    }

    public class ControllerRegistration
    {
        public Type CreateRequestType { get; set; }
        public Type UpdateRequestType { get; set; }
        public Type ViewModelType { get; set; }
        public Type EntityType { get; set; }
        public Type KeyType { get; set; }
        public string Route { get; set; }
    }

}
