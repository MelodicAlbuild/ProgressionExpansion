using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

/// <summary>
/// Base component which manufactures output from inputs.
/// </summary>
public abstract class LiquidProducer : NetworkBehaviour
{
    const float MinProgress = 0.0000001f;

    [SerializeField, Tooltip("The LiquidRecipe categories which this factory can manufacture"), FormerlySerializedAs("Categories")]
    private RecipeCategory[] m_categories = EmptyArray<RecipeCategory>.Value;

    [SerializeField, Tooltip("Time efficiency of this factory, LiquidRecipe.ProductionTime is multiplied by this number"), FormerlySerializedAs("TimeEfficiency")]
    private float m_timeEfficiency = 1;

    [SerializeField, Tooltip("Component which performs grouping")]
    private RecipeGrouping m_grouping = null;

    [SerializeField, Tooltip("True when producer can be paused by player")]
    private bool m_canPause = true;

    [NotNull, SerializeField]
    private GameObjectEvents m_events = null;

    static List<InventoryItem> m_tmpDropItems = new List<InventoryItem>();

    /// <summary>
    /// Current LiquidRecipe which is being produced.
    /// </summary>
    private LiquidRecipe m_activeRecipe = null;

    /// <summary>
    /// Amount of produced items.
    /// </summary>
    private int m_storedItemAmount = 0;

    /// <summary>
    /// Production progress, 0: inputs not loaded yet, (0-1): inputs loaded and manufacturing, 1: completed.
    /// </summary>
    private float m_progress = 0;

    /// <summary>
    /// Amount of items in queue, decremented when item is produced, 0: no production queued, -1: infinite production.
    /// </summary>
    private int m_queueSize = 0;

    /// <summary>
    /// True when production is paused.
    /// </summary>
    private bool m_isPaused = false;

    /// <summary>
    /// True when inputs are missing.
    /// </summary>
    private bool m_inputMissing = false;

    /// <summary>
    /// Last LiquidRecipe which finished production.
    /// </summary>
    private LiquidRecipe m_lastFinishedRecipe = null;

    /// <summary>
    /// Number of recipes which finished production.
    /// </summary>
    private int m_finishedRecipeCount = 0;

    /// <summary>
    /// How much is progress changed per second at current production speed.
    /// </summary>
    private float m_effectiveProgressPerSecond = 1;

    private EventInstance<LiquidRecipeProducedEvent> m_producedEvent;
    private ProducerState m_viewState;
    private float m_viewProgressPerSecond;
    private float m_viewProgressBaseTime;

    private static PropertySet m_tmpStats = new PropertySet(16);

    /// <summary>
    /// Gets current producer state.
    /// </summary>
    public ProducerState State
    {
        get { return NetworkServer.active ? GetState() : m_viewState; }
    }

    public RecipeGrouping Grouping
    {
        get { return m_grouping; }
    }

    public ArrayReader<RecipeCategory> Categories
    {
        get { return m_categories; }
    }

    public float TimeEfficiency
    {
        get { return m_timeEfficiency; }
    }

    [Display]
    public LiquidRecipe ActiveRecipe
    {
        get { return m_activeRecipe; }
        set { SetRecipe(value); }
    }

    public LiquidRecipe LastFinishedRecipe
    {
        get { return m_lastFinishedRecipe; }
    }

    public int FinishedRecipeCount
    {
        get { return m_finishedRecipeCount; }
    }

    public bool CanPause
    {
        get { return m_canPause; }
    }

    /// <summary>
    /// The size of the production queue.
    /// When zero, no production is done.
    /// When minus one, production is infinite.
    /// </summary>
    [Display]
    public int QueueSize
    {
        get { return m_queueSize; }
        set { SetQueueSize(value); }
    }

    /// <summary>
    /// The progress of the production, value between zero and one.
    /// When zero, inputs are not loaded.
    /// When larger than zero, inputs are loaded.
    /// </summary>
    [Display]
    public float Progress
    {
        get { return NetworkServer.active ? m_progress : (m_progress + m_viewProgressPerSecond * (Time.time - m_viewProgressBaseTime)); }
    }

    /// <summary>
    /// Gets value indicating whether LiquidRecipe inputs have been loaded.
    /// </summary>
    public bool InputsLoaded
    {
        get { return m_progress > 0; } // MinProgress when inputs loaded
    }

    /// <summary>
    /// True when producer is on, otherwise false.
    /// </summary>
    [Display]
    public bool IsRunning
    {
        get { return !m_canPause || !m_isPaused; }
        set { m_isPaused = m_canPause && !value; }
    }

