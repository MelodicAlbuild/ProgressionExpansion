using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidExtractor : MonoBehaviour
{
    [SerializeField]
    private float m_ExtractionRate;
    [SerializeField]
    private float m_ExtractionAmount;
    [SerializeField]
    private ItemDefinition m_CurrentDefinition;

    public void Awake()
    {
        if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out LiquidStorageManager system))
        {
            system = production.gameObject.AddComponent<LiquidStorageManager>();
            FluidSystem.LiquidStorageManagerRef = system;
        }
        InvokeRepeating("Extract", 1f, m_ExtractionRate);
    }

    public void Extract()
    {
        FluidSystem.LiquidStorageManagerRef.StoreLiquid(m_ExtractionAmount, m_CurrentDefinition.Category);
    }
}
