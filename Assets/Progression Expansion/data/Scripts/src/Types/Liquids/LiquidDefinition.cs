using UnityEngine;

[CreateAssetMenu(menuName = "Liquids/Generic Liquid")]
public class LiquidDefinition : Definition
{
    [SerializeField, LocalizedStringFormat("liquid.{0}.name", "Liquid name - {0}"), Tooltip("Name of the Liquid")]
    private LocalizedString m_name = LocalizedString.Null;

    [SerializeField, LocalizedStringFormat("liquid.{0}.description", "Liquid description - {0}", 10), Tooltip("Description of the Liquid")]
    private LocalizedString m_description = LocalizedString.Null;

    [NotNull, Tooltip("Liquid category")]
    public LiquidCategory Category;

    [Tooltip("Liquid stats"), PropertyType(typeof(LiquidStat))]
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
    public LiquidStat PrimaryStat
    {
        get
        {
            LiquidStat result = null;
            float priority = float.MinValue;
            LiquidStat tmp;
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
