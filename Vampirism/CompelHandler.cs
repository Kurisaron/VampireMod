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
    public class CompelHandler : Ability
    {
        // VARIABLES

        // FUNCTIONS
        public CompelHandler Init()
        {
            
            // Run when CompelHandler is attached
            EventManager.onCreatureHit -= new EventManager.CreatureHitEvent(Compel_OnCreatureHit);
            EventManager.onCreatureHit += new EventManager.CreatureHitEvent(Compel_OnCreatureHit);

            return this;
        }

        public void Compel_OnCreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (VampireMaster.local.abilityLevels[VampireAbilityEnum.Compel] <= 0 || creature.isPlayer || VampireMaster.local.vampireThralls == null || VampireMaster.local.vampireThralls.Count <= 0 || VampireMaster.local.vampireThralls.Contains(creature) || !collisionInstance.IsDoneByPlayer()) return;

            Creature[] closestThralls = new Creature[1];

            for (int i = 0; i < closestThralls.Length; i++)
            {
                closestThralls[i] = null;
                Creature closestThrall = null;
                float closestDistance = 10.0f;
                foreach (Creature thrall in VampireMaster.local.vampireThralls)
                {
                    if (Vector3.Distance(creature.gameObject.transform.position, thrall.gameObject.transform.position) < closestDistance && !closestThralls.Contains(thrall))
                    {
                        closestDistance = Vector3.Distance(creature.gameObject.transform.position, thrall.gameObject.transform.position);
                        closestThrall = thrall;
                    }
                }

                if (closestThrall != null)
                {
                    closestThralls[i] = closestThrall;
                }
            }
            
            foreach (Creature thrall in closestThralls)
            {
                if (thrall != null)
                {
                    thrall.brain.currentTarget = creature;
                    
                    // TO-DO: Empower thrall
                }
            }
        }


    }
}
