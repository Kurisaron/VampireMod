using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using Vampirism.Skill;

namespace Vampirism
{
    /// <summary>
    /// Script used to designate a creature as a vampire and handle core vampire mechanics
    /// </summary>
    public class Vampire : MonoBehaviour
    {
        public PowerManager power { get; private set; }
        public SkillManager skill {  get; private set; }
        public SireManager sireline { get; private set; }
        public AppearanceManager appearance { get; private set; }

        public Creature creature { get; private set; }
        private bool isPlayer { get=> creature != null ? creature.isPlayer : false; }

        /// <summary>
        /// Event invoked just before a vampire is cured
        /// </summary>
        public event VampireEvent curedEvent;


        public delegate void VampireEvent(Vampire vampire);

        public class PowerManager : VampireManager
        {
            /// <summary>
            /// Level of power for the given vampire
            /// </summary>
            private float powerLevel = 0.0f;
            /// <summary>
            /// Level of power for the given vampire
            /// </summary>
            public float PowerLevel
            {
                get => powerLevel;
                private set
                {
                    powerLevel = value;
                }
            }

            /// <summary>
            /// Event broadcasted when a vampire gains power
            /// </summary>
            public event VampireEvent powerGainedEvent;

            public PowerManager(Vampire vampire, float level = 0.0f) : base(vampire)
            {
                powerLevel = level;
            }

            /// <summary>
            /// Adds power to the vampire's pool. The higher the power, the more potent their vampiric abilities
            /// </summary>
            /// <param name="amount">Amount of power to provide the vampire</param>
            public void GainPower(float amount)
            {
                powerLevel += amount;

                if (powerGainedEvent != null)
                    powerGainedEvent(vampire);
                VampireEvents.Instance?.InvokePowerGainedEvent(vampire);
            }
        }

        public class SkillManager : VampireManager
        {
            private Dictionary<string, (VampireModule module, Coroutine passive)> skills = new Dictionary<string, (VampireModule module, Coroutine passive)>();

            /// <summary>
            /// Event broadcasted when a vampire skill is added
            /// </summary>

            public SkillManager(Vampire vampire) : base(vampire)
            {
                if (skills == null)
                    skills = new Dictionary<string, (VampireModule module, Coroutine passive)>();
            }

            public void AddSkill(VampireSkill skill)
            {
                string methodName = nameof(AddSkill);
                if (Utils.CheckError(() => skill == null, "[" + methodName + "]: Skill data is null")) return;
                string skillID = skill.id;
                if (Utils.CheckError(() => skills.ContainsKey(skillID), "[" + methodName + "]: Vampire already has skill " + skillID)) return;

                VampireModule newModule = skill.CreateModule();
                newModule?.ModuleLoaded(vampire);
                Coroutine newPassive = newModule != null ? vampire.StartCoroutine(newModule.ModulePassive()) : null;

                skills.Add(skillID, (newModule, newPassive));
            }

            public void RemoveSkill(VampireSkill skill)
            {
                string methodName = nameof(RemoveSkill);
                if (Utils.CheckError(() => skill == null, "[" + methodName + "]: Skill data is null")) return;
                string skillID = skill.id;
                if (Utils.CheckError(() => !skills.ContainsKey(skillID), "[" + methodName + "]: Vampire does not have skill " + skillID)) return;

                (VampireModule module, Coroutine passive) skillStatus = skills[skillID];
                if (skillStatus.passive != null)
                {
                    vampire.StopCoroutine(skillStatus.passive);
                    skillStatus.passive = null;
                }
                if (skillStatus.module != null)
                {
                    skillStatus.module.ModuleUnloaded();
                    skillStatus.module = null;
                }

                skills.Remove(skillID);
            }

            public VampireModule GetModule(string skillID)
            {
                if (Utils.CheckError(() => !skills.ContainsKey(skillID), "[" + nameof(GetModule) + "] Skill " + skillID + " is not present in skill manager")) return null;

                return skills[skillID].module;
            }

            public ModuleType GetModule<ModuleType>(string skillID) where ModuleType : VampireModule => GetModule(skillID) as ModuleType;
        }

        public class SireManager : VampireManager
        {
            private Vampire sire;
            public Vampire Sire
            {
                get => sire;
                set
                {
                    RemoveSire();

                    sire = value;

                    AddSire();
                }
            }

            private List<Vampire> spawn = new List<Vampire>();
            public int SpawnCount { get => spawn != null ? spawn.Count : 0; }

            public SireManager(Vampire vampire, Vampire sire = null) : base(vampire)
            {

            }

