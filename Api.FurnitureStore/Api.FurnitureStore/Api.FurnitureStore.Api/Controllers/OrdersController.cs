using Api.FurnitureStore.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.FurnitureStore.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Api.FurnitureStore.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly APIFurnitureStoreContext _context;

        public OrdersController(APIFurnitureStoreContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            return await _context.Orders.Include(p => p.OrderDatails).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var order = await _context.Orders.Include(p => p.OrderDatails).FirstOrDefaultAsync(p => p.Id == id);
            if (order == null) return NotFound();

            return Ok(order);
        }

        [HttpPost]
        public  async Task<IActionResult> Post(Order order)
        {
            if (order == null) return NotFound();

            if (order.OrderDatails == null) return BadRequest(" La orden no muestra ningun detalle ");

            await _context.Orders.AddAsync(order);

            await _context.OrderDetails.AddRangeAsync(order.OrderDatails);// en esta linea se insertan todos los detalles

            await _context.SaveChangesAsync();

            return CreatedAtAction("Post" , order.Id , order);
        }
        [HttpPut]
        public async Task<IActionResult> Put(Order order)
        {
            if (order == null) return NotFound();
            if (order.Id <= 0) return NotFound();

            var existingOrder = await _context.Orders.Include(p => p.OrderDatails).FirstOrDefaultAsync(p => p.Id == order.Id );

            if (existingOrder == null) return NotFound();

            // actualizar Order 
            existingOrder.OrderNumber = order.OrderNumber;
            existingOrder.OrderDate = order.OrderDate;
            existingOrder.Delivery = order.Delivery;
            existingOrder.ClientId = order.ClientId;

            // borrar los detalles de orden 

            _context.OrderDetails.RemoveRange(existingOrder.OrderDatails);

            // actializa todas las ordenes y sus detalles 

            _context.Orders.Update(existingOrder);

            _context.OrderDetails.AddRange(order.OrderDatails);

            // guardar los cambios 

            await _context.SaveChangesAsync();

            return NoContent();

        }
        [HttpDelete]
        public async Task<IActionResult> Delete(Order order)
        {
            if (order == null) return NotFound();

             var existingOrder = await _context.Orders.Include(p => p.OrderDatails).FirstOrDefaultAsync(p => p.Id == order.Id );

            if (existingOrder == null) return NotFound();

            _context.OrderDetails.RemoveRange(existingOrder.OrderDatails);
            _context.Orders.Remove(existingOrder);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
