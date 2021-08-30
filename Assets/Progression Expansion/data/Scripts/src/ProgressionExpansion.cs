using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProgressionExpansion : GameMod
{
    public GameObject Helper;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
    }

    public override void OnGameLoaded()
    {
        base.OnGameLoaded();
        //Helper = MissionFailed.Object();
        //Object.Instantiate(Helper);
    }
}
