using System.Collections.Generic;
using UnityEngine;

public class LiquidStorageManager : MonoBehaviour
{
    float m_totalWeight = 0;
    int m_tick = 0;
    public bool dEnabled = false;
    Dictionary<LiquidCategory, HashSet<LiquidStorage>> m_storages = new Dictionary<LiquidCategory, HashSet<LiquidStorage>>();
    HashSet<LiquidStorage> m_activeLiquidStorages = new HashSet<LiquidStorage>();
    HashSet<LiquidInfuser> m_activeLiquidInfusers = new HashSet<LiquidInfuser>();

    public static LiquidCategory WaterCategoryDefinition = null;
    public static LiquidCategory OilCategoryDefinition = null;
    public static LiquidCategory MercuryCategoryDefinition = null;
    public static LiquidCategory MagmaCategoryDefinition = null;

    public void EnableClasses(LiquidCategory wCategory, LiquidCategory oCategory, LiquidCategory meCategory, LiquidCategory maCategory)
    {
        if (!dEnabled)
        {
            if (!m_storages.ContainsKey(wCategory))
            {
                m_storages.Add(wCategory, new HashSet<LiquidStorage>());
            }

            if (!m_storages.ContainsKey(oCategory))
            {
                m_storages.Add(oCategory, new HashSet<LiquidStorage>());
            }

            if (!m_storages.ContainsKey(meCategory))
            {
                m_storages.Add(meCategory, new HashSet<LiquidStorage>());
            }

            if (!m_storages.ContainsKey(maCategory))
            {
                m_storages.Add(maCategory, new HashSet<LiquidStorage>());
            }
            dEnabled = true;
        }

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
    public DictionaryReader<LiquidCategory, HashSet<LiquidStorage>> Storages
    {
        get { return m_storages; }
    }

    /// <summary>
    /// Registers active production module.
    /// </summary>
    public void RegisterActive(LiquidStorage module)
    {
        if (!m_storages.ContainsKey(module.LiquidCategory))
        {
            m_activeLiquidStorages.Add(module);
            m_storages.Add(module.LiquidCategory, m_activeLiquidStorages);
        }
        else
        {
            m_storages.Remove(module.LiquidCategory);
            m_activeLiquidStorages.Add(module);
            m_storages.Add(module.LiquidCategory, m_activeLiquidStorages);
        }
    }

    /// <summary>
    /// Unregisters active production module.
    /// </summary>
    public void UnregisterActive(LiquidStorage module)
    {
        m_activeLiquidStorages.Remove(module);
        m_storages.Remove(module.LiquidCategory);
        m_storages.Add(module.LiquidCategory, m_activeLiquidStorages);
    }

    /// <summary>
    /// Registers production module.
    /// </summary>
    public void Register(LiquidStorage module)
    {
        if (!m_storages.ContainsKey(module.LiquidCategory))
        {
            m_activeLiquidStorages.Add(module);
            m_storages.Add(module.LiquidCategory, m_activeLiquidStorages);
        }
        else
        {
            m_storages.Remove(module.LiquidCategory);
            m_activeLiquidStorages.Add(module);
            m_storages.Add(module.LiquidCategory, m_activeLiquidStorages);
        }
    }

    /// <summary>
    /// Unregisters production module.
    /// </summary>
    public void Unregister(LiquidStorage module)
    {
        m_activeLiquidStorages.Remove(module);
        m_storages.Remove(module.LiquidCategory);
        m_storages.Add(module.LiquidCategory, m_activeLiquidStorages);
    }

    /// <summary>
    /// Registers production module.
    /// </summary>
    public void RegisterInfuser(LiquidInfuser module)
    {
        if(!m_activeLiquidInfusers.Contains(module))
        {
            m_activeLiquidInfusers.Add(module);
        }
    }

    /// <summary>
    /// Unregisters production module.
    /// </summary>
    public void UnregisterInfuser(LiquidInfuser module)
    {
        m_activeLiquidInfusers.Remove(module);
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

    public float StoreLiquid(LiquidDefinition liquid, float amount)
    {
        var storagesForTargetLiquid = m_storages[liquid.Category];
        foreach (var storage in storagesForTargetLiquid)
        {
            if (storage.Store(ref amount))
                return amount;
            break;
        }
        return 0f;
    }

    public bool RemoveLiquid(LiquidDefinition liquid, float amount)
    {
        var storagesForTargetLiquid = m_storages[liquid.Category];
        foreach (var storage in storagesForTargetLiquid)
        {
            if (storage.Remove(ref amount))
                return true;
            break;
        }
        return false;
    }

    public float GetLiquidValue(LiquidCategory category)
    {
        float total = 0f;
        var storagesForTargetLiquid = m_storages[category];
        foreach (var storage in storagesForTargetLiquid)
        {
            total += storage.CurrentStorage;
        }
        return total;
    }

    public void ExportDict()
    {
        foreach (var obj in m_storages)
        {
            foreach (var value in obj.Value)
            {
                Debug.Log("Category: " + obj.Key + " Module: " + value.name);
            }
        }
    }
}
