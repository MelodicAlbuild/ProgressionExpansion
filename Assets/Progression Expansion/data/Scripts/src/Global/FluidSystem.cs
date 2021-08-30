using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSystem : MonoBehaviour
{
    public static LiquidStorageManager LiquidStorageManagerRef;
    private void Awake()
    {
        if (LiquidStorageManagerRef != null)
        {
            if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out LiquidStorageManager system))
            {
                system = production.gameObject.AddComponent<LiquidStorageManager>();
                LiquidStorageManagerRef = system;
            }
        }
    }
}
