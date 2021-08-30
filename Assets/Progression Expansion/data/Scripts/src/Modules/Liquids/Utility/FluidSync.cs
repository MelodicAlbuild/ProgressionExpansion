using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Synchronizes producer, provides command validation.
/// </summary>
public class FluidSync : NetworkStateComponent
{
    struct Snapshot
    {
        public LiquidRecipe ActiveRecipe;
        public int StoredItemAmount;
        public int LoadedInputAmount;
        public int QueueSize;
        public bool IsRunning;
        public ProducerState State;
        public float Progress;
        public float ProgressPerSecond;
        public int ProducedRecipeCount;
    }

    static readonly CommandCall<FluidSync, LiquidRecipe> m_cmdSetRecipe = new CommandCall<FluidSync, LiquidRecipe>((comp, data) => comp.OnCmdSetRecipe(data));
    static readonly CommandCall<FluidSync, LiquidRecipe, int> m_cmdAddRecipeAmount = new CommandCall<FluidSync, LiquidRecipe, int>((comp, data) => comp.OnCmdAddRecipeAmount(data.Item1, data.Item2));
    static readonly CommandCall<FluidSync> m_cmdTakeItems = new CommandCall<FluidSync>((comp, data) => comp.OnCmdTakeItems());
    static readonly CommandCall<FluidSync> m_cmdPlaceItems = new CommandCall<FluidSync>((comp, data) => comp.OnCmdPlaceItems());
    static readonly CommandCall<FluidSync, int> m_cmdSetQueueSize = new CommandCall<FluidSync, int>((comp, data) => comp.OnCmdSetQueueSize(data));
    static readonly CommandCall<FluidSync, bool> m_cmdSetIsRunning = new CommandCall<FluidSync, bool>((comp, data) => comp.OnCmdSetIsRunning(data));

    [NotNull, SerializeField]
    private LiquidProducer m_producer = null;

    [NotNull, SerializeField]
    private ActiveUsers m_activeUsers = null;

    private Snapshot m_current;
    private int m_snapshotFrame;

    public LiquidRecipe ActiveRecipe
    {
        get { return m_producer.ActiveRecipe; }
        set
        {
            if (ActiveRecipe != value)
            {
                SetPredictedState(value, QueueSize, IsRunning);
                m_cmdSetRecipe.CallCommand(this, value);
            }
        }
    }

    public int LoadedInputAmount
    {
        get { return m_producer.LoadedInputAmount; }
    }

    /// <summary>
    /// The size of the production queue.
    /// When zero, no production is done.
    /// When minus one, production is infinite.
    /// </summary>
    public int QueueSize
    {
        get { return m_producer.QueueSize; }
        set
        {
            if (QueueSize != value)
            {
                SetPredictedState(ActiveRecipe, value, IsRunning);
                m_cmdSetQueueSize.CallCommand(this, value);
            }
        }
    }

    /// <summary>
    /// Gets total amount of items produced in queue.
    /// </summary>
    public int QueueTotalAmount
    {
        get { return ActiveRecipe != null ? ActiveRecipe.Output.Amount * QueueSize : 0; }
    }

    /// <summary>
    /// Gets value indicating whether producer can be paused.
    /// </summary>
    public bool CanPause
    {
        get { return m_producer.CanPause; }
    }

    /// <summary>
    /// True when producer is on, otherwise false.
    /// </summary>
    public bool IsRunning
    {
        get { return m_producer.IsRunning; }
        set
        {
            if (IsRunning != value)
            {
                SetPredictedState(ActiveRecipe, QueueSize, value);
                m_cmdSetIsRunning.CallCommand(this, value);
            }
        }
    }

    public ProducerState State
    {
        get { return m_producer.State; }
    }

    /// <summary>
    /// The progress of the production, value between zero and one.
    /// When zero, inputs are not loaded.
    /// When larger than zero, inputs are loaded.
    /// </summary>
    public float Progress
    {
        get { return m_producer.Progress; }
    }

    /// <summary>
    /// Gets value indicating whether producer is running and has recipe in queue.
    /// </summary>
    public bool HasActiveOrder
    {
        get { return IsRunning && QueueSize != 0 && ActiveRecipe != null; }
    }

    /// <summary>
    /// Gets icon of active recipe.
    /// </summary>
    public Sprite ActiveRecipeIcon
    {
        get { return ActiveRecipe != null ? ActiveRecipe.GetIcon() : null; }
    }

    public bool IsProducing
    {
        get { return HasActiveOrder && State == ProducerState.Default; }
    }

    /// <summary>
    /// Gets icon of stored items in <see cref="OutputInventory"/>.
    /// </summary>
    public Sprite StoredItemIcon
    {
        get { return ActiveRecipe != null && StoredItemAmount > 0 ? ActiveRecipe.Output.Item.Icon : null; }
    }

    /// <summary>
    /// Gets amount of stored items.
    /// </summary>
    public int StoredItemAmount
    {
        get { return m_producer.StoredItemCount; }
    }

    /// <summary>
    /// Gets active recipe icon.
    /// </summary>
    public ItemDefinition ActiveItemDisplay
    {
        get { return ActiveRecipe != null ? ActiveRecipe.Output.Item : null; }
    }

