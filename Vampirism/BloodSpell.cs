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

        public override void Load(SpellCaster spellCaster, Level level)
        {
            base.Load(spellCaster, level);

            manaConsumption = 0.0f;

            switch (spellCaster.ragdollHand.side)
            {
                case Side.Left:
                    BloodSpellHandler.local.spellLeft = this;
                    break;
                case Side.Right:
                    BloodSpellHandler.local.spellRight = this;
                    break;
                default:
                    break;
            }

        }

        public override void Unload()
        {
            base.Unload();

            switch (spellCaster.ragdollHand.side)
            {
                case Side.Left:
                    BloodSpellHandler.local.spellLeft = null;
                    break;
                case Side.Right:
                    BloodSpellHandler.local.spellRight = null;
                    break;
                default:
                    break;
            }
        }

        public override void Fire(bool active)
        {
            base.Fire(active);

            if (active)
            {
                spellCaster.DisableSpellWheel(this);
            }
            else
            {
                spellCaster.AllowSpellWheel(this);
            }

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
