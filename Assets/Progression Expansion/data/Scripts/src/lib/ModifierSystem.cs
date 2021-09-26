using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModifierSystem
{
    public ModifierSystem()
    {
        // Titanium Ingot
        RemoveRecipe(GUID.Parse("3f46e5d7ea446ed42a5d62589436fe0f"));
        RemoveRecipe(GUID.Parse("ea978cf7f016d2f49939509a7e2e3199"));

        // Copper Ingot
        RemoveRecipe(GUID.Parse("aba967e2915805a40b66885d19528f9d"));
        RemoveRecipe(GUID.Parse("0d96036a5fee14d4dbbe8419d755d50a"));

        // Iron Ingot
        RemoveRecipe(GUID.Parse("c335abb97c10d4f4589aa02a4245cc74"));
        RemoveRecipe(GUID.Parse("0de269afbd81460438fa04642b03a6c8"));
    }

    private void RemoveRecipe(GUID recipeID)
    {
        var recipeLookup = RuntimeAssetDatabase.Get<Recipe>().Where(recipe => recipe.AssetId == recipeID).FirstOrDefault();
        recipeLookup.Inputs = new InventoryItem[] { };
        recipeLookup.Categories = new RecipeCategory[] { };
    }
}
