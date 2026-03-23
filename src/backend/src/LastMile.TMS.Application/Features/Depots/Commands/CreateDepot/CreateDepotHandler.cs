using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Features.Depots.Commands.CreateDepot;

public class CreateDepotHandler(IAppDbContext dbContext) : IRequestHandler<CreateDepotCommand, DepotResult>
{
    public async Task<DepotResult> Handle(CreateDepotCommand request, CancellationToken cancellationToken)
    {
        var depot = new Depot
        {
            Name = request.Name,
            IsActive = request.IsActive,
            OperatingHours = request.OperatingHours != null
                ? OperatingHours.Create(
                    request.OperatingHours.Select(h =>
                        new DailyOperatingHours(h.DayOfWeek, h.OpenTime, h.CloseTime)).ToArray())
                : OperatingHours.CreateWeekdays(new TimeOnly(9, 0), new TimeOnly(17, 0))
        };

        if (request.Address != null)
        {
            depot.Address = new Address
            {
                Street1 = request.Address.Street1,
                Street2 = request.Address.Street2,
                City = request.Address.City,
                State = request.Address.State,
                PostalCode = request.Address.PostalCode,
                CountryCode = request.Address.CountryCode,
                IsResidential = request.Address.IsResidential,
                ContactName = request.Address.ContactName,
                CompanyName = request.Address.CompanyName,
                Phone = request.Address.Phone,
                Email = request.Address.Email
            };
        }

        dbContext.Depots.Add(depot);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DepotResult(depot.Id, depot.Name, depot.IsActive, depot.CreatedAt);
    }
}
