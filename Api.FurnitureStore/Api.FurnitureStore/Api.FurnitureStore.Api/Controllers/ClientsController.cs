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
    public class ClientsController : ControllerBase
    {
        private readonly APIFurnitureStoreContext _Context;

        public ClientsController(APIFurnitureStoreContext Context)
        {
            _Context = Context;
        }

        [HttpGet]
        public async Task<IEnumerable<Client>> Get()
        {
            return await _Context.Clients.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeatils(int id)
        {
            var Client = await _Context.Clients.FirstOrDefaultAsync(p => p.Id == id);

            if (Client == null) return NotFound();

            return Ok(Client);
        }
        [HttpPost]
        public async Task<IActionResult> Post(Client client)
        {
            await _Context.Clients.AddAsync(client);
            await _Context.SaveChangesAsync();

            return CreatedAtAction("Post", client.Id, client);
        }

        [HttpPut]
        public async Task<IActionResult> Put(Client client)
        {
            _Context.Clients.Update(client);
            await _Context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Client client)
        {
            if (client == null) return NotFound();

            _Context.Clients.Remove(client);
            await _Context.SaveChangesAsync();
            return NoContent();
        }
    }
}
