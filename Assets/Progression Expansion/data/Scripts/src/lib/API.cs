using Newtonsoft.Json;
using System.IO;
using System.Net;
using UnityEngine;
using MelodicAlbuild.Version.Comparitors;
using MelodicAlbuild.Version.ExtensionTypes;

public class API
{
    public static MelodicAlbuild.Version.Version API_VERSION = new MelodicAlbuild.Version.Version("1.0.0");

    public static MelodicAlbuild.Version.Version updateVersion = new MelodicAlbuild.Version.Version("0.5.0");

    private bool needUpdate = false;
    private bool supportedAPI = false;

    private string pass = "2sWC&`Am2evGztQ8";

    public API(string uVersion)
    {
        updateVersion = new MelodicAlbuild.Version.Version(uVersion);
        Debug.Log("[Progression Expansion | API]: " + updateVersion);

        APIRequester("http://api.melodicalbuild.com");
        PERequester("http://api.melodicalbuild.com/volcanoids/mods/pe", Base64.EncodeToBase64(pass));

        ProgressionExpansion.updateText = UpdateVersioner();
    }

    public string UpdateVersioner()
    {
        if (needUpdate)
        {
            return "(Update Available)";
        }
        else
        {
            return "";
        }
    }

    private string html1;
    private string html2;

    private void PERequester(string url, string password)
    {
        if (supportedAPI)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "GET";
            request.Headers.Add("Authorization", "Bearer " + password);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html1 = reader.ReadToEnd();
            }
            var root = JsonConvert.DeserializeObject<PE>(html1);
            Debug.Log(root);
            ProgressionExpansion.shortVersion = root.short_version;
            ProgressionExpansion.updateName = root.update_name;
            ProgressionExpansion.version = root.version;
            if (new MelodicAlbuild.Version.Version(root.version).IsEqual(updateVersion))
            {
                Debug.Log("[Progression Expansion | API]: Mod is Up to Date");
            }
            else
            {
                Debug.Log("[Progression Expansion | API]: Mod is Not up to Date...");

                if (new MelodicAlbuild.Version.Version(root.version).IsGreaterThan(updateVersion))
                {
                    needUpdate = true;
                }
            }
        }
    }

    private void APIRequester(string url)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.ContentType = "application/json; charset=utf-8";
        request.Method = "GET";

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
            html2 = reader.ReadToEnd();
        }

        var root = JsonConvert.DeserializeObject<APIClass>(html2);
        Debug.Log(root);
        VersionArray supportedVersions = new VersionArray(root.supported_versions);
        supportedAPI = supportedVersions.versions.Contains(API_VERSION);
        if (new MelodicAlbuild.Version.Version(root.version).IsEqual(API_VERSION))
        {
            Debug.Log("[Progression Expansion | API]: API is Up to Date");
        }
        else
        {
            Debug.Log("[Progression Expansion | API]: API is Not up to Date...");
        }
    }
}
