using System;
using System.Threading.Tasks;
using SourceGeneratorPractice.Generator.Sample.CrudGeneration.Service;

namespace SourceGeneratorPractice.Generator.Sample.CrudGeneration
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class DefaultController<TCreateRequest, TUpdateRequest, TViewModel, TEntity, TKey> : ControllerBase
        where TEntity : class
    {
        private readonly IService<TViewModel, TEntity, TKey> _service;

        public DefaultController(IService<TViewModel, TEntity, TKey> service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(TKey id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TCreateRequest request)
        {
            // Add logic to map request to ViewModel
            throw new NotImplementedException("Add request-to-viewmodel mapping logic.");
        }

        [HttpPut]
        public async Task<IActionResult> Update(TUpdateRequest request)
        {
            // Add logic to map request to ViewModel
            throw new NotImplementedException("Add request-to-viewmodel mapping logic.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(TKey id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }

}
