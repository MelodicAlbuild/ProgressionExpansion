using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Gasses/Gas Stat")]
public class GasStat : Property
{
    [SerializeField, Tooltip("Property type"), FormerlySerializedAs("Type")]
    private PropertyType m_type = null;

    [SerializeField, LocalizedStringFormat("gasStat.{0}.name", "Gas Stat name - {0}"), Tooltip("Name of the stat")]
    private LocalizedString m_name = LocalizedString.Null;

    [SerializeField, LocalizedStringFormat("gasStat.{0}.description", "Gas Stat description - {0}", 6), Tooltip("Description of the stat")]
    private LocalizedString m_description = LocalizedString.Null;

    [Tooltip("Format string for value")]
    public string FormatString = "{0}";

    [Tooltip("Optional formatter")]
    public PropertyFormatter Formatter = null;

    [Tooltip("Stat priority, higher priority is higher in stat list")]
    public float Priority = 0;

    [Tooltip("Primary stat with highest priority has stat bar visible in inventory")]
    public bool IsPrimary = false;

    public override string Name
    {
        get { return m_name.Text; }
    }

    public override string Description
    {
        get { return m_description.Text; }
    }

    public override PropertyType Type
    {
        get { return m_type; }
    }

    public LocalizedString NameLocalization
    {
        get { return m_name; }
        set { m_name = value; }
    }

    public LocalizedString DescriptionLocalization
    {
        get { return m_description; }
        set { m_description = value; }
    }

    public override void FormatValue(PropertyStorage value, StringBuilder output)
    {
        if (Formatter != null)
        {
            Formatter.Format(new PropertyValue(value, m_type), FormatString, output);
        }
        else
        {
            m_type.Handler.Format(value, FormatString, output);
        }
    }

    protected override void Localize(LocalizationProcessor handler)
    {
        handler.Process(ref m_name);
        handler.Process(ref m_description);
    }

#if UNITY_EDITOR
    [Button]
    [ContextMenu("Set name from file")]
    protected void SetNameFromFile()
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GetAssetPath(this));
        UnityEditor.Undo.RegisterCompleteObjectUndo(this, "GasStat name from file");
        m_name = new LocalizedString(m_name.Id, name, m_name.Comment);
        EditorUtils.SetDirty(this);
    }
#endif
}
