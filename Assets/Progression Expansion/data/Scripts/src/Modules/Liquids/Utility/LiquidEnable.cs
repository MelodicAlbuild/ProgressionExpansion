using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidEnable : MonoBehaviour
{
    public ItemCategory waterCategory;
    public ItemCategory oilCategory;
    public ItemCategory mercuryCategory;
    public ItemCategory magmaCategory;

    private LiquidStorageManager m_lStorageManager;
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
        m_lStorageManager.EnableClasses(waterCategory, oilCategory, mercuryCategory, magmaCategory);
    }
}
