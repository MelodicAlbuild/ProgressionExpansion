using System;

namespace MelodicAlbuild.Version
{
    /// <summary>
    /// A Semantic Versioning Class
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Major Version Number
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Minor Version Number
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Patch Version Number
        /// </summary>
        public int Patch { get; set; }

        /// <summary>
        /// Creates a Default Version with 1 Variable.
        /// 
        /// Other Numbers Default to 0.
        /// </summary>
        /// <param name="mj">Major Version Number</param>
        public Version(int mj)
        {
            Major = mj;
            Minor = 0;
            Patch = 0;
        }

        /// <summary>
        /// Creates a Default Version with 2 Variables.
        /// 
        /// Other Numbers Default to 0.
        /// </summary>
        /// <param name="mj">Major Version Number</param>
        /// <param name="mi">Minor Version Number</param>
        public Version(int mj, int mi)
        {
            Major = mj;
            Minor = mi;
            Patch = 0;
        }

        /// <summary>
        /// Creates a Default Version with 3 Variables.
        /// </summary>
        /// <param name="mj">Major Version Number</param>
        /// <param name="mi">Minor Version Number</param>
        /// <param name="pt">Patch Version Number</param>
        public Version(int mj, int mi, int pt)
        {
            Major = mj;
            Minor = mi;
            Patch = pt;
        }

        /// <summary>
        /// Creates a Default Version based off of the provided String.
        /// 
        /// STRING MUST BE IN FORMAT (Major.Minor.Patch)
        /// </summary>
        /// <param name="versionString">Version to Decode in format (Major.Minor.Patch)</param>
        public Version(string versionString)
        {
            string[] nums = versionString.Split('.');

            //foreach(string s in nums)
            //{
            //    Debug.Log(s);
            //}

            Major = Convert.ToInt32(nums[0]);
            Minor = Convert.ToInt32(nums[1]);
            Patch = Convert.ToInt32(nums[2]);
        }

        public override string ToString()
        {
            return "" + Major + "." + Minor + "." + Patch;
        }
    }
}
