using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetShaderOffset : MonoBehaviour
{
    [Range(0f, 1f)]
    public float offset;

    private void Update()
    {
        SetOffset(offset);
    }

    public void SetOffset(float val)
    {
        Shader.SetGlobalFloat("_offset", val);
    }
}