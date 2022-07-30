using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelodicAlbuild.Version.Comparitors
{
    public static class Equal
    {
        /// <summary>
        /// Compares this Version to another Version and Determines if it is equal to the Compared Version.
        /// </summary>
        /// <param name="version">This Version</param>
        /// <param name="comparitiveVersion">The Version Compared To</param>
        /// <returns>Returns True if Both Versions are Equal.</returns>
        public static bool IsEqual(this Version version, Version comparitiveVersion)
        {
            return version.Major == comparitiveVersion.Major && version.Minor == comparitiveVersion.Minor && version.Patch == comparitiveVersion.Patch;
        }
    }
}
