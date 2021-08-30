using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IFluidFactoryUi
{
    ArrayReader<RecipeCategory> Categories { get; }

    GameObject Factory { get; }
    IRecipeGrouping Grouping { get; }

    int QueueSize { get; set; }
    LiquidRecipe Recipe { get; set; }

    bool IsVisible(LiquidRecipe recipe);
    bool IsQueueable(LiquidRecipe recipe);

    /// <summary>
    /// Gets background and overlay color pairs.
    /// </summary>
    Pair<Color, Color> GetColor(LiquidRecipe recipe);
    void WriteInfo(LiquidRecipe recipe, RichTextWriter result);

    int GetModifier();

    void AddRecipe(LiquidRecipe recipe, int count);
}