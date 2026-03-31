using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Drivers.Common;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;

public class CreateDriverHandler(IAppDbContext dbContext) : IRequestHandler<CreateDriverCommand, DriverResult>
{
    public async Task<DriverResult> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"No user found with email {request.Email}");

        var driver = new Driver
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            LicenseNumber = request.LicenseNumber,
            LicenseExpiryDate = request.LicenseExpiryDate,
            Photo = request.Photo,
            ZoneId = request.ZoneId,
            DepotId = request.DepotId,
            UserId = user.Id,
            IsActive = request.IsActive
        };

        // Add ShiftSchedules
        if (request.ShiftSchedules != null)
        {
            foreach (var s in request.ShiftSchedules)
            {
                if (s.OpenTime != null && s.CloseTime != null)
                {
                    driver.ShiftSchedules.Add(new ShiftSchedule
                    {
                        DayOfWeek = s.DayOfWeek,
                        OpenTime = s.OpenTime.Value,
                        CloseTime = s.CloseTime.Value,
                        DriverId = driver.Id
                    });
                }
            }
        }

        // Add DaysOff
        if (request.DaysOff != null)
        {
            foreach (var d in request.DaysOff)
            {
                driver.DaysOff.Add(new DayOff
                {
                    Date = d.Date,
                    DriverId = driver.Id
                });
            }
        }

        dbContext.Drivers.Add(driver);
        await dbContext.SaveChangesAsync(cancellationToken);

        var shiftScheduleResults = driver.ShiftSchedules.Select(s => new ShiftScheduleResult(s.DayOfWeek, s.OpenTime, s.CloseTime)).ToList();
        var dayOffResults = driver.DaysOff.Select(d => new DayOffResult(d.Date)).ToList();

        return new DriverResult(driver.Id, driver.FirstName, driver.LastName, driver.Email, driver.Phone, driver.LicenseNumber, driver.LicenseExpiryDate, driver.Photo, driver.ZoneId, driver.DepotId, driver.UserId, driver.IsActive, driver.CreatedAt, shiftScheduleResults, dayOffResults);
    }
}
