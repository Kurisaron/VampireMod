using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;

namespace Vampirism.Skill
{
    public class ModuleProfanedSun : VampireModule
    {
        public static SkillProfanedSun skill;
        
        private SpellMergeFire spell;
        public SpellMergeFire Spell
        {
            get => spell;
            set
            {
                if (spell == value)
                    return;
                
                if (spell != null)
                {
                    spell.OnThrowEvent -= new SpellMergeFire.ThrowEvent(OnThrow);
                }

                spell = value;

                if (spell != null)
                {
                    spell.OnThrowEvent -= new SpellMergeFire.ThrowEvent(OnThrow);
                    spell.OnThrowEvent += new SpellMergeFire.ThrowEvent(OnThrow);
                }
            }
        }

        private void OnThrow(SpellMergeFire spellMergeFire, ItemMagicAreaProjectile projectile, Vector3 velocity)
        {
            if (spellMergeFire == null || spell == null || spell != spellMergeFire)
                return;

            if (projectile == null)
                return;

            StartCoroutine(SunRoutine(projectile));
        }

        private IEnumerator SunRoutine(ItemMagicAreaProjectile projectile)
        {
            while (projectile != null && Vampire != null)
            {
                Vector3 effectOrigin = projectile.item?.transform?.position ?? projectile.transform.position;
                float effectRadius = skill.clampSunRadius ? Mathf.Lerp(skill.sunRadiusScale.x, skill.sunRadiusScale.y, Vampire.Power / skill.powerAtSunRadiusMax) : Mathf.LerpUnclamped(skill.sunRadiusScale.x, skill.sunRadiusScale.y, Vampire.Power / skill.powerAtSunRadiusMax);

                List<Creature> targets = Creature.allActive.FindAll(creature => creature != null && !creature.pooled && Vector3.Distance(creature.ragdoll.transform.position, effectOrigin) < effectRadius);
                if (targets == null || targets.Count <= 0)
                {
                    yield return new WaitForSeconds(skill.sunInterval);
                    continue;
                }

                foreach (Creature target in targets)
                {
                    if (target.pooled)
                        continue;

                    if (target.IsVampire(out Vampire targetVampire) && targetVampire == Vampire)
                        continue;

                    ModuleSiphon.Siphon(Vampire, target);
                }

                yield return new WaitForSeconds(skill.sunInterval);
            }
        }
    }
}
