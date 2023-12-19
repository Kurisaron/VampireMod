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
    public class VampireSyringeModule : ItemModule
    {
        public int levelsToAdd = 1;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<VampireSyringeComponent>().module = this;
        }
    }

    public class VampireSyringeComponent : SyringeComponent
    {
        public VampireSyringeModule module;
        
        public override void InjectCreature()
        {
            base.InjectCreature();

            Creature creature = piercedCreature;
            if (creature != null)
            {
                if (creature.IsVampire(out Vampire vampire))
                    vampire.LevelUp(module.levelsToAdd);
                else
                    creature.Vampirize(module.levelsToAdd, 0, module.levelsToAdd - 1);
            }
        }
    }
}
