using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlloyForge : MonoBehaviour
{
    [SerializeField]
    private float m_ExtractionRate;
    [SerializeField]
    private float m_ExtractionAmount;
    [SerializeField]
    private ItemDefinition m_CurrentDefinition;

    public void Awake()
    {
        if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out MoltenStorageManager system))
        {
            system = production.gameObject.AddComponent<MoltenStorageManager>();
            MoltenSystem.MoltenStorageManagerRef = system;
        }
    }
}
