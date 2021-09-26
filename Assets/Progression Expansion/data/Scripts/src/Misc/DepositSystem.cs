using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DepositSystem
{
    public void InitDeposits()
    {
        depositsurface = Resources.FindObjectsOfTypeAll<DepositLocationSurface>();
        depositunderground = Resources.FindObjectsOfTypeAll<DepositLocationUnderground>();

        CreateDeposit(false, 100, "CobaltOre", 3, 5, "TitaniumOre");
        CreateDeposit(true, 100, "CobaltOre", 3, 5, "TitaniumOre");
    }
    public void CreateDeposit(bool Underground, int PercentageToReplace, string outputname, float minyield, float maxyield, string ItemToReplace)
    {

        if (Underground)
        {
            foreach (DepositLocationUnderground underground in depositunderground)
            {
                if (Random.Range(0, 100) <= PercentageToReplace)
                {
                    if ((ItemToReplace != null && underground.Ore == GetItem(ItemToReplace)) || ItemToReplace == null)
                    {
                        underground.Yield = Random.Range(minyield, maxyield);
                        OreField.SetValue(underground, GetItem(outputname));
                        //Debug.Log("[Debug System | Deposits]: Underground Deposit Replacing " + ItemToReplace + " has been replaced with " + outputname);
                    }
                }
            }
        }
        if (!Underground)
        {
            foreach (DepositLocationSurface surface in depositsurface)
            {
                if (Random.Range(0, 100) <= PercentageToReplace)
                {
                    if ((ItemToReplace != null && surface.Ore == GetItem(ItemToReplace)) || ItemToReplace == null)
                    {
                        surface.Yield = Random.Range(minyield, maxyield);
                        OreField.SetValue(surface, GetItem(outputname));
                        //Debug.Log("[Debug System | Deposits]: Above Ground Deposit Replacing " + ItemToReplace + " has been replaced with " + outputname);
                    }
                }
            }
        }
    }
    private ItemDefinition GetItem(string itemname)
    {
        ItemDefinition item = RuntimeAssetDatabase.Get<ItemDefinition>().Where(item => item.name == itemname).FirstOrDefault();
        if (item == null)
        {
            //Debug.LogError("[Questing Update | Deposits]: Item is null, name: " + itemname + ". Replacing with NullItem");
            return RuntimeAssetDatabase.Get<ItemDefinition>().Where(item => item.name == "NullItem").FirstOrDefault();
        }
        return item;

    }
    private DepositLocationSurface[] depositsurface;
    private DepositLocationUnderground[] depositunderground;
    private static readonly FieldInfo OreField = typeof(DepositLocation).GetField("m_ore", BindingFlags.NonPublic | BindingFlags.Instance);

}
