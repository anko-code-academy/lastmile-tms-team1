using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;
using LastMile.TMS.Domain.Common;
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
            depot.OperatingHours = OperatingHours.Create(
                request.OperatingHours.Select(h =>
                    new DailyOperatingHours(h.DayOfWeek, h.OpenTime, h.CloseTime)).ToArray());
        }

        if (request.Address != null)
        {
            if (depot.Address == null)
            {
                depot.Address = new Address();
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
