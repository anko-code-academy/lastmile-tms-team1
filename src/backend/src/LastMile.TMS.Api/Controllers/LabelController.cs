using LastMile.TMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.Controllers;

[ApiController]
[Route("labels")]
public class LabelController(
    ILabelService labelService,
    IAppDbContext context) : ControllerBase
{
    [HttpGet("{parcelId}/zpl")]
    public async Task<IActionResult> GetZplLabel(Guid parcelId)
    {
        var parcel = await context.Parcels
            .Include(p => p.RecipientAddress)
            .Include(p => p.Zone)
            .FirstOrDefaultAsync(p => p.Id == parcelId);

        if (parcel == null)
            return NotFound();

        var zpl = labelService.GenerateZplLabel(parcel);
        return Content(zpl, "text/plain");
    }

    [HttpGet("{parcelId}/pdf")]
    public async Task<IActionResult> GetPdfLabel(Guid parcelId)
    {
        var parcel = await context.Parcels
            .Include(p => p.RecipientAddress)
            .Include(p => p.Zone)
            .FirstOrDefaultAsync(p => p.Id == parcelId);

        if (parcel == null)
            return NotFound();

        var pdfBytes = labelService.GeneratePdfLabel(parcel);
        return File(pdfBytes, "application/pdf", $"label-{parcel.TrackingNumber}.pdf");
    }

    [HttpPost("bulk/pdf")]
    public async Task<IActionResult> GetBulkPdfLabels([FromBody] List<Guid> parcelIds)
    {
        var parcels = await context.Parcels
            .Include(p => p.RecipientAddress)
            .Include(p => p.Zone)
            .Where(p => parcelIds.Contains(p.Id))
            .ToListAsync();

        if (parcels.Count == 0)
            return NotFound();

        var pdfBytes = labelService.GenerateBulkPdfLabels(parcels);
        return File(pdfBytes, "application/pdf", "labels-bulk.pdf");
    }
}
