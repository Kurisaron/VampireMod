using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class VampireScript<T> : ThunderScript where T : class
    {
        public static T Instance { get; private set; }
        public static string DebugKey { get => typeof(T).Name + ": "; }
        
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            if (Instance == null)
            {
                Instance = this as T;
            }
        }
    }
}
