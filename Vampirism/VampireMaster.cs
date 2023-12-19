using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Newtonsoft.Json;
using VLB;
using static ThunderRoad.SpellData;

namespace Vampirism
{
    /// <summary>
    /// Used to track progression and available abilities for the player
    /// </summary>
    public class VampireMaster : VampireScript<VampireMaster>
    {
        private (string folderAddress, string fileName) saveAddress = ("Mods/Vampirism/Data/", "VampireSaveData.json");
        private string levelUpEffectID = "levelUpFX";

        private Creature playerCreature;
        private VampireProgression progression;
        public VampireProgression Progression { get => progression; }

        public event CreatureLoadEvent creatureLoadEvent;

        #region THUNDERSCRIPT OVERRIDES
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            MasterSetup();
        }
        #endregion

        #region SETUP
        private void MasterSetup()
        {
            progression = new VampireProgression(this);
            VampireProgression.levelUpEvent += PlayerLevelUpSFX;

            InitialLoad();

            EventManager.onPossess += MasterPossessEvent;
        }
        #endregion

        #region LOADING/SAVING
        private void InitialLoad()
        {
            if (LoadSave(out VampireSaveData saveData))
            {
                progression.Vampirize(saveData.isVampire, saveData.level);
                progression.EarnXP(saveData.xp);
                progression.SetSkillPoints(saveData.skillPoints);
                
                List<Ability> abilities = progression.AbilityCatalog.Keys.ToList();
                for (int i = 0; i < abilities.Count; i++)
                {
                    Ability ability = abilities[i];
                    string abilityTypeName = ability.GetType().Name;
                    progression.SetAbilityLevel(ability, saveData.abilityLevels.ContainsKey(abilityTypeName) ? saveData.abilityLevels[abilityTypeName] : 0);
                }
            }
            else
            {
                WriteSave();
            }
        }

        // Used to load the save data
        private bool LoadSave(out VampireSaveData saveData)
        {
            if (File.Exists(Path.Combine(Application.streamingAssetsPath, saveAddress.folderAddress + saveAddress.fileName)))
            {
                saveData = JsonConvert.DeserializeObject<VampireSaveData>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, saveAddress.folderAddress + saveAddress.fileName)));
                return true;
            }
            else
            {
                saveData = null;
                return false;
            }
        }

        // Used to store save data to a file
        public void WriteSave()
        {
            VampireSaveData vampireSaveData = new VampireSaveData
            {
                isVampire = progression.isVampire,
                level = progression.Level.current,
                xp = progression.XP.current,
                skillPoints = progression.SkillPoints,
                abilityLevels = new Dictionary<string, int>()
            };
            
            foreach (KeyValuePair<Ability, (AbilityHandler handler, int currentLevel)> kvp in progression.AbilityCatalog)
            {
                vampireSaveData.abilityLevels.Add(kvp.Key.GetType().Name, kvp.Value.currentLevel);
            }

            string contents = JsonConvert.SerializeObject(vampireSaveData, Formatting.Indented);
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, saveAddress.folderAddress + saveAddress.fileName), contents);
        }


        private void MasterPossessEvent(Creature creature, EventTime time)
        {
            if (time == EventTime.OnStart) return; // Delay the CreatureLoad in case of any necessary setup

            CreatureLoad(creature);
        }

        private void CreatureLoad(Creature creature)
        {
            playerCreature = creature;

            List<Ability> abilities = progression.AbilityCatalog.Keys.ToList();
            foreach (Ability ability in abilities)
            {
                ability.SetupHandler();
            }

            playerCreature.SetVampireEyes(progression.Level.current);

            CreatureLoadEvent creatureLoad = creatureLoadEvent;
            if (Utils.CheckError(() => creatureLoad == null, "Creature Load Event is null")) return;
            creatureLoad(creature);
        }
        #endregion

        #region PLAYER GETTERS
        public Creature PlayerCreature { get => playerCreature; }
        public Player Player { get => playerCreature.player; }
        public Transform PlayerHead { get => playerCreature.ragdoll.headPart.gameObject.transform; }
        public Transform PlayerJaw { get => playerCreature.jaw; }
        public Locomotion PlayerLocomotion { get => playerCreature.player.locomotion; }
        public RagdollHand PlayerHand(Side side) => side == Side.Left ? playerCreature.handLeft : playerCreature.handRight;
        public SpellCaster PlayerCaster(Side side) => PlayerHand(side).caster;

        #endregion

        #region SOUND EFFECTS
        private void PlayerLevelUpSFX(int level)
        {
            if (Utils.CheckError(() => VampireSound.Instance == null, "Instance of VampireSound is null")) return;

            VampireSound.Instance.PlaySound(levelUpEffectID);
        }
        #endregion

        public delegate void CreatureLoadEvent(Creature creature);
    }

    public class VampireProgression
    {
        private VampireMaster master;
        
        public bool isVampire { get; private set; }
        private (int current, int max) level;
        public (int current, int max) Level { get => level; }
        private float currentXP;
        public (float current, float required) XP { get => (currentXP, GetRequiredXPForLevelUp()); }
        private int skillPoints;
        public int SkillPoints { get => skillPoints; }


        #region ABILITY PROGRESSION
        private Dictionary<Ability, (AbilityHandler handler, int currentLevel)> abilityCatalog;
        public Dictionary<Ability, (AbilityHandler handler, int currentLevel)> AbilityCatalog { get => abilityCatalog; }
        private bool catalogSetup = false;
        #endregion

        #region PROGRESSION EVENTS
        public static event LevelUpEvent levelUpEvent;
        #endregion

        public VampireProgression(VampireMaster master)
        {
            this.master = master;

            isVampire = false;
            level = (0, 50);
            currentXP = 0.0f;
            skillPoints = 0;

            catalogSetup = false;
            abilityCatalog = new Dictionary<Ability, (AbilityHandler handler, int currentLevel)>();
            List<Ability> abilities = Utils.GetAllDerived<Ability>().ToList();
            for (int i = 0; i < abilities.Count; i++)
            {
                Ability ability = abilities[i];
                abilityCatalog.Add(ability, (null, 0));
            }
            catalogSetup = true;

        }


        public void Vampirize(bool newState, int potency = 1)
        {
            if (isVampire && newState)
            {
                // Source is attempting to vampirize the player, but the player is already a vampire
                // Source will increase vampirism level instead according to its potency
                LevelUp(potency);
                return;
            }

            isVampire = newState;
            level.current = isVampire ? potency : 0;
            currentXP = 0.0f;
        }

        public void EarnXP(float amount)
        {
            currentXP += amount;

            while (currentXP >= XP.required)
            {
                currentXP -= XP.required;
                LevelUp();
            }
        }

        private void LevelUp(int amount = 1)
        {
            level.current += amount;
            skillPoints += amount;
            LevelUpEvent levelUp = levelUpEvent;
            if (levelUp == null) 
                return;
            levelUp(level.current);
        }

        public void SetSkillPoints(int amount) => skillPoints = amount;

        private float GetRequiredXPForLevelUp() => level.current * 110.0f;

        public void SetAbilityHandler(Ability ability, AbilityHandler handler)
        {
            int abilityLevel = abilityCatalog[ability].currentLevel;
            abilityCatalog[ability] = (handler, abilityLevel);
        }

        public void SetAbilityLevel(Ability ability, int level)
        {
            AbilityHandler handler = abilityCatalog[ability].handler;
            abilityCatalog[ability] = (handler, level);
        }

        public delegate void LevelUpEvent(int level);

    }

}
