using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Alloys/Recipe/Recipe")]
public class MoltenRecipe : Recipe {
    [Header("Molten Addons")]
    public InventoryItem[] Moltens;

    public MoltenType RecipeType;

    [Validate("Recipe must have at least one molten input")]
    protected bool HasMoltenInput => Moltens != null && Moltens.Length > 0;

    public void OnValidate() {
        if (Inputs != null) {
            for (var i = 0; i < Inputs.Length; i++) {
                Inputs[i].Amount = Math.Max(1, Inputs[i].Amount);
            }
        }
        if (Moltens != null) {
            for (var i = 0; i < Moltens.Length; i++) {
                Moltens[i].Amount = Math.Max(1, Moltens[i].Amount);
            }
        }
        Output.Amount = Math.Max(1, Output.Amount);
    }
}

public enum MoltenType
{
    Melting,
    Alloying,
    ItemAlloying,
    Pressing,
    Default
}