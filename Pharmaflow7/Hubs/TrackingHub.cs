using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Data;
using Pharmaflow7.Models;

namespace Pharmaflow7.Hubs
{
    [Authorize]
    public class TrackingHub : Hub
    {
        private readonly AppDbContext _context;

        public TrackingHub(AppDbContext context) => _context = context;

        public async Task UpdateLocation(int shipmentId, double latitude, double longitude)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Driver)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);
            if (shipment != null)
            {
                var location = new VehicleLocation
                {
                    ShipmentId = shipmentId,
                    Latitude = (decimal)latitude,
                    Longitude = (decimal)longitude,
                    Timestamp = DateTime.UtcNow
                };
                _context.VehicleLocations.Add(location);
                await _context.SaveChangesAsync();

                await Clients.Group($"shipment_{shipmentId}").SendAsync("ReceiveLocationUpdate", shipmentId, latitude, longitude, shipment.Status);
            }
        }

        public async Task StartTrip(int shipmentId)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Driver)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);
            if (shipment != null)
            {
                shipment.Status = "In Transit";
                await _context.SaveChangesAsync();
                await Clients.Group($"shipment_{shipmentId}").SendAsync("TripStarted", shipmentId, shipment.Status);
            }
        }

        public async Task EndTrip(int shipmentId)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Driver)
                .FirstOrDefaultAsync(s => s.Id == shipmentId);
            if (shipment != null)
            {
                shipment.Status = "Delivered";
                await _context.SaveChangesAsync();
                await Clients.Group($"shipment_{shipmentId}").SendAsync("TripEnded", shipmentId, shipment.Status);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user != null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (role == "distributor")
                {
                    var driverIds = await _context.Drivers
                        .Where(d => d.DistributorId == userId)
                        .Select(d => d.Id) // Id is int
                        .ToListAsync();
                    var shipmentIds = await _context.Shipments
                        .Where(s => s.DriverId.HasValue && driverIds.Contains(s.DriverId.Value))
                        .Select(s => s.Id)
                        .ToListAsync();
                    foreach (var shipmentId in shipmentIds)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"shipment_{shipmentId}");
                    }
                }
                else if (role == "company")
                {
                    var shipmentIds = await _context.Shipments
                        .Where(s => s.CompanyId == userId)
                        .Select(s => s.Id)
                        .ToListAsync();
                    foreach (var shipmentId in shipmentIds)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"shipment_{shipmentId}");
                    }
                }
                else if (role == "driver")
                {
                    var driver = await _context.Drivers
                        .FirstOrDefaultAsync(d => d.ApplicationUserId == userId);
                    if (driver != null)
                    {
                        var shipmentIds = await _context.Shipments
                            .Where(s => s.DriverId == driver.Id)
                            .Select(s => s.Id)
                            .ToListAsync();
                        foreach (var shipmentId in shipmentIds)
                        {
                            await Groups.AddToGroupAsync(Context.ConnectionId, $"shipment_{shipmentId}");
                        }
                    }
                }
            }
            await base.OnConnectedAsync();
        }
    }
}