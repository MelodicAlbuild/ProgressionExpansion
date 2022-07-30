using System.Collections.Generic;

namespace Progression_Expansion.data.Scripts.src.data
{
    public class PEDictionaryStorage
    {
        private static Dictionary<QuestRuntime, Quest> questDictionary = new Dictionary<QuestRuntime, Quest>();

        public static void SetAllQuestInformation(Dictionary<QuestRuntime, Quest> dict)
        {
            questDictionary = dict;
        }
    }
}