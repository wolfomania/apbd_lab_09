using HomeAssignment.Data;
using HomeAssignment.Models;
using HomeAssignment.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeAssignment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController(ApbdContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var trips = await context.Trips
                .Include(t => t.IdCountries)
                .Include(t => t.ClientTrips)
                .ThenInclude(ct => ct.IdClientNavigation)
                .OrderByDescending(t => t.DateFrom)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new 
                {
                    t.Name,
                    t.Description,
                    t.DateFrom,
                    t.DateTo,
                    t.MaxPeople,
                    Countries = t.IdCountries.Select(c => new
                    {
                        c.Name
                    }),
                    Clients = t.ClientTrips.Select(ct => new
                    {
                        ct.IdClientNavigation.FirstName, 
                        ct.IdClientNavigation.LastName
                    })
                })
                .ToListAsync();

            var totalTrips = await context.Trips.CountAsync();
            var totalPages = totalTrips / pageSize;

            return Ok(new
            {
                pageNum = page,
                pageSize = pageSize,
                allPages = totalPages,
                trips
            });
        }

        [HttpDelete("/{idClient:int}")]
        public async Task<IActionResult> DeleteClient(int idClient)
        {
            var client = await context.Clients
                .Include(c => c.ClientTrips)
                .FirstOrDefaultAsync(c => c.IdClient == idClient);

            if (client == null)
                return NotFound($"Client with id: {idClient} not found");

            if (client.ClientTrips.Count != 0)
                return BadRequest("Client has assigned trips to visit");

            context.Clients.Remove(client);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("/{idTrip:int}/clients")]
        public async Task<IActionResult> AddClient(int idTrip, [FromBody] AddRequest addRequest)
        {
            
            var trip = await context.Trips.FindAsync(idTrip);
            
            if (trip == null)
                return BadRequest("Trip does not exist");

            if (trip.DateFrom <= DateTime.Now)
                return BadRequest("Trip has already occured");

            var existingClient = await context.Clients
                .FirstOrDefaultAsync(c => c.Pesel == addRequest.Pesel);
            
            if (existingClient != null)
            {
                var alreadyRegistered = await context.ClientTrips
                    .AnyAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);
                return BadRequest(alreadyRegistered ? "Client with the given PESEL number is already registered for the given trip" 
                    : "Check if a client with the given PESEL number already exists");
            }

            existingClient = new Client
            {
                FirstName = addRequest.FirstName,
                LastName = addRequest.LastName,
                Email = addRequest.Email,
                Telephone = addRequest.Telephone,
                Pesel = addRequest.Pesel
            };
            context.Clients.Add(existingClient);
            await context.SaveChangesAsync();

            var clientTrip = new ClientTrip
            {
                IdClient = existingClient.IdClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = addRequest.paymentDate
            };
            context.ClientTrips.Add(clientTrip);
            await context.SaveChangesAsync();

            return Ok();
        }
    }

}
