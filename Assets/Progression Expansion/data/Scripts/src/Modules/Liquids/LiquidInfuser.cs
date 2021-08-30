using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LiquidInfuser : MonoBehaviour
{
    private LiquidStorageManager m_lStorageManager;

    [NotNull, SerializeField, FormerlySerializedAs("m_module"), FormerlySerializedAs("m_itemPrefab")]
    private ItemObject m_itemObject = null;

    [SerializeField]
    private PackableModule m_packableModule = null;

    [NotNull, SerializeField]
    private FactoryType m_factoryType = null;

    [SerializeField, Tooltip("How many production points this module provides")]
    private int m_points = 1;

    [NotNull, SerializeField, Tooltip("Categories that can be manufactured by this module")]
    private RecipeCategory[] m_categories = EmptyArray<RecipeCategory>.Value;

    private TrainProduction.GroupInfo m_productionGroup;

    public FactoryType FactoryType
    {
        get { return m_factoryType; }
    }

    public int Points
    {
        get { return m_points; }
    }

    public PackableModule PackableModule
    {
        get { return m_packableModule; }
    }

    public ItemObject ItemObject
    {
        get { return m_itemObject; }
    }

    public ArrayReader<RecipeCategory> Categories
    {
        get { return m_categories ?? EmptyArray<RecipeCategory>.Value; }
    }
    private void Awake()
    {
        if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out LiquidStorageManager system))
        {
            system = production.gameObject.AddComponent<LiquidStorageManager>();
            FluidSystem.LiquidStorageManagerRef = system;
            m_lStorageManager = system;
        }
        else
        {
            m_lStorageManager = FluidSystem.LiquidStorageManagerRef;
        }
    }

    private void OnEnable()
    {
        m_lStorageManager.RegisterInfuser(this);
    }

    private void OnDisable()
    {
        m_lStorageManager.UnregisterInfuser(this);
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
}
