using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInit : MonoBehaviour
{
    public UiScreen m_UIScreen;
    [SerializeField]
    private string m_UiName;
    public void Start()
    {
        GlobalConfig<ScreensConfig>.Value.SetScreen(m_UiName, m_UIScreen);
    }
    public void SetName(string name)
    {
        m_UiName = name;
    }
    public void SetName(UiScreen screen)
    {
        m_UIScreen = screen;
    }
}
