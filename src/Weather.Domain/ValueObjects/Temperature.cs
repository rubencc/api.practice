namespace Weather.Domain.ValueObjects;

public class Temperature : IEquatable<Temperature>
{
    private Temperature(double value, string unit)
    {
        Value = value;
        Unit = unit.ToUpperInvariant();
    }

    public double Value { get; }
    public string Unit { get; } // "C", "F", "K"

    public static Temperature Create(double value, string unit)
    {
        return new Temperature(value, unit);
    }

    public bool Equals(Temperature? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value) && Unit == other.Unit;
    }


    public override bool Equals(object? obj) => Equals(obj as Temperature);

    public override int GetHashCode() => HashCode.Combine(Value, Unit);

    public override string ToString() => $"{Value:F1}°{Unit}";

}