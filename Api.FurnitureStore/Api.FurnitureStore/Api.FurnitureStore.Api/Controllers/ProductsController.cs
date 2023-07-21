using Api.FurnitureStore.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using API.FurnitureStore.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Api.FurnitureStore.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly  APIFurnitureStoreContext _Context;
         
        public ProductsController( APIFurnitureStoreContext Context)
        {
            _Context = Context;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            return await _Context.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeatils(int id )
        {
            var Product = await _Context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (Product == null) return NotFound("el id ingresado no existe  ");

            return Ok(Product); 

        }

        [HttpGet("GetByCategory/{productCategoryId}")]
        public async Task<IEnumerable<Product>> GetByCategory(int productCategoryId)
        {
            return await _Context.Products.Where(
                p => p.ProductCategoryId == productCategoryId
                ).ToListAsync();

        }

        [HttpPost]
        public async Task<IActionResult> Post(Product product)
        {
            if (product == null) return BadRequest();

            await _Context.Products.AddAsync(product);
            await _Context.SaveChangesAsync();
            return CreatedAtAction("Post" , product.Id, product);
        }

        [HttpPut]
        public async Task<IActionResult> Put(Product product)
        {
            if (product == null) return BadRequest(product);

            _Context.Products.Update(product);
            await _Context.SaveChangesAsync();

            return NoContent();
        }
       

        [HttpDelete]
        public async Task<IActionResult> Delete(Product product )
        {
            if (product == null) return BadRequest("ingrese un producto valido");

             _Context.Products.Remove(product);
            await _Context.SaveChangesAsync();

            return NoContent();

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteId(int id )
        {
            var product = await _Context.Products.FindAsync(id );

            if (product == null) return NotFound("el id ingresado no existe ");

            _Context.Products.Remove(product);

            await _Context.SaveChangesAsync();  

            return NoContent(); 
        }

    }
}
