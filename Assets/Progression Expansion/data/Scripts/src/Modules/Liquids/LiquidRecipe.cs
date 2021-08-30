using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Liquids/Recipe/Recipe")]
public class LiquidRecipe : Definition
{
    /// <summary>
    /// Recipe inputs, guaranteed to be unique.
    /// </summary>
    public InventoryItem[] Inputs;

    public InventoryLiquid[] Liquids;

    /// <summary>
    /// Recipe output.
    /// </summary>
    public InventoryItem Output = new InventoryItem(null, 1);

    /// <summary>
    /// Recipe icon. When null, output icon is used.
    /// </summary>
    [Tooltip("Recipe icon. When null, output item icon is used"), ValidateWorldTexture]
    public Sprite Icon;

    /// <summary>
    /// Recipe order in production screen.
    /// When two recipes have same order, they will be sorted by name.
    /// </summary>
    public int Order;

    /// <summary>
    /// Required upgrades.
    /// </summary>
    [NotNull]
    public ItemDefinition[] RequiredUpgrades;

    /// <summary>
    /// Recipe categories.
    /// </summary>
    [NotNull]
    public RecipeCategory[] Categories;

    [Tooltip("Base production time in seconds")]
    public float ProductionTime = 5;

    [Validate("Recipe must have at least one input")]
    protected bool HasInput
    {
        get { return Inputs != null && Inputs.Length > 0; }
    }

    [Validate("Recipe must have at least one fluid input")]
    protected bool HasFluidInput
    {
        get { return Liquids != null && Liquids.Length > 0; }
    }

    [Validate("Recipe inputs must be unique and valid")]
    protected bool HasUniqueInputs
    {
        get { return Inputs == null || InventoryUniqueList.IsUnique(new InventoryReader(Inputs, Inputs.Length)); }
    }

    [Validate("All recipe inputs must be valid and EnabledInBuild (when recipe EnabledInBuild)")]
    protected bool AreInputsEnabled
    {
        get { return !EnabledInBuild || Inputs == null || Inputs.All(s => s.Item == null || s.Item.EnabledInBuild); }
    }

    [Validate("All recipe inputs must have positive amount")]
    protected bool AreInputsPositive
    {
        get { return Inputs == null || Inputs.All(s => s.Amount > 0); }
    }

    [Validate("Recipe output cannot be null")]
    protected bool HasOutput
    {
        get { return Output.Item != null; }
    }

    [Validate("Recipe output must be valid and EnabledInBuild (when recipe EnabledInBuild)")]
    protected bool IsOutputEnabled
    {
        get { return !EnabledInBuild || Output.Item == null || Output.Item.EnabledInBuild; }
    }

    [Validate("Recipe output must have positive amount")]
    protected bool IsOutputPositive
    {
        get { return Output.Amount > 0; }
    }

    [Validate("All required upgrades must be EnabledInBuild (when recipe EnabledInBuild)")]
    protected bool AreUpgradesEnabled
    {
        get { return !EnabledInBuild || RequiredUpgrades == null || RequiredUpgrades.All(s => s.EnabledInBuild); }
    }

    [Toggle]
    public bool EnabledInBuild
    {
        get { return RuntimeAssetDatabase.IsEnabledInBuild(AssetId); }
        set { RuntimeAssetDatabase.EnableInBuild(AssetId, value); }
    }

    public InventoryUniqueList UniqueInputs
    {
        get { return InventoryUniqueList.CreateUnsafe(Inputs); }
    }

    void OnValidate()
    {
        if (Inputs != null)
        {
            for (int i = 0; i < Inputs.Length; i++)
            {
                Inputs[i].Amount = Math.Max(1, Inputs[i].Amount);
            }
        }
        Output.Amount = Math.Max(1, Output.Amount);
    }

    /// <summary>
    /// Gets recipe icon when it's not null, otherwise output icon.
    /// </summary>
    public override Sprite GetIcon()
    {
        return Icon != null ? Icon : Output.Item.Icon;
    }

    /// <summary>
    /// Returns true when recipe has at least one of the categories.
    /// </summary>
    public bool HasCategory(ArrayReader<RecipeCategory> categories)
    {
        if (Categories != null)
        {
            foreach (var recipeCategory in Categories)
            {
                if (categories.Contains(recipeCategory))
                    return true;
            }
        }
        return false;
    }
}
