using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Drivers.Common;
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

        driver.FirstName = request.FirstName;
        driver.LastName = request.LastName;
        driver.Phone = request.Phone;
        driver.Email = request.Email;
        driver.LicenseNumber = request.LicenseNumber;
        driver.LicenseExpiryDate = request.LicenseExpiryDate;
        driver.Photo = request.Photo;
        driver.ZoneId = request.ZoneId;
        driver.DepotId = request.DepotId;
        driver.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DriverResult(driver.Id, driver.FirstName, driver.LastName, driver.Email, driver.Phone, driver.LicenseNumber, driver.LicenseExpiryDate, driver.Photo, driver.ZoneId, driver.DepotId, driver.UserId, driver.IsActive, driver.CreatedAt);
    }
}
