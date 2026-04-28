namespace Togo.Domain.Entities;

public class PrescriptionItem
{
    private PrescriptionItem() { }

    private PrescriptionItem(long prescriptionId, long? productId, decimal quantity, string unit, string dosage, int? durationDays)
    {
        ValidateId(prescriptionId, nameof(prescriptionId));
        if (productId.HasValue)
        {
            ValidateId(productId.Value, nameof(productId));
        }

        ValidateQuantity(quantity);
        ValidateRequired(unit, nameof(unit));
        ValidateRequired(dosage, nameof(dosage));
        ValidateDuration(durationDays);

        PrescriptionId = prescriptionId;
        ProductId = productId;
        Quantity = quantity;
        Unit = unit.Trim();
        Dosage = dosage.Trim();
        DurationDays = durationDays;
    }

    public long Id { get; private set; }
    public long PrescriptionId { get; private set; }
    public long? ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public string Dosage { get; private set; } = string.Empty;
    public int? DurationDays { get; private set; }

    public static PrescriptionItem Create(long prescriptionId, long? productId, decimal quantity, string unit, string dosage, int? durationDays) =>
        new(prescriptionId, productId, quantity, unit, dosage, durationDays);

    public void Update(decimal quantity, string unit, string dosage, int? durationDays)
    {
        ValidateQuantity(quantity);
        ValidateRequired(unit, nameof(unit));
        ValidateRequired(dosage, nameof(dosage));
        ValidateDuration(durationDays);

        Quantity = quantity;
        Unit = unit.Trim();
        Dosage = dosage.Trim();
        DurationDays = durationDays;
    }

    private static void ValidateId(long id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Id must be greater than zero");
        }
    }

    private static void ValidateQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero");
        }
    }

    private static void ValidateRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required", paramName);
        }
    }

    private static void ValidateDuration(int? durationDays)
    {
        if (durationDays.HasValue && durationDays.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationDays), "Duration must be greater than zero");
        }
    }
}
