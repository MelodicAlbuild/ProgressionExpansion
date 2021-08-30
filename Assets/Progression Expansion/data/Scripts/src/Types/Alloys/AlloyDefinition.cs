using UnityEngine;

[CreateAssetMenu(menuName = "Alloys/Generic Alloy")]
public class AlloyDefinition : Definition
{
    [SerializeField, LocalizedStringFormat("alloy.{0}.name", "Alloy name - {0}"), Tooltip("Name of the Alloy")]
    private LocalizedString m_name = LocalizedString.Null;

    [SerializeField, LocalizedStringFormat("alloy.{0}.description", "Alloy description - {0}", 10), Tooltip("Description of the Alloy")]
    private LocalizedString m_description = LocalizedString.Null;

    [NotNull, Tooltip("Alloy category")]
    public ItemCategory Category;

    [Tooltip("Alloy stats"), PropertyType(typeof(AlloyStat))]
    public PropertySet Stats = new PropertySet(0);

    [Toggle]
    public bool EnabledInBuild
    {
        get { return RuntimeAssetDatabase.IsEnabledInBuild(AssetId); }
        set { RuntimeAssetDatabase.EnableInBuild(AssetId, value); }
    }

    public string Name
    {
        get { return m_name.Text; }
    }

    public string Description
    {
        get { return m_description.Text; }
    }

    /// <summary>
    /// Stat which is displayed in editor as bar.
    /// </summary>
    public AlloyStat PrimaryStat
    {
        get
        {
            AlloyStat result = null;
            float priority = float.MinValue;
            AlloyStat tmp;
            foreach (var stat in Stats.Items)
            {
                if (stat.Property.TryCast(out tmp) && tmp.IsPrimary && tmp.Priority > priority)
                {
                    result = tmp;
                    priority = tmp.Priority;
                }
            }
            return result;
        }
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

    public override string ToString()
    {
        return Name;
    }

    protected override void Localize(LocalizationProcessor handler)
    {
        if (EnabledInBuild)
        {
            handler.Process(ref m_name);
            handler.Process(ref m_description);
        }
    }
}
