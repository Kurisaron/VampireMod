using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace Vampirism
{
    public class BloodSpell : VampireSpell
    {
        // VARIABLES
        public ItemData selectionSphereData;
        public Item currentSphere;

        //=====================================================
        // CORE FUNCTIONS
        //=====================================================

        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);


        }

        public override void Unload()
        {
            base.Unload();

        }

        public override void Fire(bool active)
        {
            base.Fire(active);


        }

        public override void UpdateCaster()
        {
            base.UpdateCaster();

            if (isCasting)
            {

            }
            else
            {

            }
        }


    }
}
