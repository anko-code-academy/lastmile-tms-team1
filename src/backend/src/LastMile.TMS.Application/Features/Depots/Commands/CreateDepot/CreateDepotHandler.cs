using LastMile.TMS.Application.Common.Interfaces;
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
            IsActive = request.IsActive
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

        // Use provided operating hours or default to Mon-Fri 9:00-17:00
        var scheduleEntries = request.OperatingHours is { Count: > 0 }
            ? request.OperatingHours
                .Where(h => h.OpenTime != null && h.CloseTime != null)
                .Select(h => new ShiftSchedule
                {
                    DayOfWeek = h.DayOfWeek,
                    OpenTime = h.OpenTime!.Value,
                    CloseTime = h.CloseTime!.Value
                }).ToList()
            : CreateDefaultWeekdaySchedule();

        foreach (var schedule in scheduleEntries)
        {
            depot.ShiftSchedules.Add(schedule);
        }

        dbContext.Depots.Add(depot);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DepotResult(depot.Id, depot.Name, depot.IsActive, depot.CreatedAt);
    }

    private static List<ShiftSchedule> CreateDefaultWeekdaySchedule()
    {
        var open = new TimeOnly(9, 0);
        var close = new TimeOnly(17, 0);
        return
        [
            new ShiftSchedule { DayOfWeek = DayOfWeek.Monday, OpenTime = open, CloseTime = close },
            new ShiftSchedule { DayOfWeek = DayOfWeek.Tuesday, OpenTime = open, CloseTime = close },
            new ShiftSchedule { DayOfWeek = DayOfWeek.Wednesday, OpenTime = open, CloseTime = close },
            new ShiftSchedule { DayOfWeek = DayOfWeek.Thursday, OpenTime = open, CloseTime = close },
            new ShiftSchedule { DayOfWeek = DayOfWeek.Friday, OpenTime = open, CloseTime = close }
        ];
    }
}