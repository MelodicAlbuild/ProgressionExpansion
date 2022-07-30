using System.Linq;
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
            foreach (var liquid in liquidRecipe.Liquids)
            {
                LiquidRecipe r = liquidRecipe;
                if (r.Liquids.First(o => o.Item == liquid.Item).Amount >=
                    FluidSystem.LiquidStorageManagerRef.GetLiquidValue(liquid.Item.Category))
                {
                    return true;
                }

                return false;
            }
        }

        // An additional check for molten.
        if (recipe is MoltenRecipe moltenRecipe)
        {
            foreach (var molten in moltenRecipe.Moltens)
            {
                MoltenRecipe r = moltenRecipe;
                if (r.Moltens.First(o => o.Item == molten.Item).Amount >=
                    MoltenSystem.MoltenStorageManagerRef.GetMoltenValue(molten.Item.Category))
                {
                    return true;
                }

                return false;
            }
        }

        foreach (var item in recipe.Inputs)
        {
            var missingAmount = item.Amount - producer.GetLoadedAmount(item.Item, item.Stats);
            if (missingAmount > 0 && cargo.GetAmount(item.Item, item.Stats, item.Amount) < missingAmount) return false;
            return true;
        }
        return false;
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
        if (producer.ActiveRecipe is LiquidRecipe)
        {
            if (loadedInputs == null && source.RemoveBatch(producer.gameObject, producer.ActiveRecipe.UniqueInputs)
                                 && (producer.ActiveRecipe is LiquidRecipe liquidRecipe && FluidSystem.LiquidStorageManagerRef.RemoveLiquidBatch(liquidRecipe.Liquids)))
            {
                loadedInputs = producer.ActiveRecipe;
                foreach (var obj in liquidRecipe.Liquids)
                {
                    Debug.Log("Removing " + obj.Amount + " " + obj.Item.Name + " Leaving " + FluidSystem.LiquidStorageManagerRef.GetLiquidValue(obj.Item.Category));
                }
                return true;
            }
        }
        // 'source' is OnlineStorage.
        if (producer.ActiveRecipe is MoltenRecipe moltenRecipe)
        {
            switch(moltenRecipe.RecipeType)
            {
                case MoltenType.Melting:
                    if (loadedInputs == null && source.RemoveBatch(producer.gameObject, producer.ActiveRecipe.UniqueInputs))
                    {
                        loadedInputs = producer.ActiveRecipe;
                        return true;
                    }
                    break;
                case MoltenType.Alloying:
                    foreach (var obj in moltenRecipe.Moltens)
                    {
                        Debug.Log("Required Item Type: " + obj.Item.Name + " | Required Item Amount: " + obj.Amount);
                    }
                    if(loadedInputs == null && MoltenSystem.MoltenStorageManagerRef.RemoveMoltenBatch(moltenRecipe.Moltens))
                    {
                        loadedInputs = producer.ActiveRecipe;
                        foreach (var obj in moltenRecipe.Moltens)
                        {
                            Debug.Log("Removing " + obj.Amount + " " + obj.Item.Name + " Leaving " + MoltenSystem.MoltenStorageManagerRef.GetMoltenValue(obj.Item.Category));
                        }
                        return true;
                    }
                    break;
                case MoltenType.Pressing:
                    if (loadedInputs == null && MoltenSystem.MoltenStorageManagerRef.RemoveMoltenBatch(moltenRecipe.Moltens))
                    {
                        loadedInputs = producer.ActiveRecipe;
                        foreach (var obj in moltenRecipe.Moltens)
                        {
                            Debug.Log("Removing " + obj.Amount + " " + obj.Item.Name + " Leaving " + MoltenSystem.MoltenStorageManagerRef.GetMoltenValue(obj.Item.Category));
                        }
                        return true;
                    }
                    break;
                case MoltenType.ItemAlloying:
                case MoltenType.Default:
                    if (loadedInputs == null && source.RemoveBatch(producer.gameObject, producer.ActiveRecipe.UniqueInputs)
                                             && (MoltenSystem.MoltenStorageManagerRef.RemoveMoltenBatch(moltenRecipe.Moltens)))
                    {
                        loadedInputs = producer.ActiveRecipe;
                        foreach (var obj in moltenRecipe.Moltens)
                        {
                            Debug.Log("Removing " + obj.Amount + " " + obj.Item.Name + " Leaving " + MoltenSystem.MoltenStorageManagerRef.GetMoltenValue(obj.Item.Category));
                        }
                        return true;
                    }
                    break;
            }
        }

        if (loadedInputs == null && source.RemoveBatch(producer.gameObject, producer.ActiveRecipe.UniqueInputs))
        {
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
        if (recipe is LiquidRecipe liquidRecipe)
        {
            foreach (InventoryItem obj in liquidRecipe.Liquids)
            {

                if (FluidSystem.LiquidStorageManagerRef.GetLiquidValue(obj.Item.Category) >= obj.Amount)
                {
                    result.CurrentStyle = "Text"; // White text.
                }
                else
                {
                    result.CurrentStyle = "TextError"; // Red text.
                }
                result.Text.ConcatFormat(___m_texts.InputFormat.Text, obj.Amount, obj.Item.Name, null);
                result.Text.ConcatFormat(___m_texts.InputAvailableFormat, FluidSystem.LiquidStorageManagerRef.GetLiquidValue(obj.Item.Category));
                result.Text.AppendLine();
            }
            result.Text.AppendLine();
        } else if (recipe is MoltenRecipe moltenRecipe)
        {
            foreach (InventoryItem obj in moltenRecipe.Moltens)
            {

                if (MoltenSystem.MoltenStorageManagerRef.GetMoltenValue(obj.Item.Category) >= obj.Amount)
                {
                    result.CurrentStyle = "Text"; // White text.
                }
                else
                {
                    result.CurrentStyle = "TextError"; // Red text.
                }
                result.Text.ConcatFormat(___m_texts.InputFormat.Text, obj.Amount, obj.Item.Name, null);
                result.Text.ConcatFormat(___m_texts.InputAvailableFormat, MoltenSystem.MoltenStorageManagerRef.GetMoltenValue(obj.Item.Category));
                result.Text.AppendLine();
            }
            result.Text.AppendLine();
        }
    }
}

