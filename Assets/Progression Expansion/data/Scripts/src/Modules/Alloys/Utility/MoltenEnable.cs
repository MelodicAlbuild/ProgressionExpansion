using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoltenEnable : MonoBehaviour
{
    public ItemCategory tinCategory;
    public ItemCategory copperCategory;
    public ItemCategory ironCategory;
    public ItemCategory cobaltCategory;
    public ItemCategory titaniumCategory;
    public ItemCategory bronzeCategory;
    public ItemCategory steelCategory;
    public ItemCategory aluminumCategory;

    private MoltenStorageManager m_lStorageManager;
    private void Awake()
    {
        if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out MoltenStorageManager system))
        {
            system = production.gameObject.AddComponent<MoltenStorageManager>();
            MoltenSystem.MoltenStorageManagerRef = system;
            m_lStorageManager = system;
        }
        else
        {
            m_lStorageManager = MoltenSystem.MoltenStorageManagerRef;
        }
        m_lStorageManager.EnableClasses(tinCategory, copperCategory, ironCategory, cobaltCategory, titaniumCategory, bronzeCategory, steelCategory, aluminumCategory);
    }
}
