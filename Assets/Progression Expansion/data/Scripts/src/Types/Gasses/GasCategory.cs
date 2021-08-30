using UnityEngine;

[CreateAssetMenu(menuName = "Gasses/Gas Category")]
public class GasCategory : Definition
{
    [SerializeField, LocalizedStringFormat("gasCategory.{0}.name", "Gas Category name - {0}"), Tooltip("Category name")]
    private LocalizedString m_name = LocalizedString.Null;

    [SerializeField, LocalizedStringFormat("gasCategory.{0}.description", "Gas Category description - {0}", 6), Tooltip("Category description")]
    private LocalizedString m_description = LocalizedString.Null;

    [Tooltip("Category icon")]
    public Sprite Icon;

    [Tooltip("Category sort order")]
    public int SortOrder;

    public string Name
    {
        get { return m_name.Text; }
    }

    public string Description
    {
        get { return m_description.Text; }
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

    public override Sprite GetIcon()
    {
        return Icon;
    }

    public override string ToString()
    {
        return Name;
    }

    protected override void Localize(LocalizationProcessor handler)
    {
        handler.Process(ref m_name);
        handler.Process(ref m_description);
    }
}
