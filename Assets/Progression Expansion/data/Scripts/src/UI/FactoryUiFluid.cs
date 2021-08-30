using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using InputField = TMPro.TMP_InputField;
using Dropdown = TMPro.TMP_Dropdown;
using Text = TMPro.TextMeshProUGUI;

public class FactoryUiFluid : ActiveUserWindow, IFluidFactoryUi
{
    public Slider Progress;
    public Image ItemPreview;
    public Text Amount;
    public string AmountFormat = "{0}";
    public Text FactoryName;
    public Text RecipeAmount;
    public string RecipeAmountFormat = "{0}x";

    public Toggle RunningToggle;
    public Toggle NotificationToggle;
    public Button TakeItemsButton;
    public Button StorageButton;

    [NotNull, SerializeField]
    private GameWindow m_screen = null;

    [Tooltip("Recipe color when valid")]
    public Color NormalBackgroundColor = new Color(0, 0, 0, 0);

    [Tooltip("Recipe color when valid")]
    public Color NormalOverlayColor = new Color(1, 1, 1, 1);

    [Tooltip("Recipe color when some inputs are missing")]
    public Color MissingMaterialsBackgroundColor = Color.yellow;

    [Tooltip("Recipe color when some inputs are missing")]
    public Color MissingMaterialsOverlayColor = Color.yellow;

    [Tooltip("Recipe color when validation fails (e.g. required module is not available)")]
    public Color ValidationFailedBackgroundColor = Color.red;

    [Tooltip("Recipe color when validation fails (e.g. required module is not available)")]
    public Color ValidationFailedOverlayColor = Color.red;

    private GridModule m_module;
    private LiquidProducer m_producer;
    private FluidSync m_producerSync;
    private RecipeProducedEventHandler m_notification;

    public bool IsRunning
    {
        get { return m_producerSync != null && m_producerSync.IsRunning; }
        set { if (m_producerSync != null) m_producerSync.IsRunning = value; }
    }

    public ProducerState ProductionState
    {
        get { return m_producerSync != null ? m_producerSync.State : ProducerState.Default; }
    }

    public LiquidRecipe ActiveRecipe
    {
        get { return m_producerSync != null ? m_producerSync.ActiveRecipe : null; }
    }

    public float ProductionProgress
    {
        get { return m_producerSync != null ? m_producerSync.Progress : 0; }
    }

    public int QueueSize
    {
        get { return m_producerSync != null ? m_producerSync.QueueSize : 0; }
        set { if (m_producerSync != null) m_producerSync.QueueSize = value; }
    }

    public bool IsManual
    {
        get { return m_producer.TryCast(out Worktable worktable) && !worktable.UsesInternalStorage; }
    }

    public LiquidRecipe Recipe
    {
        get { return m_producerSync != null ? m_producerSync.ActiveRecipe : null; }
    }

    ArrayReader<RecipeCategory> IFluidFactoryUi.Categories
    {
        get { return m_producer != null ? m_producer.Categories : EmptyArray<RecipeCategory>.Value; }
    }

    GameObject IFluidFactoryUi.Factory
    {
        get { return m_producer != null ? m_producer.gameObject : null; }
    }

    IRecipeGrouping IFluidFactoryUi.Grouping
    {
        get { return m_producer.Grouping; }
    }

    int IFluidFactoryUi.QueueSize { get; set; }
    LiquidRecipe IFluidFactoryUi.Recipe { get; set; }

    protected virtual void Update()
    {
        Context.TryCacheComponentSafe(ref m_module);
        Context.TryCacheComponentSafe(ref m_producer);
        Context.TryCacheComponentSafe(ref m_producerSync);
        Context.TryCacheComponentSafe(ref m_notification);

        FactoryName.text = m_module.Item.Name;
        Progress.normalizedValue = m_producerSync.Progress;

        RunningToggle.gameObject.SetActive(m_producerSync.CanPause);
        RunningToggle.isOn = m_producerSync.IsRunning;
        NotificationToggle.gameObject.SetActive(m_notification != null);
        NotificationToggle.isOn = m_notification != null && m_notification.enabled == true;
        TakeItemsButton.gameObject.SetActive(m_producer.UsesInternalStorage);
        if (m_producer.UsesInternalStorage)
        {
            TakeItemsButton.interactable = m_producerSync.StoredItemAmount > 0;
        }

        var pushInventory = m_producer.GetPushInventory();
        StorageButton.gameObject.SetActive(pushInventory != null && pushInventory.gameObject != User);

        SetItemPreview(m_producerSync.ActiveRecipe);
    }

