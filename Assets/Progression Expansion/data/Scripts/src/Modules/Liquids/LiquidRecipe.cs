using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Liquids/Recipe/Recipe")]
public class LiquidRecipe : Recipe {
    public InventoryItem[] Liquids;

    [Validate("Recipe must have at least one fluid input")]
    protected bool HasFluidInput => Liquids != null && Liquids.Length > 0;

    public void OnValidate() {
        if (Inputs != null) {
            for (var i = 0; i < Inputs.Length; i++) {
                Inputs[i].Amount = Math.Max(1, Inputs[i].Amount);
            }
        }
        if (Liquids != null) {
            for (var i = 0; i < Liquids.Length; i++) {
                Liquids[i].Amount = Math.Max(1, Liquids[i].Amount);
            }
        }
        Output.Amount = Math.Max(1, Output.Amount);
    }
}