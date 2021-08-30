using UnityEngine;

[CreateAssetMenu(menuName = "Liquids/Liquid Category")]
public class LiquidCategory : Definition
{
    [SerializeField, LocalizedStringFormat("liquidCategory.{0}.name", "Liquid Category name - {0}"), Tooltip("Category name")]
    private LocalizedString m_name = LocalizedString.Null;

    [SerializeField, LocalizedStringFormat("liquidCategory.{0}.description", "Liquid Category description - {0}", 6), Tooltip("Category description")]
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