    /// <summary>
    /// Gets icon of first input of the <see cref="ActiveRecipe"/>.
    /// </summary>
    public Sprite FirstInputIcon
    {
        get { return ActiveRecipe != null ? ActiveRecipe.Inputs[0].Item.Icon : null; }
    }

    private void Reset()
    {
        m_producer = GetComponent<LiquidProducer>();
        m_activeUsers = GetComponent<ActiveUsers>();
    }

    /// <summary>
    /// Sets client-predicted state.
    /// </summary>
    void SetPredictedState(LiquidRecipe activeRecipe, int queueSize, bool isRunning)
    {
        if (!NetworkServer.active)
        {
            m_producer.SetViewData(activeRecipe, queueSize, isRunning);
        }
    }

    public void IncrementQueue()
    {
        QueueSize++;
    }

    public void DecrementQueue()
    {
        QueueSize--;
    }

    public void PlaceItems(GameObject user)
    {
        m_cmdPlaceItems.CallCommand(this);
    }

    public void TakeItems(GameObject user)
    {
        if (m_producer.ActiveRecipe != null && user.TryGetComponent(out Inventory inventory) && !inventory.CanAdd(m_producer.ActiveRecipe.Output.Item, 1, m_producer.ActiveRecipe.Output.Stats))
        {
            HudNotifications.Add("InventoryFull");
        }
        else
        {
            m_cmdTakeItems.CallCommand(this);
        }
    }

    public void AddRecipeAmount(LiquidRecipe recipe, int amount)
    {
        if (!NetworkServer.active)
        {
            // Client-side prediction
            if (ActiveRecipe == recipe || StoredItemAmount == 0)
            {
                if (QueueSize <= -amount && StoredItemAmount == 0)
                {
                    m_producer.SetViewData(null, 0, IsRunning);
                }
                else
                {
                    m_producer.SetViewData(recipe, QueueSize + amount, IsRunning);
                }
            }
        }
        m_cmdAddRecipeAmount.CallCommand(this, recipe, amount);
    }

    public override bool ValidateCommand(NetworkConnection sender, int cmdHash)
    {
        var flags = cmdHash == m_cmdSetRecipe.CmdHash || cmdHash == m_cmdAddRecipeAmount.CmdHash ? ValidationFlags.Default : ValidationFlags.OnDemandMultiUser;
        return m_activeUsers.IsActiveUser(CommandContext.Player, flags);
    }

    private void OnCmdSetRecipe(LiquidRecipe recipe)
    {
        // Using client-side prediction, make sure to resend state to correct possible misprediction
        MarkClientDirty(CommandContext.Sender);
        m_producer.ActiveRecipe = recipe;
    }

    private void OnCmdAddRecipeAmount(LiquidRecipe recipe, int amount)
    {
        // Using client-side prediction, make sure to resend state to correct possible misprediction
        MarkClientDirty(CommandContext.Sender);
        if (amount < 0)
        {
            // Reducing amount
            if (m_producer.QueueSize <= -amount)
            {
                // Clearing queue, take items to allow setting recipe to null
                m_producer.TakeItems(CommandContext.Player);
                if (m_producer.StoredItemCount > 0)
                {
                    // Not all items were taken, set queue size to zero and keep recipe assigned
                    m_producer.QueueSize = 0;
                }
                else
                {
                    // All items were taken
                    m_producer.ActiveRecipe = null;
                }
            }
            else
            {
                // Reduce amount, keep recipe in queue
                m_producer.QueueSize += amount;
            }
        }
        else if (ActiveRecipe == recipe)
        {
            // Adding amount to existing recipe
            m_producer.QueueSize += amount;
        }
        else
        {
            // Changing recipe
            if (m_producer.StoredItemCount > 0)
            {
                // Changing recipe and there are some stored items, pickup existing items
                m_producer.TakeItems(CommandContext.Player);
            }
            if (m_producer.StoredItemCount == 0)
            {
                // No items are stored, set recipe and amount
                m_producer.ActiveRecipe = recipe;
                m_producer.QueueSize = amount;
            }
        }
    }

    private void OnCmdTakeItems()
    {
        // Using client-side prediction, make sure to resend state to correct possible misprediction
        MarkClientDirty(CommandContext.Sender);
        m_producer.TakeItems(CommandContext.Player);
    }

    private void OnCmdPlaceItems()
    {
        // Using client-side prediction, make sure to resend state to correct possible misprediction
        MarkClientDirty(CommandContext.Sender);
        m_producer.PlaceItems(CommandContext.Player);
    }

    private void OnCmdSetQueueSize(int queueSize)
    {
        // Using client-side prediction, make sure to resend state to correct possible misprediction
        MarkClientDirty(CommandContext.Sender);
        m_producer.QueueSize = queueSize;
        m_producer.PlaceItems(CommandContext.Player);
    }

    private void OnCmdSetIsRunning(bool isRunning)
    {
        // Using client-side prediction, make sure to resend state to correct possible misprediction
        MarkClientDirty(CommandContext.Sender);
        m_producer.IsRunning = isRunning;
    }

