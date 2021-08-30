using UnityEngine;

public class LiquidStorage : MonoBehaviour
{
    [NotNull, SerializeField]
    private GridModule m_module = null;

    [SerializeField]
    private PackableModule m_packableModule = null;

    [SerializeField]
    private LiquidCategory m_liquidCategory = null;

    private LiquidStorageManager m_productionGroup;

    [SerializeField, Tooltip("How many production points this module provides")]
    private int m_points = 1;

    // Max Amount of TOTAL Storage
    [SerializeField]
    private float m_MaxStorage = 1000f;

    // Current Storage
    private float m_CurrentStorage = 0f;

    public float CurrentStorage
    {
        get { return m_CurrentStorage; }
    }

    public LiquidCategory LiquidCategory
    {
        get { return m_liquidCategory; }
    }

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
        m_CurrentStorage++;
    }

    public bool Store(ref float amount)
    {
        float space = m_MaxStorage - m_CurrentStorage;
        if (space >= amount)
        {
            m_CurrentStorage += amount;
            amount = 0;
            return true;
        }
        else
        {
            m_CurrentStorage = m_MaxStorage;
            amount -= space;
            return false;
        }
    }

    public bool Remove(ref float amount)
    {
        if (m_CurrentStorage >= amount)
        {
            m_CurrentStorage -= amount;
            amount = 0;
            return true;
        }
        else
        {
            amount -= m_CurrentStorage;
            m_CurrentStorage = 0f;
            return false;
        }
    }

    public void AssignCategory(LiquidCategory category)
    {
        m_liquidCategory = category;
    }
}
