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
    public class DarknessSpell : VampireSpell
    {
        // VARIABLES
        public float veiledStrikeCastRange = 1.0f; // distance from the player the check sphere can be
        public float veiledStrikeCheckSphereRadius = 2.0f; // radius of the check sphere
        private string veiledStrikeBoltID = "VeiledStrikeBolt";
        private ItemData veiledStrikeBoltData;
        private Creature currentTarget;

        // PROPERTIES
        private Vector3 CameraPos
        {
            get
            {
                return Camera.main.gameObject.transform.position;
            }
        }

        private Vector3 CameraForward
        {
            get
            {
                return Camera.main.gameObject.transform.forward;
            }
        }



        // FUNCTIONS
        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);

            veiledStrikeBoltData = Catalog.GetData<ItemData>(veiledStrikeBoltID);
        }

        public override void Unload()
        {
            base.Unload();

        }

        /*
        public override void Throw(Vector3 velocity)
        {
            base.Throw(velocity);

            // Run if Veiled Strike ability is unlocked
            if (VampireLevel.local.abilityLevels[VampireAbilityEnum.VeiledStrike] >= 1)
            {
                if (velocity.magnitude <= 0.5f) return;

                RaycastHit hit;
                Vector3 checkPos = Physics.Raycast(CameraPos, CameraForward, out hit, veiledStrikeCastRange) ? hit.point : CameraPos + (CameraForward.normalized * veiledStrikeCastRange);

                Collider[] foundColliders = Physics.OverlapSphere(checkPos, veiledStrikeCheckSphereRadius);
                List<Creature> foundCreatures = new List<Creature>();

                if (foundColliders != null && foundColliders.Length > 0)
                {
                    foreach (Collider collider in foundColliders)
                    {
                        // Check if current collider has a RagdollPart and Ragdoll attached to it
                        if (collider.gameObject.GetComponentInParent<RagdollPart>() != null && collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll != null && collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature != null && collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature != Player.currentCreature && !collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature.isKilled)
                        {
                            // Collider is attached to a living non-player creature
                            if (!foundCreatures.Contains(collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature))
                            {
                                foundCreatures.Add(collider.gameObject.GetComponentInParent<RagdollPart>().ragdoll.creature);
                            }
                            
                        }
                        else
                        {
                            // Collider is not attached to a living non-player creature
                        }

                    }
                }

                if (foundCreatures != null && foundCreatures.Count > 0)
                {
                    if (foundCreatures.Count > 1) foundCreatures = SortCreaturesByDistance(foundCreatures);

                    VeiledStrikeShoot(foundCreatures);
                }
                
            }
            
        }
        */

        private List<Creature> SortCreaturesByDistance(List<Creature> creatures)
        {
            List<Creature> tempList = new List<Creature>();
            
            while (tempList.Count < creatures.Count)
            {
                Creature closest = new Creature();
                float closestCreatureDistance = veiledStrikeCastRange + veiledStrikeCheckSphereRadius + 1.0f;
                
                foreach(Creature creature in creatures)
                {
                    if (!tempList.Contains(creature) && Vector3.Distance(creature.gameObject.transform.position, CameraPos) < closestCreatureDistance)
                    {
                        closestCreatureDistance = Vector3.Distance(creature.gameObject.transform.position, CameraPos);
                        closest = creature;
                    }
                }

                if (closest != null) tempList.Add(closest);

            }

            return tempList;
        }

    }
}
