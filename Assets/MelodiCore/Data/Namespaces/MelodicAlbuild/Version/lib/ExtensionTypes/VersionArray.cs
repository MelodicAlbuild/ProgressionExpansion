using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelodicAlbuild.Version.ExtensionTypes
{
    /// <summary>
    /// A Mediator Conversion from String Array to Version Array.
    /// </summary>
    public class VersionArray
    {
        /// <summary>
        /// A Version Array from Compiled Strings.
        /// </summary>
        public Version[] versions { get; set; }

        /// <summary>
        /// Creates a Default Version based off of the provided String Array.
        /// 
        /// STRINGS MUST BE IN FORMAT (Major.Minor.Patch)
        /// </summary>
        /// <param name="versionStringArray">Versions to Decode in format (Major.Minor.Patch)</param>
        public VersionArray(string[] versionStringArray)
        {
            List<Version> vList = new List<Version>();

            foreach (string version in versionStringArray)
            {
                vList.Add(new Version(version));
            }

            versions = vList.ToArray();
        }
    }
}
