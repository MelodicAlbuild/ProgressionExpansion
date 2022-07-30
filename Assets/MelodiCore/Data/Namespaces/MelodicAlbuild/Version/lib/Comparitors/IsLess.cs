using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelodicAlbuild.Version.Comparitors
{
    public static class IsLess
    {
        /// <summary>
        /// Compares this Version to another Version and Determines if it is Less than the Compared Version.
        /// </summary>
        /// <param name="version">This Version</param>
        /// <param name="comparitiveVersion">The Version Compared To</param>
        /// <returns>Returns True if This Version is Less than the Compared Version.</returns>
        public static bool IsLessThan(this Version version, Version comparitiveVersion)
        {
            if (comparitiveVersion.IsEqual(version))
            {
                return false;
            }

            if (version.Major < comparitiveVersion.Major)
            {
                return true;
            }
            else
            {
                if (version.Minor < comparitiveVersion.Minor)
                {
                    return true;
                }
                else
                {
                    if (version.Patch < comparitiveVersion.Patch)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
