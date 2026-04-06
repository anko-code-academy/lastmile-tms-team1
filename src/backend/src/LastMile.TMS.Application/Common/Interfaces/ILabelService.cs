using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Application.Common.Interfaces;

public interface ILabelService
{
    byte[] GenerateBarcodePng(string trackingNumber, int width, int height);
    byte[] GenerateQrCodePng(string trackingNumber, int size);
    string GenerateZplLabel(Parcel parcel);
    byte[] GeneratePdfLabel(Parcel parcel);
    byte[] GenerateBulkPdfLabels(List<Parcel> parcels);

    byte[] GenerateBinLabelPng(string binLabel, int width, int height);
    string GenerateBinZplLabel(Bin bin);
    byte[] GenerateBinLabelPdf(Bin bin);
}
