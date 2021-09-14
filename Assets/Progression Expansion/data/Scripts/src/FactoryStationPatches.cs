using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using System.Text;
using UnityEngine;

[HarmonyPatch]
public static class CheckRecipeInputsPatch1 {
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod() {
        return typeof(FactoryStation).GetMethod(nameof(FactoryStation.HasInputs), BindingFlags.Public | BindingFlags.Instance);
    }

    // Original; mostly copied from the original `FactoryStation`. Split out to `HasMissingItems` for the liquid check too.
    public static bool HasInputs(Producer producer, Recipe recipe, OnlineCargo cargo) {
        if (cargo == null) return false;

        // An additional check for liquids.
        if (recipe is LiquidRecipe liquidRecipe) {
            foreach (var liquid in liquidRecipe.Liquids) {
                FluidSystem.LiquidStorageManagerRef.GetLiquidValue(liquid.Item.Category);
            }
        }

        foreach (var item in recipe.Inputs) {
            var missingAmount = item.Amount - producer.GetLoadedAmount(item.Item, item.Stats);
            if (missingAmount > 0 && cargo.GetAmount(item.Item, item.Stats, item.Amount) < missingAmount) return false;
        }
        return true;
    }

    /**
     * Returns true if any of the items are missing.
     */
    private static bool HasMissingItems(Producer producer, IInventory cargo, InventoryItem item) {
        var missingAmount = item.Amount - producer.GetLoadedAmount(item.Item, item.Stats);
        if (missingAmount > 0 && cargo.GetAmount(item.Item, item.Stats, item.Amount) < missingAmount) return true;
        return false;
    }

    [HarmonyPrefix]
    public static bool Prefix(ref FactoryStation __instance, ref Recipe recipe, ref bool __result, ref OnlineCargo ___m_cargo) {
        __result = HasInputs(__instance, recipe, ___m_cargo);
        return false; // So we don't run the original method.
    }
}

[HarmonyPatch]
public static class CheckRecipeInputsPatch2 {
    [HarmonyTargetMethod]
    public static MethodBase TargetMethod() {
        return typeof(FactoryStation).GetMethod("TryLoadInputs", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public static bool TryLoadInputs(Producer producer, InventoryBase source, ref Recipe loadedInputs) {
        // 'source' is OnlineStorage.
        if (loadedInputs == null && source.RemoveBatch(producer.gameObject, producer.ActiveRecipe.UniqueInputs)
                                 && (producer.ActiveRecipe is LiquidRecipe liquidRecipe && FluidSystem.LiquidStorageManagerRef.RemoveLiquidBatch(liquidRecipe.Liquids) || true)) {
            loadedInputs = producer.ActiveRecipe;
            return true;
        }
        return false;
    }

    [HarmonyPrefix]
    public static bool Prefix(ref FactoryStation __instance, ref InventoryBase source, ref bool __result, ref Recipe ___m_loadedInputs) {
        __result = TryLoadInputs(__instance, source, ref ___m_loadedInputs);
        return false; // So we don't run the original method.
    }
}

[HarmonyPatch]
[UsedImplicitly]
public static class ShowHaveCountPatch
{
    [HarmonyTargetMethod]
    [UsedImplicitly]
    public static MethodBase TargetMethod()
    {
        return typeof(FactoryStation).GetMethod("WriteInfoInputs", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [UsedImplicitly]
    [HarmonyPostfix]
    public static void Postfix(ref Recipe recipe, ref RichTextWriter result, ref FactoryTexts ___m_texts)
    {
        Debug.Log("" + recipe.GetType());
        if (recipe is LiquidRecipe liquidRecipe)
        {
            foreach (InventoryItem obj in liquidRecipe.Liquids)
            {

                if (FluidSystem.LiquidStorageManagerRef.GetLiquidValue(LiquidStorageManager.WaterCategoryDefinition) >= obj.Amount)
                {
                    result.CurrentStyle = "Text"; // White text.
                }
                else
                {
                    result.CurrentStyle = "TextError"; // Red text.
                }
                result.Text.ConcatFormat(___m_texts.InputFormat.Text, obj.Amount, obj.Item.Name, null);
                result.Text.ConcatFormat(___m_texts.InputAvailableFormat, FluidSystem.LiquidStorageManagerRef.GetLiquidValue(LiquidStorageManager.WaterCategoryDefinition));
            }
        }
        result.Text.AppendLine();
    }
}

//[HarmonyPatch]
//[UsedImplicitly]
//public static class FixUIHoverPatch
//{
//    [HarmonyTargetMethod]
//    [UsedImplicitly]
//    public static MethodBase TargetMethod()
//    {
//        return typeof(Producer).GetMethod("WriteInfo", BindingFlags.NonPublic | BindingFlags.Instance);
//    }

//    [UsedImplicitly]
//    [HarmonyPostfix]
//    public static void Postfix(ref Recipe __instance, ref RichTextWriter result, ref FactoryTexts ___m_texts)
//    {
//        if (__instance is LiquidRecipe liquidRecipe)
//        {
//            foreach (InventoryItem obj in liquidRecipe.Liquids)
//            {

//                if (FluidSystem.LiquidStorageManagerRef.GetLiquidValue(LiquidStorageManager.WaterCategoryDefinition) >= obj.Amount)
//                {
//                    result.CurrentStyle = "Text"; // White text.
//                }
//                else
//                {
//                    result.CurrentStyle = "TextError"; // Red text.
//                }
//                result.Text.ConcatFormat(___m_texts.InputFormat.Text, obj.Amount, obj.Item.Name, null);
//                result.Text.ConcatFormat(___m_texts.InputAvailableFormat, FluidSystem.LiquidStorageManagerRef.GetLiquidValue(LiquidStorageManager.WaterCategoryDefinition));
//            }
//        }
//        result.Text.AppendLine();
//    }
//}