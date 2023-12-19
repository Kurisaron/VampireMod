﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class GarlicSyringeModule : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<GarlicSyringeComponent>();
        }
    }

    public class GarlicSyringeComponent : SyringeComponent
    {
        public override void InjectPlayer()
        {
            base.InjectPlayer();

            if (VampireMaster.local.isVampire)
            {
                VampireMaster.local.Vampirize(false);
            }
            else
            {
                
            }


        }
    }
}