    bool Equals(in Snapshot x, in Snapshot y)
    {
        // Progress is fully client-side predicted, ignore
        return x.ActiveRecipe == y.ActiveRecipe
            && x.StoredItemAmount == y.StoredItemAmount
            && x.LoadedInputAmount == y.LoadedInputAmount
            && x.QueueSize == y.QueueSize
            && x.IsRunning == y.IsRunning
            && x.State == y.State
            && x.ProducedRecipeCount == y.ProducedRecipeCount
            && MathUtil.WithinFraction(x.ProgressPerSecond, y.ProgressPerSecond, 0.1f);
    }

    protected override int GetVersion()
    {
        if (NetworkServer.active && m_snapshotFrame != Time.frameCount)
        {
            m_snapshotFrame = Time.frameCount;

            Snapshot state;
            state.ActiveRecipe = m_producer.ActiveRecipe;
            state.StoredItemAmount = m_producer.StoredItemCount;
            state.LoadedInputAmount = m_producer.LoadedInputAmount;
            state.QueueSize = m_producer.QueueSize;
            state.IsRunning = m_producer.IsRunning;
            state.State = m_producer.State;
            state.Progress = m_producer.Progress;
            state.ProgressPerSecond = m_producer.EffectiveProgressPerSecond;
            state.ProducedRecipeCount = m_producer.FinishedRecipeCount;

            // When new progress is smaller than old, next item is being produced, update must be sent
            if (!Equals(m_current, state) || state.Progress < m_current.Progress)
            {
                CurrentVersion++;
                m_current = state;
            }
        }
        return CurrentVersion;
    }

    protected override float GetPriority(int clientConnectionId, GameObject player, Vector3 playerPosition)
    {
        // When dirty:
        // Once every 10 frames, send to all active users        
        // Once every 30 frames, send to all observers with range of 10,
        // Once every 5s, send to all observers

        if (m_activeUsers.Contains(player))
        {
            // Active production window
            return float.MaxValue;
        }
        else
        {
            // Object synchronized to client, might be seeing world UI
            // TODO: Priority design (split into sections, grouping by importance, distance, ...)
            // TODO: Support update rate (limit update rate to lower value than default world update)
            return 1;
        }
    }

    protected override bool Serialize(int clientConnectionId, NetworkWriter writer, int availableSpace, ref ServerCustomData customData)
    {
        if (availableSpace < 24)
            return false;

        // Min.size: 1 + 1 + 1 + 2 + 0 + 0 + 0 = 5 B
        // Typ.size: 1 + 2 + 1 + 2 + 1 + 1 + 1 = 9 B
        // Max size: 1 + 5 + 1 + 2 + 5 + 5 + 5 = 24 B

        SmallBitFieldBuffer buf = new SmallBitFieldBuffer(0);
        buf.Write(m_current.IsRunning);
        buf.Write((int)m_current.State, 3);
        buf.Write(m_current.StoredItemAmount == 0);
        buf.Write(m_current.LoadedInputAmount == 0);
        buf.Write(m_current.QueueSize < 0);
        buf.Write(m_current.QueueSize == 0);
        writer.Write((byte)buf.BitField.Bits);

        writer.SerializeDefinition(m_current.ActiveRecipe);
        writer.WriteUNorm8(m_current.Progress);
        writer.WriteHalf(m_current.ProgressPerSecond);
        if (m_current.StoredItemAmount > 0)
        {
            writer.WritePackedUInt32((uint)m_current.StoredItemAmount);
        }
        if (m_current.LoadedInputAmount > 0)
        {
            writer.WritePackedUInt32((uint)m_current.LoadedInputAmount);
        }
        if (m_current.QueueSize > 0)
        {
            writer.WritePackedUInt32((uint)m_current.QueueSize);
        }
        return true;
    }

    protected override void Deserialize(NetworkReader reader)
    {
        SmallBitFieldBuffer buf = new SmallBitFieldBuffer(reader.ReadByte());
        m_current.IsRunning = buf.Read();
        m_current.State = (ProducerState)buf.Read(3);
        bool storedItemZero = buf.Read();
        bool loadedAmountZero = buf.Read();
        bool queueSizeNegative = buf.Read();
        bool queueSizeZero = buf.Read();

        // Read any apply data to m_current
        m_current.ActiveRecipe = reader.DeserializeDefinition() as LiquidRecipe;
        m_current.Progress = reader.ReadUNorm8();
        m_current.ProgressPerSecond = reader.ReadHalf();
        m_current.StoredItemAmount = storedItemZero ? 0 : (int)reader.ReadPackedUInt32();
        m_current.LoadedInputAmount = loadedAmountZero ? 0 : (int)reader.ReadPackedUInt32();
        m_current.QueueSize = queueSizeNegative ? -1 : (queueSizeZero ? 0 : (int)reader.ReadPackedUInt32());


        m_producer.SetViewData(m_current.ActiveRecipe, m_current.StoredItemAmount, m_current.LoadedInputAmount, m_current.QueueSize, m_current.IsRunning, m_current.State, m_current.Progress, m_current.ProgressPerSecond);
    }
}
