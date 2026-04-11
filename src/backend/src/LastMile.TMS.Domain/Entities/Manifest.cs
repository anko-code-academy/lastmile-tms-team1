using LastMile.TMS.Domain.Common;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Entities;

public class Manifest : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public ManifestStatus Status { get; private set; }
    public Guid DepotId { get; private set; }
    public Depot Depot { get; set; } = null!;
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public ICollection<ManifestItem> Items { get; set; } = new List<ManifestItem>();

    public static Manifest Create(string name, Guid depotId)
    {
        return new Manifest
        {
            Name = name,
            DepotId = depotId,
            Status = ManifestStatus.Open
        };
    }

    public void StartReceiving()
    {
        if (Status != ManifestStatus.Open)
            throw new InvalidOperationException($"Cannot start receiving from status {Status}");

        Status = ManifestStatus.Receiving;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status != ManifestStatus.Receiving)
            throw new InvalidOperationException($"Cannot complete from status {Status}");

        Status = ManifestStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
