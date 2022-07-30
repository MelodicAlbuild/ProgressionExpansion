using System.Collections.Generic;
using UnityEngine;

public class LiquidStorageManager : MonoBehaviour
{
    float                                            m_totalWeight          = 0;
    int                                              m_tick                 = 0;
    public bool                                      dEnabled               = false;
    HashSet<LiquidStorage> m_storages             = new HashSet<LiquidStorage>();
    HashSet<LiquidStorage> m_storagesActive = new HashSet<LiquidStorage>();

    public static ItemCategory WaterCategoryDefinition   = null;
    public static ItemCategory OilCategoryDefinition     = null;
    public static ItemCategory MercuryCategoryDefinition = null;
    public static ItemCategory MagmaCategoryDefinition   = null;

    public void EnableClasses(ItemCategory wCategory, ItemCategory oCategory, ItemCategory meCategory, ItemCategory maCategory)
    {

        if(WaterCategoryDefinition == null) 
        {
            WaterCategoryDefinition = wCategory;
        }

        if (OilCategoryDefinition == null)
        {
            OilCategoryDefinition = oCategory;
        }

        if (MercuryCategoryDefinition == null)
        {
            MercuryCategoryDefinition = meCategory;
        }

        if (MagmaCategoryDefinition == null)
        {
            MagmaCategoryDefinition = maCategory;
        }
    }

    /// <summary>
    /// Gets total weight for current frame, automatically resets when frame advances.
    /// </summary>
    public float TotalWeight
    {
        get { return Time.frameCount == m_tick ? m_totalWeight : 0.0f; }
    }

    /// <summary>
    /// Gets all production modules.
    /// </summary>
    public HashSet<LiquidStorage> Storages
    {
        get { return m_storages; }
    }

    /// <summary>
    /// Registers active production module.
    /// </summary>
    public void RegisterActive(LiquidStorage module)
    {
        m_storagesActive.Add(module);
    }

    /// <summary>
    /// Unregisters active production module.
    /// </summary>
    public void UnregisterActive(LiquidStorage module)
    {
        m_storagesActive.Remove(module);
    }

    /// <summary>
    /// Registers production module.
    /// </summary>
    public void Register(LiquidStorage module)
    {
        m_storages.Add(module);
    }

    /// <summary>
    /// Unregisters production module.
    /// </summary>
    public void Unregister(LiquidStorage module)
    {
        m_storages.Remove(module);
    }

    /// <summary>
    /// Adds weight for current frame.
    /// </summary>
    public void AddWeightForFrame(float weight)
    {
        if (Time.frameCount != m_tick)
        {
            m_totalWeight = 0;
            m_tick = Time.frameCount;
        }
        m_totalWeight += weight;
    }

    public float StoreLiquid(float amount, ItemCategory category)
    {
        var storagesForTargetLiquid = m_storages;
        foreach (var storage in storagesForTargetLiquid)
        {
            if (storage.Store(ref amount, ref category))
                return amount;
            break;
        }
        return 0f;
    }

    public bool RemoveLiquid(float amount, ItemCategory category)
    {
        var storagesForTargetLiquid = m_storages;
        foreach (var storage in storagesForTargetLiquid)
        {
            if (storage.Remove(ref amount, category))
                return true;
            break;
        }
        return false;
    }

    public bool RemoveLiquidBatch(InventoryItem[] liquids) {
        for (var i = 0; i < liquids.Length; i++) {
            var storagesForTargetLiquid = m_storages;
            var amountToRemove = liquids[i].Amount;
            foreach (var storage in storagesForTargetLiquid) {
                if (!storage.Remove(ref amountToRemove, liquids[i].Item.Category)) return false;
                break;
            }
        }
        return true;
    }

    public float GetLiquidValue(ItemCategory category)
    {
        float total = 0f;
        foreach(LiquidStorage storage in m_storages)
        {
            total += storage.GetValueType(category);
        }
        return total;
    }
}
