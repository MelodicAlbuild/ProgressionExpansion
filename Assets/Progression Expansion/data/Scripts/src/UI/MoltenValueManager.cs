using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoltenValueManager : MonoBehaviour
{
    public TextMeshProUGUI m_TinValue;
    public TextMeshProUGUI m_CopperValue;
    public TextMeshProUGUI m_IronValue;
    public TextMeshProUGUI m_CobaltValue;

    public ItemCategory tinCategory;
    public ItemCategory copperCategory;
    public ItemCategory ironCategory;
    public ItemCategory cobaltCategory;
    public ItemCategory titaniumCategory;
    public ItemCategory bronzeCategory;
    public ItemCategory steelCategory;
    public ItemCategory aluminumCategory;

    private float tinValue = 0f;
    private float copperValue = 0f;
    private float ironValue = 0f;
    private float cobaltValue = 0f;

    private MoltenStorageManager lManager;

    public void Awake()
    {
        if (gameObject.TryGetComponentInParent(out TrainProduction production) && !production.TryGetComponent(out MoltenStorageManager system))
        {
            system = production.gameObject.AddComponent<MoltenStorageManager>();
            lManager = system;
        } else
        {
            lManager = MoltenSystem.MoltenStorageManagerRef;
        }
        lManager.EnableClasses(tinCategory, copperCategory, ironCategory, cobaltCategory, titaniumCategory, bronzeCategory, steelCategory, aluminumCategory);
    }

    public void Update()
    {
        // Set Internal Values
        tinValue = lManager.GetMoltenValue(tinCategory);
        copperValue = lManager.GetMoltenValue(copperCategory);
        ironValue = lManager.GetMoltenValue(ironCategory);
        cobaltValue = lManager.GetMoltenValue(cobaltCategory);

        // Set Display
        m_TinValue.text = "" + tinValue;
        m_CopperValue.text = "" + copperValue;
        m_IronValue.text = "" + ironValue;
        m_CobaltValue.text = "" + cobaltValue;
    }
}
