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
    }
}
