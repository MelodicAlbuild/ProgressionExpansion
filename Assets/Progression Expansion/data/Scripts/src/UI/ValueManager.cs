using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ValueManager : MonoBehaviour
{
    public TextMeshProUGUI m_WaterValue;
    public TextMeshProUGUI m_OilValue;
    public TextMeshProUGUI m_MercuryValue;
    public TextMeshProUGUI m_MagmaValue;

    public ItemCategory waterCategory;
    public ItemCategory oilCategory;
    public ItemCategory mercuryCategory;
    public ItemCategory magmaCategory;

    private float waterValue = 0f;
    private float oilValue = 0f;
    private float mercuryValue = 0f;
    private float magmaValue = 0f;

    private LiquidStorageManager lManager;

    public void Awake()
    {
        lManager = FluidSystem.LiquidStorageManagerRef;
        lManager.EnableClasses(waterCategory, oilCategory, mercuryCategory, magmaCategory);
    }

    public void Update()
    {
        // Set Internal Values
        waterValue = lManager.GetLiquidValue(waterCategory);
        oilValue = lManager.GetLiquidValue(oilCategory);
        mercuryValue = lManager.GetLiquidValue(mercuryCategory);
        magmaValue = lManager.GetLiquidValue(magmaCategory);

        // Set Display
        m_WaterValue.text = "" + waterValue;
        m_OilValue.text = "" + oilValue;
        m_MercuryValue.text = "" + mercuryValue;
        m_MagmaValue.text = "" + magmaValue;
    }
}
