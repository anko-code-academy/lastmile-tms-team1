using HotChocolate.Authorization;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Infrastructure.Services;
using LastMile.TMS.Persistence;
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

    [HttpGet("bin/{binId}/zpl")]
    public async Task<IActionResult> GetBinZplLabel(Guid binId)
    {
        var bin = await context.Bins
            .Include(b => b.Zone)
            .FirstOrDefaultAsync(b => b.Id == binId);

        if (bin == null)
            return NotFound();

        var zpl = labelService.GenerateBinZplLabel(bin);
        return Content(zpl, "text/plain");
    }

    [HttpGet("bin/{binId}/pdf")]
    public async Task<IActionResult> GetBinPdfLabel(Guid binId)
    {
        var bin = await context.Bins
            .Include(b => b.Zone)
            .FirstOrDefaultAsync(b => b.Id == binId);

        if (bin == null)
            return NotFound();

        var pdfBytes = labelService.GenerateBinLabelPdf(bin);
        return File(pdfBytes, "application/pdf", $"bin-label-{bin.Label}.pdf");
    }

    [HttpGet("bin/{binId}/png")]
    public async Task<IActionResult> GetBinPngLabel(Guid binId)
    {
        var bin = await context.Bins
            .Include(b => b.Zone)
            .FirstOrDefaultAsync(b => b.Id == binId);

        if (bin == null)
            return NotFound();

        var pngBytes = labelService.GenerateBinLabelPng(bin.Label, 300, 100);
        return File(pngBytes, "image/png", $"bin-label-{bin.Label}.png");
    }
}
