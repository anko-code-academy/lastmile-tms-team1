using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Features.Drivers.Common;
using LastMile.TMS.Domain.Entities;
using MediatR;

namespace LastMile.TMS.Application.Features.Drivers.Commands.CreateDriver;

public class CreateDriverHandler(IAppDbContext dbContext) : IRequestHandler<CreateDriverCommand, DriverResult>
{
    public async Task<DriverResult> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
    {
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
            UserId = request.UserId,
            IsActive = request.IsActive
        };

        dbContext.Drivers.Add(driver);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new DriverResult(driver.Id, driver.FirstName, driver.LastName, driver.Email, driver.Phone, driver.LicenseNumber, driver.LicenseExpiryDate, driver.Photo, driver.ZoneId, driver.DepotId, driver.UserId, driver.IsActive, driver.CreatedAt);
    }
}
