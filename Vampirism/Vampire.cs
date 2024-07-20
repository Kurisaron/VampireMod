using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class Vampire : MonoBehaviour
    {
        
        public Creature Creature { get => gameObject.GetComponent<Creature>(); }
        private bool isPlayer
        {
            get
            {
                Creature creature = Creature;
                return creature != null && creature.isPlayer;
            }
        }



        private (int current, int max) level = (1, 50);
        public int CurrentLevel { get => level.current; }
        public int MaxLevel { get => level.max; }
        public float LevelScale {
            get
            {
                if (level.current <= 0)
                    return 0.0f;

                return (float)level.current / (float)level.max;
            }
        }


        private float xp = 0.0f;
        public float XP { get => xp; }



        private Vampire sire;
        private List<Vampire> spawn = new List<Vampire>();
        public Vampire Sire
        {
            get => sire;
            private set
            {
                if (sire != null)
                {
                    curedEvent -= new VampireCuredEvent(OnSireCured);
                    Creature sireCreature = sire.Creature;
                    if (sireCreature != null)
                        sireCreature.OnKillEvent -= new Creature.KillEvent(OnSireKilled);

                    sire.spawn.Remove(this);
                    Creature.OnKillEvent -= new Creature.KillEvent(OnKilled);
                }

                if (value != null)
                {
                    sire = value;

                    curedEvent += new VampireCuredEvent(OnSireCured);
                    Creature sireCreature = sire.Creature;
                    if (sireCreature != null)
                        sireCreature.OnKillEvent += new Creature.KillEvent(OnSireKilled);

                    int sireID = sire.Creature.factionId;
                    int spawnID = Creature.factionId;
                    if (sireID != spawnID)
                        Creature.SetFaction(sireID);

                    sire.spawn.Add(this);
                    Creature.OnKillEvent += new Creature.KillEvent(OnKilled);
                }
            }
        }
        public (int current, int max) SpawnCount
        {
            get
            {
                if (spawn == null)
                    spawn = new List<Vampire>();

                (int lower, int upper) limit = (1, 10);
                int maxForLevel = CurrentLevel <= 0 ? 0 : Mathf.FloorToInt(Mathf.LerpUnclamped(limit.lower, limit.upper, LevelScale));
                return (spawn.Count, maxForLevel);
            }
        }



        public (Color iris, Color sclera) HumanColors { get; private set; }
        public (Color iris, Color sclera) VampireColors
        {
            get
            {
                Color iris = Color.Lerp(new Color(0.5f, 0.0f, 0.0f, 1.0f), Color.red, LevelScale);
                Color sclera = Color.Lerp(HumanColors.sclera, Color.black, LevelScale);
                return (iris, sclera);
            }
        }


        // Event broadcasted when a vampire is created
        public static event SiredEvent sireEvent;
        // Event broadcasted when a vampire earns xp
        public static event XPEarnedEvent xpEarnedEvent;
        // Event broadcasted when a vampire levels up
        public static event LevelEvent levelEvent;
        // Event broadcasted when a vampire is cured
        public static event CuredEvent curedEvent;

        public static Vampire Vampirize(Creature creature, int startingLevel = 1, float startingXP = 0.0f, Vampire sire = null)
        {
            // attempt to find a Vampire script attached to the given creature in case it is already present
            // add an instance of the Vampire script if it is not already present
            Vampire newVampire = creature.gameObject.GetComponent<Vampire>() ?? creature.gameObject.AddComponent<Vampire>();
            newVampire.level.current = Math.Max(newVampire.level.current, startingLevel);
            newVampire.xp = Mathf.Max(newVampire.xp, startingXP);
            newVampire.spawn = newVampire.spawn ?? new List<Vampire>();
            newVampire.Sire = sire;

            Color iris = creature.GetColor(Creature.ColorModifier.EyesIris);
            Color sclera = creature.GetColor(Creature.ColorModifier.EyesSclera);
            newVampire.HumanColors = (iris, sclera);
            newVampire.RefreshEyes(newVampire);
            levelEvent += new LevelEvent(newVampire.RefreshEyes);

            SiredEvent vampireEvent = sireEvent;
            if (vampireEvent != null)
                vampireEvent(newVampire);


            return newVampire;
        }

        public static void Cure(Creature creature)
        {
            if (creature.IsVampire(out Vampire vampire))
            {
                creature.SetColor(vampire.HumanColors.iris, Creature.ColorModifier.EyesIris);
                creature.SetColor(vampire.HumanColors.sclera, Creature.ColorModifier.EyesSclera);
                levelEvent -= new LevelEvent(vampire.RefreshEyes);

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
        /// Add xp to this vampire's current pool. Contributes to leveling up vampire powers
        /// </summary>
        /// <param name="amount">Amount of xp to provide the vampire</param>
        public void EarnXP(float amount)
        {
            xp += amount;

            float required = GetXPRequirement(level.current);
            int levelUpAmount = 0;
            while (xp >= required) 
            {
                xp -= required;
                levelUpAmount += 1;
                required = GetXPRequirement(level.current + levelUpAmount);
            }

            if (levelUpAmount > 0) LevelUp(levelUpAmount);

            XPEarnedEvent vampireXPEvent = xpEarnedEvent;
            if (vampireXPEvent != null) vampireXPEvent(this);

            // LOCAL FUNCTION
            float GetXPRequirement(int levelAmount) => (50.0f * Mathf.Pow(level.current, 2.0f)) + (20.0f * level.current);
        }

        public void LevelUp(int amount = 1)
        {
            level.current += amount;

            LevelEvent vampireLevel = levelEvent;
            if (vampireLevel != null) vampireLevel(this);

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

        private void OnKilled(CollisionInstance collisionInstance, EventTime eventTime)
        {
            if (sire != null)
                Sire = null;
        }

        private void RefreshEyes(Vampire vampire)
        {
            if (vampire == null || this != vampire) return;

            Creature creature = vampire.Creature;
            if (creature == null) return;

            (Color iris, Color sclera) EyeColors = CurrentLevel <= 0 ? HumanColors : VampireColors;
            creature.SetColor(EyeColors.iris, Creature.ColorModifier.EyesIris);
            creature.SetColor(EyeColors.sclera, Creature.ColorModifier.EyesIris);
        }

        public delegate void SiredEvent(Vampire vampire);
        public delegate void XPEarnedEvent(Vampire vampire);
        public delegate void LevelEvent(Vampire vampire);
        public delegate void CuredEvent(Creature creature);
    }
}
