using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace LastMile.TMS.Infrastructure.Services;

public class LabelService : ILabelService
{
    public byte[] GenerateBarcodePng(string trackingNumber, int width, int height)
    {
        var writer = new ZXing.SkiaSharp.BarcodeWriter
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 10,
                PureBarcode = false
            }
        };

        using var bitmap = writer.Write(trackingNumber);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public byte[] GenerateQrCodePng(string trackingNumber, int size)
    {
        var writer = new ZXing.SkiaSharp.BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = size,
                Width = size,
                Margin = 2
            }
        };

        using var bitmap = writer.Write(trackingNumber);
        using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    public string GenerateZplLabel(Parcel parcel)
    {
        var address = parcel.RecipientAddress;
        var zoneName = parcel.Zone?.Name ?? "Unknown";

        var addressLine = address.Street1;
        if (!string.IsNullOrEmpty(address.Street2))
        {
            addressLine += ", " + address.Street2;
        }

        var cityStateZip = $"{address.City}, {address.State} {address.PostalCode}";
        var recipientName = !string.IsNullOrEmpty(address.ContactName)
            ? address.ContactName
            : (!string.IsNullOrEmpty(address.CompanyName) ? address.CompanyName : "N/A");

        return $@"^XA
^FO50,30^BY3^BCN,100,Y,N,N^FD{parcel.TrackingNumber}^FS
^FO50,150^A0N,30,30^FD{recipientName}^FS
^FO50,190^A0N,25,25^FD{addressLine}^FS
^FO50,220^A0N,25,25^FD{cityStateZip}^FS
^FO50,260^A0N,25,25^FDSort Zone: {zoneName}^FS
^FO50,300^A0N,25,25^FD{{Parcel Type: {parcel.ParcelType?.ToString() ?? "N/A"}}}^FS
^XZ";
    }

    public byte[] GeneratePdfLabel(Parcel parcel)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var address = parcel.RecipientAddress;
        var zoneName = parcel.Zone?.Name ?? "Unknown";
        var barcodeImage = GenerateBarcodePng(parcel.TrackingNumber, 300, 80);
        var qrCodeImage = GenerateQrCodePng(parcel.TrackingNumber, 100);

        var addressLine = address.Street1;
        if (!string.IsNullOrEmpty(address.Street2))
        {
            addressLine += ", " + address.Street2;
        }

        var cityStateZip = $"{address.City}, {address.State} {address.PostalCode}";
        var recipientName = !string.IsNullOrEmpty(address.ContactName)
            ? address.ContactName
            : (!string.IsNullOrEmpty(address.CompanyName) ? address.CompanyName : "N/A");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"Tracking: {parcel.TrackingNumber}").Bold().FontSize(14);
                            col.Item().Text($"Recipient: {recipientName}").FontSize(12);
                            col.Item().Text($" {addressLine}").FontSize(10);
                            col.Item().Text($" {cityStateZip}").FontSize(10);
                            col.Item().Text($" {address.CountryCode}").FontSize(10);
                        });

                        row.ConstantItem(100).Image(qrCodeImage);
                    });

                    column.Item().LineHorizontal(1);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Sort Zone: {zoneName}").FontSize(10);
                        row.RelativeItem().Text($"Parcel Type: {parcel.ParcelType?.ToString() ?? "N/A"}").FontSize(10);
                        row.RelativeItem().Text($"Service: {parcel.ServiceType}").FontSize(10);
                    });

                    column.Item().Image(barcodeImage);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateBulkPdfLabels(List<Parcel> parcels)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            foreach (var parcel in parcels)
            {
                var address = parcel.RecipientAddress;
                var zoneName = parcel.Zone?.Name ?? "Unknown";
                var barcodeImage = GenerateBarcodePng(parcel.TrackingNumber, 300, 80);
                var qrCodeImage = GenerateQrCodePng(parcel.TrackingNumber, 100);

                var addressLine = address.Street1;
                if (!string.IsNullOrEmpty(address.Street2))
                {
                    addressLine += ", " + address.Street2;
                }

                var cityStateZip = $"{address.City}, {address.State} {address.PostalCode}";
                var recipientName = !string.IsNullOrEmpty(address.ContactName)
                    ? address.ContactName
                    : (!string.IsNullOrEmpty(address.CompanyName) ? address.CompanyName : "N/A");

                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Tracking: {parcel.TrackingNumber}").Bold().FontSize(14);
                                col.Item().Text($"Recipient: {recipientName}").FontSize(12);
                                col.Item().Text($" {addressLine}").FontSize(10);
                                col.Item().Text($" {cityStateZip}").FontSize(10);
                                col.Item().Text($" {address.CountryCode}").FontSize(10);
                            });

                            row.ConstantItem(100).Image(qrCodeImage);
                        });

                        column.Item().LineHorizontal(1);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Sort Zone: {zoneName}").FontSize(10);
                            row.RelativeItem().Text($"Parcel Type: {parcel.ParcelType?.ToString() ?? "N/A"}").FontSize(10);
                            row.RelativeItem().Text($"Service: {parcel.ServiceType}").FontSize(10);
                        });

                        column.Item().Image(barcodeImage);
                    });
                });
            }
        });

        return document.GeneratePdf();
    }
}
