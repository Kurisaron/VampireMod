using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public abstract class VampireModule : MonoBehaviour
    {
        private Vampire vampire;
        protected Vampire Vampire { get => vampire; }

        public bool DestructionReady { get; protected set; }


        protected virtual void Awake()
        {
            vampire = GetComponent<Vampire>();
            DestructionReady = true;
        }

        protected virtual void OnDestroy()
        {

        }
    }
}
