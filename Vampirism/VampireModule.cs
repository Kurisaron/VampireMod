using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Vampirism.Skill
{
    public abstract class VampireModule : MonoBehaviour
    {
        private Vampire vampire;
        protected Vampire Vampire { get => vampire; }

        protected virtual void Awake()
        {
            vampire = GetComponent<Vampire>();
        }

        protected virtual void OnDestroy()
        {

        }
    }
}
