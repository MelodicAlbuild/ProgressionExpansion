using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLiquidProducer : DataComponent
{
    public LiquidRecipe ActiveRecipe;
    public float Progress;
    public int QueueSize;
    public bool IsPaused;
}