    /// <summary>
    /// Gets value indicating whether producer is running and has LiquidRecipe in queue.
    /// </summary>
    public bool HasActiveOrder
    {
        get { return IsRunning && m_queueSize != 0 && ActiveRecipe != null; }
    }

    /// <summary>
    /// Gets value indicating whether item is being produced (progress is increasing).
    /// </summary>
    public bool IsProducing
    {
        get { return HasActiveOrder && State == ProducerState.Default; }
    }

    /// <summary>
    /// Time it takes to produce the output of active LiquidRecipe.
    /// </summary>
    public float ProductionTime
    {
        get { return m_activeRecipe != null ? ActiveRecipe.ProductionTime * TimeEfficiency / DevSettings.Instance.ProductionMultiplier : 0; }
    }

    /// <summary>
    /// How much is progress changed per second at current production speed.
    /// </summary>
    public float EffectiveProgressPerSecond
    {
        get { return m_activeRecipe != null ? m_effectiveProgressPerSecond : 0; }
    }

    /// <summary>
    /// Gets number of items stored.
    /// </summary>
    [Display]
    public int StoredItemCount
    {
        get { return m_activeRecipe != null ? m_storedItemAmount : 0; }
        set { m_storedItemAmount = value; }
    }

    /// <summary>
    /// True when creating items into internal storage represented by <see cref="StoredItemCount"/>.
    /// </summary>
    public virtual bool UsesInternalStorage
    {
        get { return false; }
    }

    /// <summary>
    /// How many times the LiquidRecipe can be produced with currently loaded inputs.
    /// </summary>
    [Display]
    public virtual int LoadedInputAmount
    {
        get { return m_progress > 0 ? 1 : 0; }
    }

    /// <summary>
    /// Gets value indicating whether inputs are missing.
    /// </summary>
    public bool InputsMissing
    {
        get { return m_inputMissing; }
    }

    protected abstract ProducerTexts Texts { get; }

    /// <summary>
    /// Tries to push items away from output inventory, it can push them to drillship storage, player inventory or else.
    /// </summary>
    protected abstract void TryPushOutput();

    /// <summary>
    /// Tries to load inputs for active LiquidRecipe, it's ok to partially load inputs and keep track of what's loaded.
    /// Returns true when all inputs are loaded, otherwise false.
    /// </summary>
    /// <remarks>
    /// This method will be called periodically until it returns true.
    /// </remarks>
    protected abstract bool TrySyncInputs();

    /// <summary>
    /// Tries to create output item, it's either created as whole or not at all.
    /// Returns true when output was created, otherwise false.
    /// </summary>
    /// <remarks>
    /// Method will be called periodically until it returns true.
    /// </remarks>
    protected abstract bool TryCreateOutput();

    /// <summary>
    /// Gets value indicating whether there are inputs available for LiquidRecipe.
    /// </summary>
    public abstract bool HasInputs(LiquidRecipe LiquidRecipe);

    /// <summary>
    /// Gets amount of input item available.
    /// </summary>
    protected abstract int GetInputAmount(InventoryItem item);

    /// <summary>
    /// Gets inventory from which are inputs taken, can be null.
    /// </summary>
    /// <returns></returns>
    public abstract InventoryBase GetPullInventory();

    /// <summary>
    /// Gets inventory into which are finished products pushed, can be null.
    /// </summary>
    public abstract InventoryBase GetPushInventory();

    /// <summary>
    /// Gets amount of loaded input.
    /// </summary>
    public abstract int GetLoadedAmount(ItemDefinition item, PropertySetReader stats);

    /// <summary>
    /// Gets producer state.
    /// </summary>
    /// <returns></returns>
    protected abstract ProducerState GetState();

    /// <summary>
    /// Gets value indicating whether to show LiquidRecipe in the UI or not.
    /// Default implementation checks that <see cref="LiquidRecipe.Categories"/> contains at least on of the <see cref="Producer.Categories"/>.
    /// </summary>
    public virtual bool IsVisible(LiquidRecipe LiquidRecipe)
    {
        return LiquidRecipe.HasCategory(Categories);
    }

    /// <summary>
    /// Gets value indicating whether LiquidRecipe can be set as active LiquidRecipe.
    /// Default implementation uses <see cref="IsVisible(LiquidRecipe)"/>.
    /// </summary>
    public virtual bool IsQueueable(LiquidRecipe LiquidRecipe)
    {
        return IsVisible(LiquidRecipe);
    }

