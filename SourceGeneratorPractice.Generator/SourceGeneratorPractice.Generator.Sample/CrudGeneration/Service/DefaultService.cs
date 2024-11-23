using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration.Service
{
    public class DefaultService<TViewModel, TEntity, TKey> : IService<TViewModel, TEntity, TKey>
        where TEntity : class
    {
        private readonly IRepository<TEntity, TKey> _repository;

        public DefaultService(IRepository<TEntity, TKey> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TViewModel>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            return entities.Select(e => MapToViewModel(e)).ToList();
        }

        public async Task<TViewModel> GetByIdAsync(TKey id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? default : MapToViewModel(entity);
        }

        public async Task AddAsync(TViewModel viewModel)
        {
            var entity = MapToEntity(viewModel);
            await _repository.AddAsync(entity);
        }

        public async Task UpdateAsync(TViewModel viewModel)
        {
            var entity = MapToEntity(viewModel);
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(TKey id) => await _repository.DeleteAsync(id);

        private TEntity MapToEntity(TViewModel viewModel)
        {
            // Mapping logic from ViewModel to Entity
            throw new NotImplementedException("Add your mapping logic here.");
        }

        private TViewModel MapToViewModel(TEntity entity)
        {
            // Mapping logic from Entity to ViewModel
            throw new NotImplementedException("Add your mapping logic here.");
        }
    }

}
