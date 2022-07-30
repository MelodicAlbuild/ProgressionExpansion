using System.Collections.Generic;
using UnityEngine;

public class MoltenStorageManager : MonoBehaviour
{
    float                                            m_totalWeight          = 0;
    int                                              m_tick                 = 0;
    public bool                                      dEnabled               = false;
    HashSet<MoltenStorage> m_storages             = new HashSet<MoltenStorage>();
    HashSet<MoltenStorage> m_storagesActive = new HashSet<MoltenStorage>();

    public static ItemCategory MoltenTinCategoryDefinition   = null;
    public static ItemCategory MoltenCopperCategoryDefinition     = null;
    public static ItemCategory MoltenIronCategoryDefinition = null;
    public static ItemCategory MoltenCobaltCategoryDefinition   = null;
    public static ItemCategory MoltenTitaniumCategoryDefinition = null;
    public static ItemCategory MoltenBronzeCategoryDefinition = null;
    public static ItemCategory MoltenSteelCategoryDefinition = null;
    public static ItemCategory MoltenAluminumCategoryDefinition = null;

    public void EnableClasses(ItemCategory mtCategory, ItemCategory mcCategory, ItemCategory miCategory, ItemCategory mcoCategory, ItemCategory mtiCategory, ItemCategory mbCategory, ItemCategory msCategory, ItemCategory maCategory)
    {

        if(MoltenTinCategoryDefinition == null) 
        {
            MoltenTinCategoryDefinition = mtCategory;
        }

        if (MoltenCopperCategoryDefinition == null)
        {
            MoltenCopperCategoryDefinition = mcCategory;
        }

        if (MoltenIronCategoryDefinition == null)
        {
            MoltenIronCategoryDefinition = miCategory;
        }

        if (MoltenCobaltCategoryDefinition == null)
        {
            MoltenCobaltCategoryDefinition = mcoCategory;
        }

        if (MoltenTitaniumCategoryDefinition == null)
        {
            MoltenTitaniumCategoryDefinition = mtiCategory;
        }

        if (MoltenBronzeCategoryDefinition == null)
        {
            MoltenBronzeCategoryDefinition = mbCategory;
        }

        if (MoltenSteelCategoryDefinition == null)
        {
            MoltenSteelCategoryDefinition = msCategory;
        }

        if (MoltenAluminumCategoryDefinition == null)
        {
            MoltenAluminumCategoryDefinition = maCategory;
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
    public HashSet<MoltenStorage> Storages
    {
        get { return m_storages; }
    }

    /// <summary>
    /// Registers active production module.
    /// </summary>
    public void RegisterActive(MoltenStorage module)
    {
        m_storagesActive.Add(module);
    }

    /// <summary>
    /// Unregisters active production module.
    /// </summary>
    public void UnregisterActive(MoltenStorage module)
    {
        m_storagesActive.Remove(module);
    }

    /// <summary>
    /// Registers production module.
    /// </summary>
    public void Register(MoltenStorage module)
    {
        m_storages.Add(module);
    }

    /// <summary>
    /// Unregisters production module.
    /// </summary>
    public void Unregister(MoltenStorage module)
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

    public float StoreMolten(float amount, ItemCategory category)
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

    public bool RemoveMolten(float amount, ItemCategory category)
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

    public bool RemoveMoltenBatch(InventoryItem[] moltenObjects) {
        for (var i = 0; i < moltenObjects.Length; i++) {
            var storagesForTargetLiquid = m_storages;
            var amountToRemove = moltenObjects[i].Amount;
            foreach (var storage in storagesForTargetLiquid) {
                if (!storage.Remove(ref amountToRemove, moltenObjects[i].Item.Category)) return false;
                break;
            }
        }
        return true;
    }

    public float GetMoltenValue(ItemCategory category)
    {
        float total = 0f;
        foreach(MoltenStorage storage in m_storages)
        {
            total += storage.GetValueType(category);
        }
        return total;
    }
}
