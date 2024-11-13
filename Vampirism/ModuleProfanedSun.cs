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
        
        public override string GetSkillID() => "ProfanedSun";

        public override void ModuleLoaded(Vampire vampire)
        {
            base.ModuleLoaded(vampire);

            Mana vampireMana = moduleVampire?.Creature?.mana;
            if (!Utils.CheckError(() => vampireMana == null, "No mana component found for vampire upon profaned sun module load"))
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellLoadEvent += new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);
                vampireMana.OnSpellUnloadEvent += new Mana.SpellLoadEvent(OnSpellUnload);
            }
        }

        public override void ModuleUnloaded()
        {
            Mana vampireMana = moduleVampire?.Creature?.mana;
            if (!Utils.CheckError(() => vampireMana == null, "No mana component found for vampire upon profaned sun module unload"))
            {
                vampireMana.OnSpellLoadEvent -= new Mana.SpellLoadEvent(OnSpellLoad);
                vampireMana.OnSpellUnloadEvent -= new Mana.SpellLoadEvent(OnSpellUnload);
            }

            base.ModuleUnloaded();
        }

        private void OnSpellLoad(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellMergeFire spellMergeFire))
                return;

            string debugPrefix = GetDebugPrefix(nameof(OnSpellLoad));

            Debug.Log(debugPrefix + " Fire merge spell loaded");

            Creature casterCreature = caster?.ragdollHand?.creature;
            Creature moduleCreature = moduleVampire?.Creature;
            if (Utils.CheckError(() => casterCreature == null, debugPrefix + " Spell caster creature is null")
                || Utils.CheckError(() => moduleCreature == null, debugPrefix + " Module creature is null")
                || Utils.CheckError(() => casterCreature != moduleCreature, debugPrefix + " Caster creature is not module creature")) return;

            spellMergeFire.OnThrowEvent -= new SpellMergeFire.ThrowEvent(OnThrow);
            spellMergeFire.OnThrowEvent += new SpellMergeFire.ThrowEvent(OnThrow);

        }

        private void OnSpellUnload(SpellData spell, SpellCaster caster = null)
        {
            if (!(spell is SpellMergeFire spellMergeFire))
                return;

            string debugPrefix = GetDebugPrefix(nameof(OnSpellUnload));

            Debug.Log(debugPrefix + " Fire merge spell unloaded");

            Creature casterCreature = caster?.ragdollHand?.creature;
            Creature moduleCreature = moduleVampire?.Creature;
            if (Utils.CheckError(() => casterCreature == null, debugPrefix + " Spell caster creature is null")
                || Utils.CheckError(() => moduleCreature == null, debugPrefix + " Module creature is null")
                || Utils.CheckError(() => casterCreature != moduleCreature, debugPrefix + " Caster creature is not module creature")) return;

            spellMergeFire.OnThrowEvent -= new SpellMergeFire.ThrowEvent(OnThrow);
        }

        private void OnThrow(SpellMergeFire spellMergeFire, ItemMagicAreaProjectile projectile, Vector3 velocity)
        {
            string debugPrefix = GetDebugPrefix(nameof(OnThrow));
            debugPrefix = debugPrefix ?? "NULL";

            if (projectile == null)
            {
                Debug.LogError(debugPrefix + " No projectile thrown");
                return;
            }

            Coroutine sunRoutine = moduleVampire?.StartCoroutine(SunRoutine(projectile));
            if (sunRoutine == null)
                Debug.LogError(debugPrefix + " Sun routine not started");
        }

        private IEnumerator SunRoutine(ItemMagicAreaProjectile projectile)
        {
            string debugPrefix = GetDebugPrefix(nameof(SunRoutine));

            Debug.Log(debugPrefix + " Sun routine started");
            
            SkillProfanedSun profanedSunSkill = GetSkill<SkillProfanedSun>();
            ModuleSiphon siphonModule = moduleVampire?.skill?.GetModule<ModuleSiphon>("Siphon");
            while (true)
            {
                Debug.Log(GetDebugPrefix(nameof(SunRoutine)) + " Sun routine tick start");

                if (Utils.CheckError(() => projectile == null, debugPrefix + " Projectile is null")) break;
                if (Utils.CheckError(() => moduleVampire == null, debugPrefix + " Module vampire is null")) break;
                if (Utils.CheckError(() => profanedSunSkill == null, debugPrefix + " Skill data is null")) break;
                if (Utils.CheckError(() => siphonModule == null, debugPrefix + " Siphon module for module vampire is null")) break;

                Transform sunTransform = projectile.item?.transform ?? projectile.transform;
                if (Utils.CheckError(() => sunTransform == null, debugPrefix + " Transform used for sun aoe is null")) break;
                Vector3 effectOrigin = sunTransform.position;
                float effectRadius = profanedSunSkill.clampSunRadius ? Mathf.Lerp(profanedSunSkill.sunRadiusScale.x, profanedSunSkill.sunRadiusScale.y, moduleVampire.power.PowerLevel / profanedSunSkill.powerAtSunRadiusMax) : Mathf.LerpUnclamped(profanedSunSkill.sunRadiusScale.x, profanedSunSkill.sunRadiusScale.y, moduleVampire.power.PowerLevel / profanedSunSkill.powerAtSunRadiusMax);

                List<Creature> targets = Creature.allActive.FindAll(creature => creature != null && !creature.pooled && Vector3.Distance(creature.ragdoll.transform.position, effectOrigin) < effectRadius);
                if (targets != null && targets.Count > 0)
                {
                    foreach (Creature target in targets)
                    {
                        if (Utils.CheckError(() => target == null, debugPrefix + " Target is null")
                            || Utils.CheckError(() => target.pooled, debugPrefix + " Target is pooled"))
                            continue;

                        if (target.IsVampire(out Vampire targetVampire) && targetVampire == moduleVampire)
                            continue;

                        siphonModule.Siphon(moduleVampire, target);
                    }
                }
                else
                    Debug.Log(GetDebugPrefix(nameof(SunRoutine)) + " No targets found for profaned sun AOE");

                Debug.Log(GetDebugPrefix(nameof(SunRoutine)) + " Sun routine tick end");

                yield return new WaitForSeconds(profanedSunSkill.sunInterval);
            }

            Debug.Log(GetDebugPrefix(nameof(SunRoutine)) + " Sun routine ended");
        }
    }
}
