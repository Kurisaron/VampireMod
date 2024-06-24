using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vampirism
{
    public class VampireSaveData
    {
        public int level { get; set; }
        public float xp { get; set; }

        /// <summary>
        /// Default constructor. Sets all member fields to default values
        /// </summary>
        public VampireSaveData()
        {
            level = 0;
            xp = 0;
        }

        /// <summary>
        /// Alternate constructor to set member fields to provided values
        /// </summary>
        /// <param name="level"></param>
        /// <param name="xp"></param>
        public VampireSaveData(int level, float xp)
        {
            this.level = level;
            this.xp = xp;
        }
    }
}
