using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration.Service
{
    public interface IService<TViewModel, TEntity, TKey>
    {
        Task<IEnumerable<TViewModel>> GetAllAsync();
        Task<TViewModel?> GetByIdAsync(TKey id);
        Task AddAsync(TViewModel viewModel);
        Task UpdateAsync(TViewModel viewModel);
        Task DeleteAsync(TKey id);
    }
}
