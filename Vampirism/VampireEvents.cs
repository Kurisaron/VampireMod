using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using Vampirism.Skill;
using static Vampirism.VampireEvents;

namespace Vampirism
{
    public class VampireEvents : VampireScript<VampireEvents>
    {
        public static Vampire.VampireEvent sireEvent;
        public static CuredEvent curedEvent;
        public static Vampire.VampireEvent powerGainedEvent;

        public static SiphonEvent siphonEvent;
        
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
        }

        public void InvokeSireEvent(Vampire vampire) => InvokeVampireEvent(sireEvent, vampire);
        public void InvokePowerGainedEvent(Vampire vampire) => InvokeVampireEvent(powerGainedEvent, vampire);

        private void InvokeVampireEvent(Vampire.VampireEvent vampireEvent, Vampire vampire)
        {
            string methodName = nameof(InvokeVampireEvent);
            if (Utils.CheckError(() => vampireEvent == null, methodName + ": No vampire event present") || Utils.CheckError(() => vampire == null, methodName + ": No vampire present")) return;

            vampireEvent(vampire);
        }

        public void InvokeCuredEvent(Creature creature, EventTime eventTime)
        {
            string methodName = nameof(InvokeCuredEvent);
            if (Utils.CheckError(() => curedEvent == null, methodName + ": No vampire event present") || Utils.CheckError(() => creature == null, methodName + ": No creature present")) return;

            curedEvent(creature, eventTime);
        }

        public void InvokeSiphonEvent(Vampire source, Creature target, float damage)
        {
            string methodName = nameof(InvokeSiphonEvent);
            if (Utils.CheckError(() => siphonEvent == null, methodName + ": No vampire event present") || Utils.CheckError(() => source == null, methodName + ": No source vampire present") || Utils.CheckError(() => target == null, methodName + ": No target present")) return;

            siphonEvent(source, target, damage);
        }

        public delegate void CuredEvent(Creature creature, EventTime eventTime);
        public delegate void SiphonEvent(Vampire source, Creature target, float damage);
    }
}
