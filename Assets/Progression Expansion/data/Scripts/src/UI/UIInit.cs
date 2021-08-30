using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInit : MonoBehaviour
{
    public UiScreen m_UIScreen;
    [SerializeField]
    private string m_UiName;
    public void Awake()
    {
        int m_Length = GlobalConfig<ScreensConfig>.Value.Elements.Length;
        ScreensConfig.Element[] m_Elements = new ScreensConfig.Element[m_Length + 1];
        int i = 0;
        foreach(var obj in GlobalConfig<ScreensConfig>.Value.Elements)
        {
            m_Elements[i] = obj;
            i++;
        }
        m_Elements[m_Length] = new ScreensConfig.Element { Name = m_UiName, Prefab = m_UIScreen };

        GlobalConfig<ScreensConfig>.Value.Elements = m_Elements;
        foreach(var obj in GlobalConfig<ScreensConfig>.Value.Elements)
        {
            Debug.Log(obj.Name + " " + obj.Prefab);
        }
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