[HarmonyPatch]
[UsedImplicitly]
public static class BuildAlloyOutputSystem
{
    private static readonly MethodInfo FACTORY_STATION_TRY_STORE = typeof(FactoryStation).GetMethod("TryStore", BindingFlags.NonPublic | BindingFlags.Instance);
    private static readonly MethodInfo FACTORY_STATION_ON_RECIPE_PRODUCED = typeof(FactoryStation).GetMethod("OnRecipeProduced", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyTargetMethod]
    [UsedImplicitly]
    public static MethodBase TargetMethod()
    {
        return typeof(FactoryStation).GetMethod("TryCreateOutput", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public static bool TryCreateOutput(ref FactoryStation __instance, ref Recipe ___m_loadedInputs)
    {
        if (__instance.ActiveRecipe is MoltenRecipe moltenRecipe)
        {
            switch(moltenRecipe.RecipeType)
            {
                case MoltenType.Melting:
                    MoltenSystem.MoltenStorageManagerRef.StoreMolten(__instance.ActiveRecipe.Output.Amount, __instance.ActiveRecipe.Output.Item.Category);
                    return true;
                case MoltenType.Alloying:
                    MoltenSystem.MoltenStorageManagerRef.StoreMolten(__instance.ActiveRecipe.Output.Amount, __instance.ActiveRecipe.Output.Item.Category);
                    return true;
                case MoltenType.ItemAlloying:
                    MoltenSystem.MoltenStorageManagerRef.StoreMolten(__instance.ActiveRecipe.Output.Amount, __instance.ActiveRecipe.Output.Item.Category);
                    return true;
                case MoltenType.Pressing:
                    return TryCreateOutput_Original(ref __instance, ref ___m_loadedInputs);
                case MoltenType.Default:
                    return TryCreateOutput_Original(ref __instance, ref ___m_loadedInputs);
            }
        }

        // Not our recipe, run the original method.
        return TryCreateOutput_Original(ref __instance, ref ___m_loadedInputs);
    }

    public static bool TryCreateOutput_Original(ref FactoryStation __instance, ref Recipe ___m_loadedInputs)
    {
        if ((bool)FACTORY_STATION_TRY_STORE.Invoke(__instance, new object[] { __instance.ActiveRecipe.Output }))
        {
            ___m_loadedInputs = null;
            FACTORY_STATION_ON_RECIPE_PRODUCED.Invoke(__instance, new object[] { __instance.ActiveRecipe });
            return true;
        }
        return false;
    }

    [UsedImplicitly]
    [HarmonyPrefix]
    public static bool Prefix(ref FactoryStation __instance, ref bool __result, ref Recipe ___m_loadedInputs)
    {
        __result = TryCreateOutput(ref __instance, ref ___m_loadedInputs);
        return false; // So we don't run the original method.
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