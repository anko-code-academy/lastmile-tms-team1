using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Drivers.Common;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Features.Drivers.Commands.UpdateDriver;

public class UpdateDriverHandler(IAppDbContext dbContext) : IRequestHandler<UpdateDriverCommand, DriverResult>
{
    public async Task<DriverResult> Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = await dbContext.Drivers
            .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Driver with ID {request.Id} not found.");

        driver.LicenseNumber = request.LicenseNumber;
        driver.LicenseExpiryDate = request.LicenseExpiryDate;
        driver.Photo = request.Photo;

        // Sync ShiftSchedules
        var existingSchedules = await dbContext.ShiftSchedules
            .Where(s => s.DriverId == driver.Id)
            .ToListAsync(cancellationToken);

        if (request.ShiftSchedules != null)
        {
            var requestedDays = request.ShiftSchedules
                .Select(h => h.DayOfWeek)
                .ToHashSet();

            // Remove schedules for days NOT in request
            foreach (var existing in existingSchedules)
            {
                if (!requestedDays.Contains(existing.DayOfWeek))
                {
                    dbContext.ShiftSchedules.Remove(existing);
                }
            }

            foreach (var h in request.ShiftSchedules)
            {
                var existing = existingSchedules
                    .FirstOrDefault(s => s.DayOfWeek == h.DayOfWeek);

                if (existing != null)
                {
                    if (h.OpenTime == null || h.CloseTime == null)
                    {
                        dbContext.ShiftSchedules.Remove(existing);
                    }
                    else
                    {
                        existing.OpenTime = h.OpenTime.Value;
                        existing.CloseTime = h.CloseTime.Value;
                    }
                }
                else if (h.OpenTime != null && h.CloseTime != null)
                {
                    dbContext.ShiftSchedules.Add(new ShiftSchedule
                    {
                        DayOfWeek = h.DayOfWeek,
                        OpenTime = h.OpenTime.Value,
                        CloseTime = h.CloseTime.Value,
                        DriverId = driver.Id
                    });
                }
            }
        }
        else
        {
            // If null, don't modify existing schedules
        }

        // Sync DaysOff
        var existingDaysOff = await dbContext.DaysOff
            .Where(d => d.DriverId == driver.Id)
            .ToListAsync(cancellationToken);

        if (request.DaysOff != null)
        {
            var requestedDates = request.DaysOff
                .Select(d => d.Date.Date)
                .ToHashSet();

            // Remove days off not in request
            foreach (var existing in existingDaysOff)
            {
                if (!requestedDates.Contains(existing.Date.Date))
                {
                    dbContext.DaysOff.Remove(existing);
                }
            }

            foreach (var d in request.DaysOff)
            {
                var existing = existingDaysOff
                    .FirstOrDefault(x => x.Date.Date == d.Date.Date);

                if (existing == null)
                {
                    dbContext.DaysOff.Add(new DayOff
                    {
                        Date = d.Date,
                        DriverId = driver.Id
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Reload shift schedules and days off after save
        var updatedSchedules = await dbContext.ShiftSchedules
            .Where(s => s.DriverId == driver.Id)
            .ToListAsync(cancellationToken);
        var updatedDaysOff = await dbContext.DaysOff
            .Where(d => d.DriverId == driver.Id)
            .ToListAsync(cancellationToken);

        var shiftScheduleResults = updatedSchedules.Select(s => new ShiftScheduleResult(s.DayOfWeek, s.OpenTime, s.CloseTime)).ToList();
        var dayOffResults = updatedDaysOff.Select(d => new DayOffResult(d.Date)).ToList();

        return new DriverResult(driver.Id, driver.LicenseNumber, driver.LicenseExpiryDate, driver.Photo, driver.UserId, driver.CreatedAt, shiftScheduleResults, dayOffResults);
    }
}
