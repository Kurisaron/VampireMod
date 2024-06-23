using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;


namespace Vampirism
{
    
    public class VampireSpell : SpellCastCharge
    {
        // VARIABLES
        public bool isCasting;
        
        // FUNCTIONS
        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
        }


        public override void Fire(bool active)
        {
            base.Fire(active);

            isCasting = active;

            if(active)
            {
                // Spell has just started to be cast
            }
            else
            {
                // Spell has stopped being cast
            }
        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();

            if(isCasting)
            {

            }
        }

        
    }
}
