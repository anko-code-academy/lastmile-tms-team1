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
            foreach (var h in request.OperatingHours)
            {
                var existing = await dbContext.ShiftSchedules
                    .FirstOrDefaultAsync(s => s.DepotId == depot.Id && s.DayOfWeek == h.DayOfWeek, cancellationToken);

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
                    // Insert new schedule (only if times are provided)
                    depot.ShiftSchedules.Add(new ShiftSchedule
                    {
                        DayOfWeek = h.DayOfWeek,
                        OpenTime = h.OpenTime!.Value,
                        CloseTime = h.CloseTime!.Value
                    });
                }
            }
        }

        if (request.Address != null)
        {
            if (depot.Address == null)
            {
                depot.Address = new Address();
                dbContext.Addresses.Add(depot.Address);
            }

            depot.Address.Street1 = request.Address.Street1;
            depot.Address.Street2 = request.Address.Street2;
            depot.Address.City = request.Address.City;
            depot.Address.State = request.Address.State;
            depot.Address.PostalCode = request.Address.PostalCode;
            depot.Address.CountryCode = request.Address.CountryCode;
            depot.Address.IsResidential = request.Address.IsResidential;
            depot.Address.ContactName = request.Address.ContactName;
            depot.Address.CompanyName = request.Address.CompanyName;
            depot.Address.Phone = request.Address.Phone;
            depot.Address.Email = request.Address.Email;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DepotResult(depot.Id, depot.Name, depot.IsActive, depot.CreatedAt);
    }
}