using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Depots.Commands.UpdateDepot;

public class UpdateDepotHandler(IAppDbContext dbContext) : IRequestHandler<UpdateDepotCommand, DepotResult>
{
    public async Task<DepotResult> Handle(UpdateDepotCommand request, CancellationToken cancellationToken)
    {
        var depot = await dbContext.Depots
            .Include(d => d.Address)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Depot with ID {request.Id} not found.");

        depot.Name = request.Name;
        depot.IsActive = request.IsActive;

        if (request.OperatingHours != null)
        {
            // Load existing schedules directly, not via navigation property to avoid EF tracking issues
            var existingSchedules = await dbContext.ShiftSchedules
                .Where(s => s.DepotId == depot.Id)
                .ToListAsync(cancellationToken);

            // Get the set of days in the request
            var requestedDays = request.OperatingHours
                .Select(h => h.DayOfWeek)
                .ToHashSet();

            // Remove schedules for days NOT in the request
            foreach (var existing in existingSchedules)
            {
                if (!requestedDays.Contains(existing.DayOfWeek))
                {
                    dbContext.ShiftSchedules.Remove(existing);
                }
            }

            foreach (var h in request.OperatingHours)
            {
                var existing = existingSchedules
                    .FirstOrDefault(s => s.DayOfWeek == h.DayOfWeek);

                if (existing != null)
                {
                    if (h.OpenTime == null || h.CloseTime == null)
                    {
                        // Empty times → remove this day's schedule
                        dbContext.ShiftSchedules.Remove(existing);
                    }
                    else
                    {
                        // Update existing schedule
                        existing.OpenTime = h.OpenTime!.Value;
                        existing.CloseTime = h.CloseTime!.Value;
                    }
                }
                else if (h.OpenTime != null && h.CloseTime != null)
                {
                    // Insert new schedule
                    var schedule = new ShiftSchedule
                    {
                        DayOfWeek = h.DayOfWeek,
                        OpenTime = h.OpenTime!.Value,
                        CloseTime = h.CloseTime!.Value,
                        DepotId = depot.Id
                    };
                    dbContext.ShiftSchedules.Add(schedule);
                }
            }
        }

        if (depot.Address == null)
        {
            // Pre-migration data: AddressId is set but Address navigation is not loaded
            // This should not happen after migration, but handle for backward compatibility
            throw new InvalidOperationException($"Depot with ID {request.Id} has no Address. Please apply the 'MakeDepotAddressRequired' migration first.");
        }

        depot.Address.Street1 = request.Address.Street1;
        depot.Address.Street2 = request.Address.Street2;
        depot.Address.City = request.Address.City;
        depot.Address.State = request.Address.State;
        depot.Address.PostalCode = request.Address.PostalCode;
        depot.Address.CountryCode = string.IsNullOrEmpty(request.Address.CountryCode) ? "US" : request.Address.CountryCode;
        depot.Address.IsResidential = request.Address.IsResidential;
        depot.Address.ContactName = request.Address.ContactName;
        depot.Address.CompanyName = request.Address.CompanyName;
        depot.Address.Phone = request.Address.Phone;
        depot.Address.Email = request.Address.Email;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DepotResult(depot.Id, depot.Name, depot.IsActive, depot.CreatedAt);
    }
}