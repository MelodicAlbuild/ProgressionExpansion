using UnityEngine;

[CreateAssetMenu(menuName = "Gasses/Generic Gas")]
public class GasDefinition : Definition
{
    [SerializeField, LocalizedStringFormat("gas.{0}.name", "Gas name - {0}"), Tooltip("Name of the Gas")]
    private LocalizedString m_name = LocalizedString.Null;

    [SerializeField, LocalizedStringFormat("gas.{0}.description", "Gas description - {0}", 10), Tooltip("Description of the Gas")]
    private LocalizedString m_description = LocalizedString.Null;

    [NotNull, Tooltip("Gas category")]
    public ItemCategory Category;

    [Tooltip("Gas stats"), PropertyType(typeof(GasStat))]
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
    public GasStat PrimaryStat
    {
        get
        {
            GasStat result = null;
            float priority = float.MinValue;
            GasStat tmp;
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
