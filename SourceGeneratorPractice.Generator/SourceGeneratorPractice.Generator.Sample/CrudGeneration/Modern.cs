using System;
using Microsoft.Extensions.DependencyInjection;
using SourceGeneratorPractice.Generator.Sample.CrudGeneration.Repository;
using SourceGeneratorPractice.Generator.Sample.CrudGeneration.Service;
using SourceGeneratorPractice.Generator.Sample.Models;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration
{
    public static class Modern
    {
        public static void AddCrud(this IServiceCollection services)
        {
            services
                .AddModern()
                .AddRepositories(options =>
                {
                    options.AddRepository<EfCoreDbContext, Person, int>();
                    // options.AddRepository<EfCoreDbContext, Student, Guid>();
                })
                .AddServices(options =>
                {
                    options.AddService<PersonViewModel, Person, int>();
                    // options.AddService<StudentViewModel, Student, Guid>();
                })
                .AddControllers(options =>
                {
                    options.AddController<PersonViewModel, PersonViewModel, PersonViewModel, Person, int>("api/persons");
                    // options.AddController<StudentViewModel, StudentViewModel, StudentViewModel, Student, Guid>("api/students");
                });
        }

        public static IServiceCollection AddModern(this IServiceCollection services)
        {
            return services;
        }

        public static IServiceCollection AddRepositories(
            this IServiceCollection services,
            Action<RepositoryOptions> configure)
        {
            var options = new RepositoryOptions();
            configure(options);

            foreach (var repositoryRegistration in options.RepositoryRegistrations)
            {
                var dbContextType = repositoryRegistration.DbContextType;
                var entityType = repositoryRegistration.EntityType;
                var keyType = repositoryRegistration.KeyType;

                // Register repository dynamically
                var repositoryInterface = typeof(IRepository<,>).MakeGenericType(entityType, keyType);
                var repositoryImplementation = typeof(EfCoreRepository<,,>).MakeGenericType(dbContextType, entityType, keyType);

                services.AddScoped(repositoryInterface, repositoryImplementation);
            }

            return services;
        }

        public static IServiceCollection AddServices(
            this IServiceCollection services,
            Action<ServiceOptions> configure)
        {
            var options = new ServiceOptions();
            configure(options);

            foreach (var serviceRegistration in options.ServiceRegistrations)
            {
                var viewModelType = serviceRegistration.ViewModelType;
                var entityType = serviceRegistration.EntityType;
                var keyType = serviceRegistration.KeyType;

                // Register the service dynamically
                var serviceInterface = typeof(IService<,,>).MakeGenericType(viewModelType, entityType, keyType);
                var serviceImplementation = typeof(DefaultService<,,>).MakeGenericType(viewModelType, entityType, keyType);

                services.AddScoped(serviceInterface, serviceImplementation);
            }

            return services;
        }

        public static IServiceCollection AddControllers(
            this IServiceCollection services,
            Action<ControllerOptions> configure)
        {
            var options = new ControllerOptions();
            configure(options);

            foreach (var controllerRegistration in options.ControllerRegistrations)
            {
                // Register controller dynamically
                var controllerImplementation = typeof(DefaultController<,,,,>).MakeGenericType(
                                                                                               controllerRegistration.CreateRequestType,
                                                                                               controllerRegistration.UpdateRequestType,
                                                                                               controllerRegistration.ViewModelType,
                                                                                               controllerRegistration.EntityType,
                                                                                               controllerRegistration.KeyType
                                                                                              );

                services.AddTransient(controllerImplementation);
            }

            return services;
        }

    }
}
