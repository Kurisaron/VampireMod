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
    public class Vampire : MonoBehaviour
    {
        
        private float power;
        public float Power
        {
            get => power;
            private set
            {
                power = value;
            }
        }

        public Creature Creature { get => gameObject.GetComponent<Creature>(); }
        private bool isPlayer
        {
            get
            {
                Creature creature = Creature;
                return creature != null && creature.isPlayer;
            }
        }

        private Vampire sire;
        private List<Vampire> spawn = new List<Vampire>();
        public Vampire Sire
        {
            get => sire;
            private set
            {
                if (sire != null)
                {
                    curedEvent -= new CuredEvent(OnSireCured);
                    Creature sireCreature = sire.Creature;
                    if (sireCreature != null)
                        sireCreature.OnKillEvent -= new Creature.KillEvent(OnSireKilled);

                    sire.spawn.Remove(this);
                }

                if (value != null)
                {
                    sire = value;

                    curedEvent -= new CuredEvent(OnSireCured);
                    curedEvent += new CuredEvent(OnSireCured);
                    Creature sireCreature = sire.Creature;
                    if (sireCreature != null)
                        sireCreature.OnKillEvent += new Creature.KillEvent(OnSireKilled);

                    int sireID = sire.Creature.factionId;
                    int spawnID = Creature.factionId;
                    if (sireID != spawnID)
                    {
                        Creature.SetFaction(sireID);
                        if (!Creature.isPlayer)
                        {
                            Creature.brain.Load(Creature.brain.instance.id);
                            Creature.brain.canDamage = true;
                        }
                    }

                    sire.spawn.Add(this);
                }
            }
        }
        public int SpawnCount
        {
            get
            {
                if (spawn == null)
                    spawn = new List<Vampire>();

                return spawn.Count;
            }
        }


        public (Color iris, Color sclera) HumanColors { get; private set; }
        

        // Event broadcasted when a vampire is created
        public static event SiredEvent sireEvent;
        // Event broadcasted when a vampire gains power
        public static event PowerGainedEvent powerGainedEvent;
        // Event broadcasted when a vampire ability module is added
        public static event ModuleEvent moduleAddedEvent;
        // Event broadcasted when a vampire is cured
        public static event CuredEvent curedEvent;

        public static Vampire Vampirize(Creature creature, float startingPower = 1.0f, Vampire sire = null)
        {
            // attempt to find a Vampire script attached to the given creature in case it is already present
            // add an instance of the Vampire script if it is not already present
            Vampire newVampire = creature.gameObject.GetComponent<Vampire>() ?? creature.gameObject.AddComponent<Vampire>();
            newVampire.Power = Math.Max(newVampire.Power, startingPower);
            newVampire.spawn = newVampire.spawn ?? new List<Vampire>();
            newVampire.Sire = sire;

            Color iris = creature.GetColor(Creature.ColorModifier.EyesIris);
            Color sclera = creature.GetColor(Creature.ColorModifier.EyesSclera);
            newVampire.HumanColors = (iris, sclera);
            newVampire.RefreshEyes();
            newVampire.SetKillEvent(true);

            SiredEvent vampireEvent = sireEvent;
            if (vampireEvent != null)
                vampireEvent(newVampire);


            return newVampire;
        }

        public static void Cure(Creature creature)
        {
            if (creature.IsVampire(out Vampire vampire))
            {
                vampire.SetKillEvent(false);
                
                creature.SetColor(vampire.HumanColors.iris, Creature.ColorModifier.EyesIris);
                creature.SetColor(vampire.HumanColors.sclera, Creature.ColorModifier.EyesSclera);

                CuredEvent vampireCured = curedEvent;
                if (vampireCured != null)
                    vampireCured(creature);

                MonoBehaviour.Destroy(vampire);
            }
        }

        public static bool IsVampire(Creature creature, out Vampire vampire)
        {
            vampire = creature.gameObject.GetComponent<Vampire>();
            return vampire != null;
        }

        /// <summary>
        /// Adds power to the vampire's pool. The higher the power, the more potent their vampiric abilities
        /// </summary>
        /// <param name="amount">Amount of power to provide the vampire</param>
        public void GainPower(float amount)
        {
            power += amount;

            RefreshEyes();

            PowerGainedEvent powerEvent = powerGainedEvent;
            if (powerEvent != null) powerEvent(this);

        }

        public T AddModule<T>() where T : VampireModule
        {
            T module = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();

            ModuleEvent addedEvent = moduleAddedEvent;
            if (module != null && addedEvent != null)
                addedEvent(this, module);

            return module;
        }

        public T GetModule<T>() where T : VampireModule
        {
            return gameObject.GetComponent<T>();
        }

        public VampireModule[] GetModules()
        {
            return gameObject.GetComponents<VampireModule>();
        }

        public void RemoveModule<T>(Action<T> preDestroyAction = null) where T : VampireModule
        {
            T module = gameObject.GetComponent<T>();
            if (module != null)
            {
                if (preDestroyAction != null)
                    preDestroyAction(module);
                
                Destroy(module);
            }
            else
                Debug.LogWarning("Vampire module " + module.GetType().Name + " not found on vampire, cannot remove");
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

        private void OnSireCured(Creature sire)
        {
            if (this.sire.Creature != sire)
                return;

            Cure(Creature);
        }

        private void OnSireKilled(CollisionInstance collisionInstance, EventTime eventTime)
        {
            Creature.Kill();
        }

        private void SetKillEvent(bool active)
        {
            Creature creature = Creature;
            if (creature == null) return;

            creature.OnKillEvent -= new Creature.KillEvent(OnKilled);
            if (active)
                creature.OnKillEvent += new Creature.KillEvent(OnKilled);
        }

        private void OnKilled(CollisionInstance collisionInstance, EventTime eventTime)
        {
            StartCoroutine(DestroyVampirismOnDeath());
        }

        // Coroutine used to delay destruction of vampirism script and modules until all on death events for them have occured
        private IEnumerator DestroyVampirismOnDeath()
        {
            if (sire != null)
                Sire = null;

            VampireModule[] modules = GetModules();
            if (modules != null && modules.Length > 0)
            {
                int count = modules.Length;
                for (int i = 0; i < count; i++)
                {
                    VampireModule module = modules[i];
                    if (module == null) continue;

                    yield return new WaitUntil(() => module.DestructionReady);

                    Destroy(module);
                }
            }

            SetKillEvent(false);
            Destroy(this);
        }

        private void RefreshEyes()
        {
            Creature creature = Creature;
            if (creature == null) return;

            (Color iris, Color sclera) EyeColors = (Color.red, HumanColors.sclera);
            creature.SetColor(EyeColors.iris, Creature.ColorModifier.EyesIris);
            creature.SetColor(EyeColors.sclera, Creature.ColorModifier.EyesIris);
        }

        public delegate void SiredEvent(Vampire vampire);
        public delegate void PowerGainedEvent(Vampire vampire);
        public delegate void ModuleEvent(Vampire vampire, VampireModule module);
        public delegate void CuredEvent(Creature creature);
    }
}