    /// <summary>
    /// Gets value indicating whether LiquidRecipe can be produced.
    /// Default implementation uses <see cref="IsQueueable(LiquidRecipe)"/>.
    /// </summary>
    public virtual bool IsProducible(LiquidRecipe LiquidRecipe)
    {
        return IsQueueable(LiquidRecipe);
    }

    /// <summary>
    /// Adds one item to queue, intended for UI binding.
    /// </summary>
    public void IncrementQueue()
    {
        QueueSize++;
    }

    /// <summary>
    /// Removes one item from queue, intended for UI binding.
    /// </summary>
    public void DecrementQueue()
    {
        // -1 is infinity, don't go under -1
        QueueSize = Math.Max(QueueSize - 1, -1);
    }

    public virtual void WriteInfo(LiquidRecipe LiquidRecipe, RichTextWriter result)
    {
        var texts = Texts;

        result.CurrentStyle = "Title";
        result.Text.ConcatFormat(LiquidRecipe.Output.Amount == 1 ? texts.TitleFormat : texts.TitleFormatAmount, LiquidRecipe.Output.Item.Name, LiquidRecipe.Output.Amount);
        result.Text.AppendLine();

        result.AppendString("Text", LiquidRecipe.Output.Item.Description);
        result.Text.AppendLine();

        WriteInfoStats(LiquidRecipe, result.Text, texts);
        result.Text.AppendLine();
        WriteInfoInputs(LiquidRecipe, result);
    }

    protected virtual void WriteInfoStats(LiquidRecipe LiquidRecipe, StringBuilder result, ProducerTexts texts)
    {
        result.AppendLine();

        m_tmpStats.Clear();
        LiquidRecipe.Output.Item.GetStats(m_tmpStats);

        foreach (var stat in m_tmpStats.Items)
        {
            if (stat.Property != null && !stat.Property.IsInternal)
            {
                result.ConcatFormat(texts.StatFormat, stat.Property.Name, stat.Subformat);
                result.AppendLine();
            }
        }
        m_tmpStats.Clear();
    }

    protected virtual void WriteInfoInputs(LiquidRecipe LiquidRecipe, RichTextWriter result)
    {
        foreach (var input in LiquidRecipe.Inputs)
        {
            int availableAmount = GetInputAmount(input);
            if (availableAmount >= input.Amount)
            {
                result.CurrentStyle = "Text";
                result.Text.ConcatFormat(Texts.InputFormat.Text, input.Amount, input.Item.Name);
            }
            else
            {
                result.CurrentStyle = "TextError";
                result.Text.ConcatFormat(Texts.InputFormat.Text, input.Amount, input.Item.Name);
                result.CurrentStyle = "Text";
                result.Text.ConcatFormat(Texts.InputAvailableFormat.Text, availableAmount);
            }
            result.Text.AppendLine();
        }
    }

    public virtual void PlaceItems(GameObject user)
    {
    }

    public void TakeItems(GameObject user)
    {
        if (m_activeRecipe != null && m_storedItemAmount > 0 && user.TryGetComponentSafe(out Inventory inventory))
        {
            m_storedItemAmount -= inventory.Add(m_activeRecipe.Output.Item, m_activeRecipe.Output.Item.Stats, m_storedItemAmount);
        }
    }

    /// <summary>
    /// Sets new LiquidRecipe, cancels existing manufacturing process.
    /// </summary>
    private void SetRecipe(LiquidRecipe LiquidRecipe)
    {
        if (LiquidRecipe == m_activeRecipe)
            return;

        // LiquidRecipe can be changed only when there's no stored item
        if (m_storedItemAmount == 0 && (LiquidRecipe == null || IsQueueable(LiquidRecipe)))
        {
            m_activeRecipe = LiquidRecipe;
            m_queueSize = m_activeRecipe != null ? 1 : 0;
            m_progress = 0;
        }
    }

    private void SetQueueSize(int newQueueSize)
    {
        if (m_activeRecipe == null || m_queueSize == newQueueSize)
            return;

        m_queueSize = newQueueSize;
    }

    public void SetViewData(LiquidRecipe activeRecipe, int queueSize, bool isRunning)
    {
        m_activeRecipe = activeRecipe;
        m_queueSize = queueSize;
        m_isPaused = !isRunning;
    }

