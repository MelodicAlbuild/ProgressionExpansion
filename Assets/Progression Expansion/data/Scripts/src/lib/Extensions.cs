using System.Collections.Generic;
using System.Reflection;
using Progression_Expansion.data.Scripts.src.data;

public static class Extensions
{
    private static readonly FieldInfo QUEST_MANAGER_M_QUESTS = typeof(QuestManager).GetField("m_quests", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo QUEST_LINE_M_QUESTS = typeof(QuestLine).GetField("m_quests", BindingFlags.Instance | BindingFlags.NonPublic);
    
    public static void Store(this QuestManager qm)
    {
        var questsArr = qm.Quests;
        Dictionary<QuestRuntime, Quest> questDictionary = new Dictionary<QuestRuntime, Quest>();
        foreach (var obj in questsArr)
        {
            questDictionary.Add(obj, obj.Quest);
        }
        PEDictionaryStorage.SetAllQuestInformation(questDictionary);
    }

    public static void ClearQuests(this QuestManager qm)
    {
        var questsArr = (Quest[]) QUEST_MANAGER_M_QUESTS.GetValue(qm);
        List<Quest> storedQuests = new List<Quest>();
        foreach (Quest q in questsArr)
        {
            if (q.name.StartsWith('E'))
            {
                storedQuests.Add(q);
            }

            if (q.name is "Q01_CraftCore" or "Q02_ClaimDrillship")
            {
                storedQuests.Add(q);
            }
        }
        QUEST_MANAGER_M_QUESTS.SetValue(qm, storedQuests.ToArray());
    }
    
    public static void ClearQuests(this QuestManager qm, string[] exceptions)
    {
        var questsArr = (Quest[]) QUEST_MANAGER_M_QUESTS.GetValue(qm);
        List<Quest> storedQuests = new List<Quest>();
        foreach (Quest q in questsArr)
        {
            if (q.name.StartsWith('E'))
            {
                storedQuests.Add(q);
            }

            if (exceptions.Contains(q.name))
            {
                storedQuests.Add(q);
            }
        }
        QUEST_MANAGER_M_QUESTS.SetValue(qm, storedQuests.ToArray());
    }

    public static void SetQuests(this QuestLine qm, List<Quest> quests)
    {
        QUEST_LINE_M_QUESTS.SetValue(qm, quests);
    }
}