    protected override void OnBecomeInactiveUser()
    {
        GameWindowManager.Instance.Close(m_screen);
    }

    void SetItemPreview(LiquidRecipe recipe)
    {
        var item = recipe != null ? recipe.Output.Item : null;

        var sprite = item != null ? item.Icon : null;
        ItemPreview.overrideSprite = sprite;
        ItemPreview.gameObject.SetActive(sprite != null);

        if (recipe != null && recipe.Output.Amount != 1)
        {
            RecipeAmount.SetTextFormat(RecipeAmountFormat, recipe.Output.Amount);
            RecipeAmount.enabled = true;
        }
        else
        {
            RecipeAmount.enabled = false;
        }

        if (m_producer.UsesInternalStorage)
        {
            Amount.gameObject.SetActive(true);
            Amount.SetTextFormat(AmountFormat, m_producerSync.StoredItemAmount);
        }
        else
        {
            Amount.gameObject.SetActive(false);
        }
    }

    public void TakeItems()
    {
        m_producerSync.TakeItems(User);
    }

    public void TogglePause()
    {
        IsRunning = !IsRunning;
    }

    public void ToggleIsRunning(bool isRunning)
    {
        IsRunning = isRunning;
    }

    public void ToggleNotification(bool enabled)
    {
        if (m_notification != null)
        {
            m_notification.enabled = enabled;
        }
    }

    public int GetModifier()
    {
        return InputMultiplierHelper.GetMultiplier();
    }

    public void QueuePlus()
    {
        if (QueueSize >= 0)
        {
            QueueSize = QueueSize + GetModifier();
        }
    }

    public void QueueMinus()
    {
        if (QueueSize > 0)
        {
            QueueSize -= Math.Min(QueueSize, GetModifier());
        }
        else if (QueueSize < 0)
        {
            QueueSize = 1;
        }
    }

    public void QueueInfinite()
    {
        QueueSize = -1;
    }

    bool IFluidFactoryUi.IsVisible(LiquidRecipe recipe)
    {
        return m_producer != null && m_producer.IsVisible(recipe);
    }

    public void AddRecipe(LiquidRecipe recipe, int count)
    {
        if (m_producerSync != null)
        {
            m_producerSync.AddRecipeAmount(recipe, count);
        }
    }

    public bool IsQueueable(LiquidRecipe recipe)
    {
        return m_producer != null && m_producer.IsQueueable(recipe);
    }

    public Pair<Color, Color> GetColor(LiquidRecipe recipe)
    {
        if (m_producer == null || recipe == null)
        {
            return new Pair<Color, Color>(NormalBackgroundColor, NormalOverlayColor);
        }
        if (!m_producer.IsProducible(recipe))
        {
            // IsProducible: IsQueueable + ValidateModule
            // IsProducible: IsVisible + ValidateModule
            // IsProducible: HasCategory + ValidateUpgrade + ValidateModule

            return new Pair<Color, Color>(ValidationFailedBackgroundColor, ValidationFailedOverlayColor);
        }
        else if (!m_producer.HasInputs(recipe))
        {
            return new Pair<Color, Color>(MissingMaterialsBackgroundColor, MissingMaterialsOverlayColor);
        }
        else
        {
            return new Pair<Color, Color>(NormalBackgroundColor, NormalOverlayColor);
        }
    }

    public void WriteInfo(LiquidRecipe recipe, RichTextWriter result)
    {
        if (m_producer != null)
        {
            m_producer.WriteInfo(recipe, result);
        }
    }
}
