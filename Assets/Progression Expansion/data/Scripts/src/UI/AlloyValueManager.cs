using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlloyValueManager : MonoBehaviour
{
    public TextMeshProUGUI m_BronzeValue;
    public TextMeshProUGUI m_SteelValue;
    public TextMeshProUGUI m_TitaniumValue;

    public ItemCategory tinCategory;
    public ItemCategory copperCategory;
    public ItemCategory ironCategory;
    public ItemCategory cobaltCategory;
    public ItemCategory titaniumCategory;
    public ItemCategory bronzeCategory;
    public ItemCategory steelCategory;
    public ItemCategory aluminumCategory;

    private float bronzeValue = 0f;
    private float steelValue = 0f;
    private float titaniumValue = 0f;

    private MoltenStorageManager lManager;

    public void Awake()
    {
        if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out MoltenStorageManager system))
        {
            system = production.gameObject.AddComponent<MoltenStorageManager>();
            lManager = system;
        }
        else
        {
            lManager = MoltenSystem.MoltenStorageManagerRef;
        }
        lManager.EnableClasses(tinCategory, copperCategory, ironCategory, cobaltCategory, titaniumCategory, bronzeCategory, steelCategory, aluminumCategory);
    }

    public void Update()
    {
        // Set Internal Values
        bronzeValue = lManager.GetMoltenValue(bronzeCategory);
        steelValue = lManager.GetMoltenValue(steelCategory);
        titaniumValue = lManager.GetMoltenValue(titaniumCategory);

        // Set Display
        m_BronzeValue.text = "" + bronzeValue;
        m_SteelValue.text = "" + steelValue;
        m_TitaniumValue.text = "" + titaniumValue;
    }
}