            private void AddSire()
            {
                if (Utils.CheckError(() => sire == null, "[" + nameof(AddSire) + "]: Sire is null"))
                    return;

                vampire.curedEvent -= new VampireEvent(OnSireCured);
                vampire.curedEvent += new VampireEvent(OnSireCured);
                Creature sireCreature = sire.creature;
                if (sireCreature != null)
                    sireCreature.OnKillEvent += new Creature.KillEvent(OnSireKilled);

                Creature spawnCreature = vampire.creature;
                if (spawnCreature != null)
                {
                    int sireID = sireCreature.factionId;
                    int spawnID = spawnCreature.factionId;
                    if (sireID != spawnID)
                    {
                        spawnCreature.SetFaction(sireID);
                        if (!spawnCreature.isPlayer)
                        {
                            spawnCreature.brain.Load(spawnCreature.brain.instance.id);
                            spawnCreature.brain.canDamage = true;
                        }
                    }
                }

                sire.sireline.spawn.Add(vampire);
            }

            private void RemoveSire()
            {
                if (Utils.CheckError(() => sire == null, "[" + nameof(RemoveSire) + "]: Sire is null"))
                    return;

                vampire.curedEvent -= new VampireEvent(OnSireCured);
                Creature sireCreature = sire.creature;
                if (sireCreature != null)
                    sireCreature.OnKillEvent -= new Creature.KillEvent(OnSireKilled);

                sire.sireline.spawn.Remove(vampire);
            }

            /// <summary>
            /// Perform an action for each spawn
            /// </summary>
            /// <param name="action">Action delegate to perform on each vampire spawn</param>
            public void PerformSpawnAction(Action<Vampire> action)
            {
                if (action == null || spawn == null || spawn.Count == 0)
                    return;

                for (int i = 0; i < spawn.Count; i++)
                {
                    Vampire vampire = spawn[i];
                    if (vampire == null) continue;

                    action(vampire);
                }
            }

            private void OnSireCured(Vampire checkedSire)
            {
                if (checkedSire == null || checkedSire != sire) return;

                vampire.creature.CureVampirism();
            }

            private void OnSireKilled(CollisionInstance collisionInstance, EventTime eventTime)
            {
                vampire.creature.Kill();
            }

        }

        public class AppearanceManager : VampireManager
        {
            public (Color iris, Color sclera) HumanEyes { get; private set; }

            public AppearanceManager(Vampire vampire) : base(vampire)
            {
                Creature creature = vampire?.creature;
                if (creature == null) return;
                Color iris = creature.GetColor(Creature.ColorModifier.EyesIris);
                Color sclera = creature.GetColor(Creature.ColorModifier.EyesSclera);
                HumanEyes = (iris, sclera);
                RefreshEyes(vampire);

                vampire.power.powerGainedEvent -= new VampireEvent(RefreshEyes);
                vampire.power.powerGainedEvent += new VampireEvent(RefreshEyes);
                vampire.curedEvent -= new VampireEvent(OnCured);
                vampire.curedEvent += new VampireEvent(OnCured);
            }

            private void RefreshEyes(Vampire vampire)
            {
                Creature creature = vampire?.creature;
                if (creature == null || vampire != this.vampire) return;

                (Color iris, Color sclera) EyeColors = (Color.red, HumanEyes.sclera);
                creature.SetColor(EyeColors.iris, Creature.ColorModifier.EyesIris);
                creature.SetColor(EyeColors.sclera, Creature.ColorModifier.EyesSclera);
            }

            private void OnCured(Vampire vampire)
            {
                if (vampire == null || vampire.creature == null) return;

                Creature curedCreature = vampire.creature;

                curedCreature.SetColor(HumanEyes.iris, Creature.ColorModifier.EyesIris);
                curedCreature.SetColor(HumanEyes.sclera, Creature.ColorModifier.EyesSclera);
            }
        }

        public abstract class VampireManager
        {
            public Vampire vampire { get; private set; }

            protected VampireManager(Vampire vampire) => this.vampire = vampire;
        }

        public static class VampireUtility
        {
            public static Vampire Vampirize(Creature creature, float startingPower = 1.0f, Vampire sire = null)
            {
                // attempt to find a Vampire script attached to the given creature in case it is already present
                // add an instance of the Vampire script if it is not already present
                Vampire newVampire = creature.gameObject.GetComponent<Vampire>() ?? creature.gameObject.AddComponent<Vampire>();
                newVampire.creature = creature;
                newVampire.power = new PowerManager(newVampire, startingPower);
                newVampire.skill = new SkillManager(newVampire);
                newVampire.sireline = new SireManager(newVampire, sire);
                newVampire.appearance = new AppearanceManager(newVampire);

                VampireEvents.Instance.InvokeSireEvent(newVampire);

                return newVampire;
            }

            public static void Cure(Creature creature)
            {
                if (creature.IsVampire(out Vampire vampire))
                {
                    vampire.curedEvent(vampire);
                    VampireEvents.Instance.InvokeCuredEvent(creature, EventTime.OnStart);

                    MonoBehaviour.Destroy(vampire);

                    VampireEvents.Instance.InvokeCuredEvent(creature, EventTime.OnEnd);
                }
            }

            public static bool IsVampire(Creature creature, out Vampire vampire)
            {
                vampire = creature.gameObject.GetComponent<Vampire>();
                return vampire != null;
            }

        }

    }
}
