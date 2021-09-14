using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class FluidStation : LiquidProducer, IPersistentBehaviour<DataLiquidModule>, IProductionQueue, IRemoveCondition
{
    OnlineCargo m_cargo;
    TrainUpgrades m_upgrades;
    TrainProduction m_production;
    TrainProduction.GroupInfo m_productionGroup;

    [NotNull, SerializeField]
    FactoryType m_factoryType = null;

    [NotNull, SerializeField]
    FactoryTexts m_texts = null;

    [SerializeField, Tooltip("Production weight, how much production points to use relative to other production stations")]
    float m_weight = 1.0f;

    [SerializeField, Tooltip("Automatically installs upgrade when produced")]
    bool m_autoInstall = false;

    LiquidRecipe m_loadedInputs = null;

    public FactoryType FactoryType
    {
        get { return m_factoryType; }
    }

    public TrainProduction TrainProduction
    {
        get { return m_production; }
    }

    public bool OnlineCargoAvailable
    {
        get { return m_cargo != null && m_cargo.Inventories.Count > 0; }
    }

    [Display]
    public LiquidRecipe LoadedInputs
    {
        get { return m_loadedInputs; }
    }

    protected override ProducerTexts Texts
    {
        get { return m_texts; }
    }

    int IProductionQueue.QueueSize
    {
        get { return (ActiveRecipe != null && QueueSize != 0) ? 1 : 0; }
    }

    RecipeAmount IProductionQueue.this[int queueIndex]
    {
        get { return new RecipeAmount { Recipe = ActiveRecipe, Amount = QueueSize }; }
    }

    protected override void Awake()
    {
        base.Awake();
        m_production = GetComponentInParent<TrainProduction>();
        m_cargo = GetComponentInParent<OnlineCargo>();
        m_productionGroup = m_production.GetOrCreate(m_factoryType);
        m_upgrades = GetComponentInParent<TrainUpgrades>();
        m_production.RegisterQueue(this);
    }

    private void OnDestroy()
    {
        m_production.UnregisterQueue(this);
    }

    public override bool IsProducible(LiquidRecipe recipe)
    {
        // Category, upgrade and module check
        return base.IsQueueable(recipe) && ValidateModule(recipe, true);
    }

    public override bool IsQueueable(LiquidRecipe recipe)
    {
        return base.IsQueueable(recipe) && ValidateModule(recipe, false);
    }

    public override bool IsVisible(LiquidRecipe recipe)
    {
        // Category check and upgrade check
        return base.IsVisible(recipe) && ValidateUpgrade(recipe);
    }

    protected override ProducerState GetState()
    {
        if (ActiveRecipe == null) return ProducerState.Default;
        if (!ValidateUpgrade(ActiveRecipe)) return ProducerState.TechnologyMissing;
        if (!ValidateModule(ActiveRecipe, true)) return ProducerState.ProductionUnitMissing;
        if (QueueSize == 0 || (Progress > 0 && Progress < 1)) return ProducerState.Default;
        if (QueueSize != 0 && Progress == 0 && !OnlineCargoAvailable) return ProducerState.InputInventoryUnavailable;
        if (QueueSize != 0 && Progress == 0 && InputsMissing) return ProducerState.InputMissing;
        if (Progress == 1 && !OnlineCargoAvailable) return ProducerState.OutputInventoryUnavailable;
        if (Progress == 1) return ProducerState.OutputFull;
        return ProducerState.Default;
    }

    public override InventoryBase GetPullInventory()
    {
        return m_cargo;
    }

    public override InventoryBase GetPushInventory()
    {
        return m_cargo;
    }

    public bool ValidateUpgrade(LiquidRecipe recipe)
    {
        foreach (var upgrade in (recipe.RequiredUpgrades ?? EmptyArray<ItemDefinition>.Value))
        {
            if (m_upgrades == null || !m_upgrades.Upgrades.Contains(upgrade))
                return false;
        }
        return true;
    }

    public bool ValidateModule(LiquidRecipe recipe, bool deployed)
    {
        if (m_productionGroup == null)
        {
            return false;
        }

        var moduleList = deployed ? m_productionGroup.ActiveModules : m_productionGroup.Modules;
        foreach (var module in moduleList)
        {
            if (HasModuleCategory(module, recipe))
                return true;
        }
        return false;
    }

    public void SetFactoryType(FactoryType factoryType)
    {
        if (m_production != null)
        {
            m_production.UnregisterQueue(this);
        }
        m_factoryType = factoryType;
        if (m_production != null)
        {
            m_productionGroup = m_production.GetOrCreate(factoryType);
            m_production.RegisterQueue(this);
        }
    }

    protected bool HasModuleCategory(ProductionModule module, LiquidRecipe recipe)
    {
        foreach (var category in module.Categories)
        {
            if (recipe.Categories.Contains(category))
                return true;
        }
        return false;
    }

    protected override bool PreProcess()
    {
        if (base.PreProcess())
        {
            m_productionGroup.AddWeightForFrame(m_weight);
            return true;
        }
        return false;
    }

    protected override void UpdateProgress(float progressPerSecond)
    {
        if (m_weight > 0)
        {
            progressPerSecond *= m_productionGroup.Points * (m_weight / m_productionGroup.TotalWeight);
        }
        else
        {
            progressPerSecond = 0;
        }
        base.UpdateProgress(progressPerSecond);
    }

    protected override void TryPushOutput()
    {
        // Nothing to do, creating directly into drillship storage
    }

    protected override bool TryCreateOutput()
    {
        // Creating directly into drillship storage
        if (TryStore(ActiveRecipe.Output))
        {
            m_loadedInputs = null;
            OnRecipeProduced(ActiveRecipe);
            return true;
        }
        return false;
    }

    private bool TryStore(InventoryItem item)
    {
        if (m_autoInstall && item.Amount == 1 && m_upgrades != null && m_upgrades.Crew != null && m_upgrades.Crew.IsPlayerOwned && !m_upgrades.IsInstalled(ActiveRecipe.Output.Item))
        {
            if (m_upgrades.Add(gameObject, ActiveRecipe.Output.Item, ActiveRecipe.Output.Stats, item.Amount, 0) > 0)
                return true;
        }
        if (m_cargo != null && m_cargo.AddBatch(gameObject, ActiveRecipe.Output.Item, ActiveRecipe.Output.Stats, ActiveRecipe.Output.Amount))
        {
            return true;
        }
        return false;
    }

    private void OnRecipeProduced(LiquidRecipe recipe)
    {
        if (recipe.Output.Item.GetModules().Any())
        {
            CommonStats.Instance.ModulesProduced.IncrementTrainStat(m_upgrades.gameObject, recipe.Output.Amount);
        }
    }

    public override bool HasInputs(LiquidRecipe recipe)
    {
        if (m_cargo == null)
            return false;

        foreach (var item in recipe.Inputs)
        {
            int missingAmount = item.Amount - GetLoadedAmount(item.Item, item.Stats);
            if (missingAmount > 0 && m_cargo.GetAmount(item.Item, item.Stats, item.Amount) < missingAmount)
                return false;
        }
        return true;
    }

    protected override int GetInputAmount(InventoryItem item)
    {
        // When production has not started yet, include loaded amount
        int availableAmount = Progress == 0 ? GetLoadedAmount(item.Item, item.Stats) : 0;
        if (m_cargo != null)
        {
            availableAmount += m_cargo.GetAmount(item.Item, item.Stats, item.Amount);
        }
        return availableAmount;
    }

    public override void WriteInfo(LiquidRecipe recipe, RichTextWriter result)
    {
        base.WriteInfo(recipe, result);

        result.Text.AppendLine();
        if (ValidateModule(recipe, true))
        {
            result.AppendString("Subtitle", m_texts.ActiveModule);
        }
        else
        {
            result.AppendString("SubtitleError", m_texts.NoActiveModule);
        }
        result.CurrentStyle = "Text";
        WriteInfoModules(recipe, result);

        if (recipe.RequiredUpgrades.Length > 0)
        {
            result.Text.AppendLine();
            result.Text.AppendLine();
            if (ValidateUpgrade(recipe))
            {
                result.AppendString("Subtitle", m_texts.RequiredUpgrades);
            }
            else
            {
                result.AppendString("SubtitleError", m_texts.RequiredUpgradesMissing);
            }
            WriteInfoUpgrades(recipe, result);
        }
    }

    private void WriteInfoModules(LiquidRecipe recipe, RichTextWriter result)
    {
        string errorStyle = ValidateModule(recipe, true) ? "TextInactive" : "TextError";

        foreach (var module in GameResources.Instance.ProductionModules)
        {
            if (module.FactoryType == m_factoryType && HasModuleCategory(module, recipe))
            {
                var gridModule = module.ItemObject;
                result.Text.AppendLine();
                if (HasActiveModule(gridModule.Item))
                {
                    result.AppendString("Text", gridModule.Item.Name);
                    result.Text.Append(m_texts.Active);
                }
                else
                {
                    result.AppendString(errorStyle, gridModule.Item.Name);
                }
            }
        }
    }

    private void WriteInfoUpgrades(LiquidRecipe recipe, RichTextWriter result)
    {
        foreach (var upgrade in recipe.RequiredUpgrades)
        {
            result.Text.AppendLine();
            if (m_upgrades != null && m_upgrades.Upgrades.Contains(upgrade))
            {
                result.AppendString("Text", upgrade.Name);
                result.Text.Append(m_texts.Installed);
            }
            else
            {
                result.AppendString("TextError", upgrade.Name);
                result.Text.Append(m_texts.Missing);
            }
        }
    }

    private bool HasActiveModule(ItemDefinition moduleItem)
    {
        if (m_productionGroup != null)
        {
            foreach (var active in m_productionGroup.ActiveModules)
            {
                if (active.ItemObject.Item == moduleItem)
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected override void WriteInfoInputs(LiquidRecipe recipe, RichTextWriter result)
    {
        if (!OnlineCargoAvailable)
        {
            result.AppendString("SubtitleError", m_texts.OnlineCargoMissing);
        }
        else if (!HasInputs(recipe))
        {
            result.AppendString("SubtitleError", m_texts.OnlineCargoItemsMissing);
        }
        else
        {
            result.AppendString("Subtitle", m_texts.OnlineCargoItems);
        }
        result.Text.AppendLine();

        base.WriteInfoInputs(recipe, result);
    }

    /// <summary>
    /// Returns true when inputs are loaded.
    /// </summary>
    protected override bool TrySyncInputs()
    {
        if (m_cargo == null)
            return false;

        var recipe = QueueSize != 0 ? ActiveRecipe : null;

        // Correct inputs loaded
        if (m_loadedInputs == recipe)
        {
            return true;
        }

        // Different inputs loaded, try return
        if (m_loadedInputs != null && !TryReturnInputs(m_cargo))
        {
            return false;
        }

        // Nothing loaded, try load correct inputs
        if (recipe != null && !TryLoadInputs(m_cargo))
        {
            return false;
        }
        return true;
    }

    public override int GetLoadedAmount(ItemDefinition item, PropertySetReader stats)
    {
        return m_loadedInputs != null ? m_loadedInputs.UniqueInputs.GetAmount(item, stats) : 0;
    }

    bool TryLoadInputs(InventoryBase source)
    {
        // We can do this, inputs are unique
        if (m_loadedInputs == null && source.RemoveBatch(gameObject, ActiveRecipe.UniqueInputs))
        {
            m_loadedInputs = ActiveRecipe;
            return true;
        }
        return false;
    }

    bool TryReturnInputs(InventoryBase target)
    {
        if (m_loadedInputs == null)
        {
            return true;
        }
        else if (target.AddBatch(gameObject, m_loadedInputs.UniqueInputs))
        {
            m_loadedInputs = null;
            return true;
        }
        return false;
    }

    protected override void OnDeath(DeathEvent eventArgs, List<InventoryItem> dropItemList)
    {
        TryReturnInputs(m_cargo);
        base.OnDeath(eventArgs, dropItemList);
        if (m_loadedInputs != null)
        {
            foreach (var input in m_loadedInputs.UniqueInputs)
            {
                dropItemList.Add(input);
            }
        }
    }

    void IPersistentBehaviour<DataLiquidModule>.Load(DataLiquidModule data)
    {
        Load(data);
        m_weight = data.Weight;
        m_loadedInputs = data.LoadedInputsRecipe;
        if (data.LoadedInputs != null && data.LoadedInputs.Length > 0 && data.LoadedInputs.Any(s => s.Amount > 0 && s.Item != null))
        {
            InventoryItem[] items = data.LoadedInputs.Select(s => (InventoryItem)s);
            PersistentContext.Current.PostLoad += () => { if (m_cargo.TryGetFallbackInventory(out var fallback)) fallback.ForceAdd(new InventoryReader(items, items.Length)); };
        }
    }

    void IPersistentBehaviourBase<DataLiquidModule>.Save(DataLiquidModule data)
    {
        Save(data);
        data.Weight = m_weight;
        data.LoadedInputsRecipe = m_loadedInputs;
    }

    bool IRemoveCondition.Evaluate(GameObject player, out string errorNotificationId)
    {
        TakeItems(player);
        if (StoredItemCount > 0)
        {
            // There are finished items, which cannot be picked up by player
            errorNotificationId = "CannotDeconstructStation";
            return false;
        }
        else if (TryReturnInputs(m_cargo))
        {
            // Returned everything into cargo
            ActiveRecipe = null;
            errorNotificationId = null;
            return true;
        }
        else if (player.TryGetComponentSafe(out Inventory playerInventory) && TryReturnInputs(playerInventory))
        {
            // Returned everything into storage and player inventory
            ActiveRecipe = null;
            errorNotificationId = null;
            return true;
        }
        else
        {
            // Items does not fit into storage or player inventory (splitting items into both inventories is not supported)
            errorNotificationId = "CannotDeconstructStation";
            return false;
        }
    }
}