    public virtual void SetViewData(LiquidRecipe activeRecipe, int storedItemAmount, int loadedInputAmount, int queueSize, bool isRunning, ProducerState state, float progress, float progressPerSecond)
    {
        SetViewData(activeRecipe, queueSize, isRunning);
        m_storedItemAmount = storedItemAmount;
        m_viewState = state;
        m_progress = progress;
        m_viewProgressPerSecond = progressPerSecond;
        m_viewProgressBaseTime = Time.time;
    }

    protected virtual void Awake()
    {
        m_producedEvent = m_events.GetEvent<LiquidRecipeProducedEvent>();
        m_events.GetEvent<DeathEvent>().Subscribe(OnDeath);
    }

    private void Update()
    {
        if (!NetworkServer.active)
            return;

        if (PreProcess())
        {
            UpdateProgress(1.0f / ProductionTime);
        }
        else
        {
            m_effectiveProgressPerSecond = 0;
        }
        if (m_progress >= 1)
        {
            // Finished, try to create output item
            if (TryCreateOutput())
            {
                m_lastFinishedRecipe = m_activeRecipe;
                m_finishedRecipeCount++;

                m_progress = 0;
                if (m_queueSize > 0)
                {
                    SetQueueSize(m_queueSize - 1);
                }

                m_producedEvent.Raise(new LiquidRecipeProducedEvent() { Recipe = m_lastFinishedRecipe });
            }
            else
            {
                m_progress = 1;
            }
        }
        TryPushOutput();
    }

    /// <summary>
    /// First part of progress update, check conditions, loads items.
    /// Returns true when production is running and <see cref="UpdateProgress"/> will be called in <see cref="LateUpdate"/>.
    /// </summary>
    protected virtual bool PreProcess()
    {
        m_inputMissing = false;
        if (m_canPause && m_isPaused)
        {
            return false;
        }

        if (m_queueSize == 0 || m_activeRecipe == null)
        {
            m_progress = 0;
            TrySyncInputs();
            return false;
        }

        if (m_progress < 1 && !IsProducible(ActiveRecipe))
        {
            return false;
        }

        if (m_progress == 0)
        {
            if (TrySyncInputs())
            {
                // Inputs loaded
                m_progress = MinProgress;
            }
            else
            {
                // Inputs not loaded
                m_inputMissing = true;
                return false;
            }
        }
        return m_progress < 1;
    }

    /// <summary>
    /// Second part of progress update, advances progress value.
    /// </summary>
    protected virtual void UpdateProgress(float progressPerSecond)
    {
        m_progress = Mathf.Max(m_progress + progressPerSecond * Time.deltaTime, MinProgress);
        m_effectiveProgressPerSecond = progressPerSecond;
    }

    /// <summary>
    /// Called when object is destroyed to fill the <paramref name="dropItemList"/>.
    /// Default implementation adds stored items of active LiquidRecipe.
    /// </summary>
    protected virtual void OnDeath(DeathEvent eventArgs, List<InventoryItem> dropItemList)
    {
        if (m_storedItemAmount > 0 && m_activeRecipe != null)
        {
            var output = m_activeRecipe.Output;
            output.Amount *= m_storedItemAmount;
            dropItemList.Add(output);
        }
    }

    /// <summary>
    /// Occurs when object is destroyed.
    /// </summary>
    private void OnDeath(DeathEvent eventArgs)
    {
        if (NetworkServer.active)
        {
            using (m_tmpDropItems.Temporary())
            {
                OnDeath(eventArgs, m_tmpDropItems);
                if (m_tmpDropItems.Count > 0)
                {
                    var dropPrefab = InventoryControl.GetRandomDropPrefab();
                    if (dropPrefab != null)
                    {
                        var drop = Instantiate(dropPrefab, transform.position, transform.rotation);
                        foreach (var item in m_tmpDropItems)
                        {
                            drop.Inventory.ForceAdd(item.Item, item.Amount, item.Stats);
                        }
                        NetworkServer.Spawn(drop.gameObject);
                    }
                }
            }
        }
    }

    protected void Load(DataLiquidProducer data)
    {
        m_activeRecipe = data.ActiveRecipe;
        m_isPaused = m_canPause && data.IsPaused;
        m_progress = data.Progress;
        m_queueSize = data.QueueSize;
    }

    protected void Save(DataLiquidProducer data)
    {
        data.ActiveRecipe = ActiveRecipe;
        data.IsPaused = m_canPause && m_isPaused;
        data.Progress = Progress;
        data.QueueSize = QueueSize;
    }
}
