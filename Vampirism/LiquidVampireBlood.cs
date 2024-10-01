using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using Vampirism.Skill;

namespace Vampirism
{
    public class LiquidVampireBlood : LiquidData
    {
        public float potency = 20.0f;

        public override void OnLiquidReception(
            LiquidReceiver liquidReceiver, 
            float dilution, 
            LiquidContainer liquidContainer)
        {
            base.OnLiquidReception(liquidReceiver, dilution, liquidContainer);

            Creature receivingCreature = liquidReceiver?.GetComponentInParent<Creature>();
            if (receivingCreature == null)
                return;

            if (receivingCreature.IsVampire(out Vampire receivingVampire))
            {
                receivingCreature.Heal(potency * dilution);
                receivingVampire.GainPower(potency * dilution);
            }
            else
                receivingCreature.Vampirize(potency * dilution);
        }
    }
}
