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
        [JsonProperty(Required = Required.Always)]
        public bool isVampire { get; set; }
        public int level { get; set; }
        public float xp { get; set; }
        public int skillPoints { get; set; }
        public Dictionary<string, int> abilityLevels { get; set; } = new Dictionary<string, int>();
    }
}
