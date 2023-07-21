using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.FurnitureStore.Data;
using API.FurnitureStore.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Api.FurnitureStore.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly APIFurnitureStoreContext _Context;

        public ProductCategoriesController(APIFurnitureStoreContext Context)
        {
            _Context = Context;
        }

        [HttpGet]
        public async Task<IEnumerable<ProductCategory>> Get()
        {
           return await _Context.ProductsCategories.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var category = await _Context.ProductsCategories.FindAsync(id);

            if (category == null ) return NotFound("el ID ingresado no existe ");

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ProductCategory category)
        {
            if (category == null ||  category.Name ==  string.Empty) return BadRequest();
 
            await _Context.ProductsCategories.AddAsync(category);
            await _Context.SaveChangesAsync();

            return CreatedAtAction("Post" , category.Id , category);

        }


        [HttpPut]
        public async Task<IActionResult> Put(ProductCategory category)
        {
            if (category == null || category.Name == string.Empty) return BadRequest();

            _Context.ProductsCategories.Update(category);

            await _Context.SaveChangesAsync();

            return NoContent();
          
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(ProductCategory Category)
        {
            if (Category == null || Category.Name == string.Empty) return BadRequest();

            _Context.ProductsCategories.Remove(Category);
            await _Context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteId(int id )
        {
            var Category =  await _Context.ProductsCategories.FindAsync(id);

            if (Category == null ) return NotFound();
            _Context.ProductsCategories.Remove(Category);
            await _Context.SaveChangesAsync();  

            return NoContent();
        }
     
    }
}
