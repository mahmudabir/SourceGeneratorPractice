using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration
{
    public interface IRepository<TEntity, TKey>
    {
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TKey id);
    }
}
