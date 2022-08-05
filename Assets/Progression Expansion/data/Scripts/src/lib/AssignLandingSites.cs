using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Progression_Expansion.data.Scripts.src.lib
{
    public class AssignLandingSites : MonoBehaviour
    {
        private static readonly FieldInfo TASK_LOOT_WRECK_M_LANDING_SITES = typeof(TaskLootWreck).GetField("m_landingSites", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TASK_LOOT_CARGO_M_DATA =
            typeof(TaskLootCargo).GetField("m_data", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TASK_LOOT_CARGO_LANDING_SITES =
            typeof(TaskLootCargo.Arguments).GetField("LandingSites", BindingFlags.Instance | BindingFlags.NonPublic);
        
        public TaskLootCargo lootCargo;
        public TaskLootWreck lootWreck;

        public void Startup()
        {
            if (lootCargo == null)
            {
                List<LandingSite> obj = (List<LandingSite>)TASK_LOOT_WRECK_M_LANDING_SITES.GetValue(lootWreck);
                List<LandingSite> obj2 = new List<LandingSite>();
                if (obj.Count > 0)
                {
                    obj.Clear();
                }

                foreach (LandingSite l in LandingSite.Instances)
                {
                    if (l.IslandLevel == 1)
                    {
                        obj2.Add(l);
                    }
                }
                
                TASK_LOOT_WRECK_M_LANDING_SITES.SetValue(lootWreck, obj2);
            } else if (lootWreck == null)
            {
                TaskLootCargo.Arguments args = (TaskLootCargo.Arguments)TASK_LOOT_CARGO_M_DATA.GetValue(lootCargo);
                List<LandingSite> obj = (List<LandingSite>)TASK_LOOT_CARGO_LANDING_SITES.GetValue(args);
                List<LandingSite> obj2 = new List<LandingSite>();
                if (obj.Count > 0)
                {
                    obj.Clear();
                }
                
                foreach (LandingSite l in LandingSite.Instances)
                {
                    if (l.IslandLevel == 0)
                    {
                        obj2.Add(l);
                    }
                }
                TASK_LOOT_CARGO_LANDING_SITES.SetValue(args, obj2);
            }
        }
    }
}