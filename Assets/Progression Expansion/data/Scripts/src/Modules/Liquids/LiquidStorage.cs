using System.Collections.Generic;
using UnityEngine;

public class LiquidStorage : MonoBehaviour
{
    [NotNull, SerializeField]
    private GridModule m_module = null;

    [SerializeField]
    private PackableModule m_packableModule = null;

    private LiquidStorageManager m_productionGroup;

    [SerializeField, Tooltip("How many production points this module provides")]
    private int m_points = 1;

    // Max Amount of TOTAL Storage
    [SerializeField]
    private float m_MaxStorage = 1000f;

    Dictionary<ItemCategory, float> storageSystem = new Dictionary<ItemCategory, float>();

    public DictionaryReader<ItemCategory, float> StorageSystem
    {
        get { return storageSystem; }
    }

    private float m_CurrentTotalStorage = 0f;

    public int Points
    {
        get { return m_points; }
    }

    private void Reset()
    {
        m_module = GetComponent<GridModule>();
        m_packableModule = GetComponent<PackableModule>();
    }

    private void Awake()
    {
        if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out LiquidStorageManager system))
        {
            system = production.gameObject.AddComponent<LiquidStorageManager>();
            FluidSystem.LiquidStorageManagerRef = system;
            m_productionGroup = system;
        }
        else
        {
            m_productionGroup = FluidSystem.LiquidStorageManagerRef;
        }

        if (m_productionGroup != null)
        {
            m_productionGroup.Register(this);
        }
        //InvokeRepeating("Store1", 1f, 5f);
    }

    private void OnDestroy()
    {
        if (m_productionGroup != null)
        {
            m_productionGroup.Unregister(this);
            m_productionGroup = null;
        }
    }

    private void OnEnable()
    {
        if (m_productionGroup != null)
        {
            m_productionGroup.RegisterActive(this);
        }
    }

    private void OnDisable()
    {
        if (m_productionGroup != null)
        {
            m_productionGroup.UnregisterActive(this);
        }
    }

    private void Update()
    {
        UpdateRunningState();
    }

    private void UpdateRunningState()
    {
        if (m_packableModule != null)
        {
            m_packableModule.SetRunningState(m_productionGroup != null && m_productionGroup.TotalWeight > 0);
        }
    }

    public void Store1()
    {
        m_CurrentTotalStorage++;
    }

    public bool Store(ref float amount, ref ItemCategory liquidType)
    {
        float space = m_MaxStorage - m_CurrentTotalStorage;
        if (space >= amount)
        {
            if(!storageSystem.ContainsKey(liquidType))
            {
                storageSystem.Add(liquidType, amount);
            } else
            {
                storageSystem[liquidType] += amount;
            }
            m_CurrentTotalStorage += amount;
            amount = 0;
            return true;
        }
        else
        {
            float useSpace = amount - space;
            float totalUseSpace = amount - useSpace;
            if (!storageSystem.ContainsKey(liquidType))
            {
                storageSystem.Add(liquidType, totalUseSpace);
            }
            else
            {
                storageSystem[liquidType] += totalUseSpace;
            }
            m_CurrentTotalStorage = m_MaxStorage;
            amount -= space;
            return false;
        }
    }

    public bool Remove(ref float amount, ref ItemCategory category)
    {
        if (m_CurrentTotalStorage >= amount)
        {
            if (!storageSystem.ContainsKey(category))
            {
                storageSystem.Add(category, 0f);
                return false;
            }
            else
            {
                storageSystem[category] -= amount;
            }
            m_CurrentTotalStorage -= amount;
            amount = 0;
            return true;
        }
        else
        {
            if (!storageSystem.ContainsKey(category))
            {
                storageSystem.Add(category, 0f);
                return false;
            }
            else
            {
                storageSystem[category] = 0;
            }
            amount -= m_CurrentTotalStorage;
            m_CurrentTotalStorage = 0f;
            return false;
        }
    }

    public bool Remove(ref int amount, ref ItemCategory category)
    {
        if (m_CurrentTotalStorage >= amount)
        {
            m_CurrentTotalStorage -= amount;
            amount = 0;
            return true;
        }
        else
        {
            amount           = (int) (amount - m_CurrentTotalStorage);
            m_CurrentTotalStorage = 0f;
            return false;
        }
    }

    public float GetValueType(ItemCategory category)
    {
        if(!storageSystem.ContainsKey(category))
        {
            storageSystem.Add(category, 0f);
            return 0f;
        } else
        {
            return storageSystem[category];
        }
    }
}
