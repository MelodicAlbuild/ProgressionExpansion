using System;
using System.Collections.Generic;

public class VersionsArray
{
    public Versions[] versions { get; set; }

    /// <summary>
    /// Creates a Default Version based off of the provided String Array.
    /// 
    /// STRINGS MUST BE IN FORMAT (Major.Minor.Patch)
    /// </summary>
    /// <param name="versionStringArray">Versions to Decode in format (Major.Minor.Patch)</param>
    public VersionsArray(string[] versionStringArray)
    {
        List<Versions> vList = new List<Versions>();

        foreach (string version in versionStringArray)
        {
            vList.Add(new Versions(version));
        }

        versions = vList.ToArray();
    }
}
