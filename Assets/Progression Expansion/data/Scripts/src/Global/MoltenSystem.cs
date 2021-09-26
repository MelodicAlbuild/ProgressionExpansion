using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoltenSystem : MonoBehaviour
{
    public static MoltenStorageManager MoltenStorageManagerRef;
    private void Awake()
    {
        if (MoltenStorageManagerRef == null)
        {
            if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out MoltenStorageManager system))
            {
                system = production.gameObject.AddComponent<MoltenStorageManager>();
                MoltenStorageManagerRef = system;
            }
        }
    }
}
