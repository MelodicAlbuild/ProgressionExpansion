using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Progression_Expansion.data.Extras;
using Progression_Expansion.data.Scripts.src.data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionExpansion : GameMod
{
    private static Quest[] alteredQuests;
    private static FieldInfo mQuestsField;
    private static FieldInfo mIslandQuestsField;
    private static QuestManager mIslandQuestManager;
    private static QuestLine mIslandQuestLine;

    private Scene newScene;
    private bool questsLoaded = false;

    public string updateVersion;

    public static string shortVersion = "0.0";
    public static string version = "0.0.0";
    public static string updateName = "";
    public static string updateText = "";
    public override void Load()
    {
        base.Load();
        SceneManager.sceneLoaded += OnSceneLoaded;

        updateVersion = Owner.Manifest.Version;
        Debug.Log("[Progression Expansion | Load]: " + Owner.Manifest.Version);
        new API(updateVersion);

        var log = new StringWriter();
        var modName = nameof(ProgressionExpansion);

        log.Write($"{modName} loading.");

        var harmony = new Harmony(GUID.Create().ToString());
        harmony.PatchAll(GetType().Assembly);

        var i = 0;
        foreach (var patchedMethod in harmony.GetPatchedMethods())
        {
            log.Write($"\r\nPatched: {patchedMethod.DeclaringType?.FullName}:{patchedMethod}");
            i++;
        }
        log.Write($"\r\nPatched {i} methods.");
        log.Write($"\r\n{modName} loaded.");
        Debug.Log(log.ToString());
    }

    public override void OnGameLoading(Scene gameScene, AsyncOperation gameSceneLoadOperation)
    {
        if (!newScene.isLoaded)
        {
            SceneManager.LoadScene("QuestAdditions", LoadSceneMode.Additive);
        }
    }

    public override void OnMainMenuLoaded(Scene mainMenuScene)
    {
        base.OnMainMenuLoaded(mainMenuScene);
        GameObject.Find("EarlyAccess").gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Progression Expansion\n " + shortVersion;
        GameObject.Find("Version").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = "Expansion Version: " + version + " | (" + updateName + ") " + updateText;
    }

    public override void OnGameLoaded(Scene gameScene)
    {
        base.OnGameLoaded(gameScene);

        foreach (var obj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (obj.name == "Gameplay")
            {
                foreach (var gObj in obj.GetChildren())
                {
                    if (gObj.name == "IslandGameplay")
                    {
                        mIslandQuestManager = gObj.gameObject.GetComponent<QuestManager>();
                    }

                    if (gObj.name == "Quests")
                    {
                        mIslandQuestLine = gObj.gameObject.GetComponentInChildren<QuestLine>();
                    }
                }
            }
        }

        new DepositSystem().InitDeposits();

        //Tests();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.LogWarning($"Additive scene loaded {scene.name}, isLoaded = {scene.isLoaded}");

        if (scene.name == "QuestAdditions" && !questsLoaded)
        {
            newScene = scene;
            if (mIslandQuestManager != null && mIslandQuestLine != null)
            {
                mIslandQuestsField = mIslandQuestManager.GetType().GetField("m_quests", BindingFlags.NonPublic | BindingFlags.Instance);
                mIslandQuestManager.Store();

                GameObject[] rootObjects = newScene.GetRootGameObjects();
                QuestLine line = rootObjects.First(o => o.name == "Line").GetComponent<QuestLine>();
                GameObject additions = rootObjects.First(o => o.name == "QuestAdditions");
                GameObject baseGameAlterations = additions.GetComponentsInChildren<Assignee>().First(o => o.gameObject.name == "BaseGame").gameObject;
                GameObject peGameAlterations = additions.GetComponentsInChildren<Assignee>().First(o => o.gameObject.name == "ProgressionExpansion").gameObject;

                List<string> exceptionList = new List<string>()
                {
                    "Q01_CraftCore",
                    "Q02_ClaimDrillship",
                    "Q08_RaidCog1_Intel",
                    "Q87_DestroyDrill",
                    "Q23_DestroyCogDrillshipT2",
                    "Q19_CaveAttack",
                    "Q26_TravelToArea2"
                };
                List<int> exceptionIndex = new List<int>()
                {
                    5, 16, 23, 25, 26
                };
                
                mIslandQuestManager.ClearQuests(exceptionList.ToArray());
                Debug.Log(" ");
                foreach (Quest q in baseGameAlterations.GetComponentsInChildren<Quest>())
                {
                    Debug.Log("[Progression Expansion | Game Loading]: Adding Base Game Quest " + q.Name + " to Island Quest Manager");
                    mIslandQuestManager.AddNewQuest(q);
                }
                foreach (Quest q in peGameAlterations.GetComponentsInChildren<Quest>())
                {
                    Debug.Log("[Progression Expansion | Game Loading]: Adding Progression Expansion Quest " + q.Name + " to Island Quest Manager");
                    mIslandQuestManager.AddNewQuest(q);
                }
                
                mIslandQuestManager.Store();
                List<Quest> questList = new List<Quest>();

                foreach (var obj in (mIslandQuestsField.GetValue(mIslandQuestManager) as Quest[]))
                {
                    questList.Add(obj);
                    Debug.Log("Dictionary Conversion: Quest Added to List Enable - " + obj.name);
                }

                for (int j = 0; j < exceptionList.Count - 2; j++)
                {
                    Quest q = questList.Find(o => o.name == exceptionList[j + 2]);
                    line.Insert(exceptionIndex[j], q);
                    Debug.Log("[Progression Expansion | Game Loading]: Adding Excluded Quest " + q.Name + " to Island Quest Line");
                }

                foreach (Quest quest in line.Quests)
                {
                    Debug.Log("[Progression Expansion | Game Loading]: [mIslandQuestLine.Quests] | " + quest);
                }

                mIslandQuestLine.SetQuests(line.Quests.ToList());
                questsLoaded = true;
            }
            else
            {
                Debug.Log("[Progression Expansion | Game Loading]: [mIslandQuestManager] set equal to null.");
            }
        }
    }

    private void Tests()
    {
        mQuestsField = typeof(QuestManager).GetField("m_quests", BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var obj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (obj.name == "Gameplay")
            {
                foreach (var gObj in obj.GetChildren())
                {
                    if (gObj.name == "IslandGameplay")
                    {
                        mIslandQuestManager = gObj.gameObject.GetComponent<QuestManager>();
                    }
                }
            }
        }

        mIslandQuestsField = mIslandQuestManager.GetType().GetField("m_quests", BindingFlags.NonPublic | BindingFlags.Instance);

        var questManagers = Resources.FindObjectsOfTypeAll<QuestManager>();

        if (questManagers != null)
        {
            Quest[] lamArray = (Quest[])mIslandQuestsField.GetValue(mIslandQuestManager);

            Debug.Log("[Progression Expansion | Questing]: [mIslandQuestManager.Quests] | " + lamArray.Length);
            Debug.Log(" ");

            foreach (var quest in lamArray)
            {
                Debug.Log("[Progression Expansion | Questing]: [mIslandQuestManager.Quests] | " + quest);
            }
            Debug.Log(" ");
            foreach (var questManager in questManagers)
            {
                // If we got here quests are enabled.
                ProcessQuestManager(questManager);
            }
            Debug.Log(" ");
            lamArray = (Quest[])mIslandQuestsField.GetValue(mIslandQuestManager);
            foreach (var quest in (Quest[])mIslandQuestsField.GetValue(mIslandQuestManager))
            {
                Debug.Log("[Progression Expansion | Questing]: [mIslandQuestManager.Quests] | " + quest);
            }

            Debug.Log(" ");
            Debug.Log("[Progression Expansion | Questing]: [mIslandQuestManager.Quests] | " + lamArray.Length);
        }
    }

    private void ProcessQuestManager(QuestManager questManager)
    {
        var quests = (Quest[])mQuestsField.GetValue(questManager);
        if (quests?.Length > 0)
        {
            if (alteredQuests != null)
            {
                // We've already setup the quests, just overwrite the list.
                mQuestsField.SetValue(questManager, alteredQuests);
                return;
            }

            // New quests haven't been setup yet, do so and cache the result.
            alteredQuests = CreateNewQuests(quests);

            mQuestsField.SetValue(questManager, alteredQuests);
            mQuestsField.SetValue(mIslandQuestManager, alteredQuests);
        }
    }

    private Quest[] CreateNewQuests(Quest[] oldQuests)
    {
        Quest[] tempArray = new Quest[3];
        int i = 0;
        foreach (var obj in oldQuests)
        {
            Debug.Log("[Progression Expansion | Questing]: [oldQuests] | " + obj);

            char[] array = obj.name.ToString().Take(3).ToArray();
            char questType = array[0];
            string numConvert = "" + array[1] + array[2];
            int questNum = Convert.ToInt32(numConvert);

            if (questType == 'Q')
            {
                if (questNum <= 3)
                {
                    tempArray[i] = obj;
                    i++;
                }
            }
        }

        List<Quest> quests = new List<Quest>();
        foreach (var obj in oldQuests)
        {
            char[] array = obj.name.ToString().Take(3).ToArray();
            char questType = array[0];
            if (questType == 'E')
            {
                quests.Add(obj);
            }
        }
        var tempList = tempArray.ToList();

        Debug.Log(" ");

        foreach (var obj in tempList)
        {
            quests.Add(obj);
        }
        var obj2 = GameObject.Find("QuestAdditions");
        if (obj2 != null)
        {
            foreach (Quest quest in obj2.GetComponentsInChildren<Quest>())
            {
                quests.Add(quest);
            }
        }

        foreach (var obj in oldQuests)
        {
            char[] array = obj.name.ToString().Take(3).ToArray();
            char questType = array[0];
            string numConvert = "" + array[1] + array[2];
            int questNum = Convert.ToInt32(numConvert);

            if (questType == 'Q')
            {
                if (questNum > 3)
                {
                    quests.Add(obj);
                }
            }
        }

        foreach (var obj in quests.ToArray())
        {
            Debug.Log("[Progression Expansion | Questing]: [quests] | " + obj);
        }

        alteredQuests = quests.ToArray();

        foreach (var obj in alteredQuests)
        {
            Debug.Log("[Progression Expansion | Questing]: [alteredQuests] | " + obj);
        }

        return tempArray;
    }
}