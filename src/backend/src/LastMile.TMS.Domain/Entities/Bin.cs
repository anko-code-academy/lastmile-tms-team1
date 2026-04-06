using LastMile.TMS.Domain.Common;

namespace LastMile.TMS.Domain.Entities;

public class Bin : BaseAuditableEntity
{
    public string Label { get; private set; } = string.Empty;
    public string? Description { get; set; }
    public int Aisle { get; set; }
    public int Slot { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid ZoneId { get; set; }
    public Zone Zone { get; set; } = null!;

    public static string GenerateLabel(string depotNumber, string zoneLetter, int aisle, int slot)
        => $"D{depotNumber}-{zoneLetter}-A{aisle}-{slot:D2}";

    public void SetLabel(string depotNumber, string zoneLetter)
    {
        Label = GenerateLabel(depotNumber, zoneLetter, Aisle, Slot);
    }
}
