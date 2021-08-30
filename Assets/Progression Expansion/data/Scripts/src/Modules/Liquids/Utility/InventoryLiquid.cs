using System;

[Serializable]
public struct InventoryLiquid : IEquatable<InventoryLiquid>
{
    public static readonly InventoryLiquid Null = default;

    /// <summary>
    /// Inventory item definition.
    /// </summary>
    [NotNull]
    public LiquidDefinition Item;

    /// <summary>
    /// Item amount, must be positive when item is not null.
    /// </summary>
    public int Amount;

    /// <summary>
    /// Item instance stats.
    /// </summary>
    public PropertySetReader Stats;

    /// <summary>
    /// Gets value indicating whether item is valid (non-null item and positive amount).
    /// </summary>
    public bool IsValid
    {
        get { return Item != null && Amount > 0; }
    }

    /// <summary>
    /// Initializes a new instances of the <see cref="InventoryItem"/> with empty stats.
    /// </summary>
    public InventoryLiquid(LiquidDefinition item, int amount)
    {
        Item = item;
        Amount = amount;
        Stats = PropertySetReader.EmptySet;
    }

    /// <summary>
    /// Initializes a new instances of the <see cref="InventoryItem"/>.
    /// Stats are not copied.
    /// </summary>
    public InventoryLiquid(LiquidDefinition item, int amount, PropertySetReader stats)
    {
        Item = item;
        Amount = amount;
        Stats = stats;
    }

    /// <summary>
    /// Gets value indicating whether items are compatible or not.
    /// Item definitions are tested for reference equality, stats are compared by value, one-by-one.
    /// </summary>
    public bool IsCompatible(LiquidDefinition item, PropertySetReader stats)
    {
        return item == Item && stats.Matches(Stats);
    }

    /// <summary>
    /// Gets value indicating whether items are compatible or not.
    /// Item definitions are tested for reference equality, stats are compared by value, one-by-one.
    /// </summary>
    public bool IsCompatible(InventoryLiquid item)
    {
        return item.Item == Item && item.Stats.Matches(Stats);
    }

    /// <summary>
    /// Compares two inventory items for equality, item, amount and stats must match.
    /// </summary>
    public bool Equals(InventoryLiquid other)
    {
        return Item == other.Item && Amount == other.Amount && Stats.Matches(other.Stats);
    }

    /// <summary>
    /// Returns string in format: {Amount}x {ItemName}
    /// </summary>
    public override string ToString()
    {
        return $"{Amount}x {(Item != null ? Item.name : "null")}";
    }
}
