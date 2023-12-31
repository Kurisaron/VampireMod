﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class VampireManager : VampireScript<VampireManager>
    {
        private AbilityManager abilityManager;
        public List<Ability> Abilities { get => abilities; }
        public Dictionary<Ability, int> DefaultAbilities
        {
            get
            {
                if (abilities == null) return null;

                Dictionary<Ability, int> dict = new Dictionary<Ability, int>();
                for (int i = 0; i < abilities.Count; i++)
                {
                    Ability ability = abilities[i];
                    if (ability == null) continue;
                    dict.Add(ability, ability.BaseLevel);
                }
                return dict;
            }
        }
        private VampireSaveData saveData;
        public VampireSaveData SaveData
        {
            get => saveData;
            set
            {
                saveData = value;
                WriteSave();
            }
        }
        private string folderAddress { get => Path.Combine(Application.streamingAssetsPath, "Mods", "Vampirism", "Data"); }
        private string saveAddress { get => Path.Combine(folderAddress, "VampireSaveData.json"); }



        #region OVERRIDES
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            LoadSave();

            EventManager.onPossess += Manager_PossessEvent;
        }

        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
        }
        #endregion

        #region EVENT PUBLISHERS
        public static event PossessLoadEvent possessLoadEvent;
        #endregion

        #region EVENT SUBSCRIBERS
        private void Manager_PossessEvent(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnStart) return;
            PossessLoad();


            void PossessLoad()
            {
                Vampire playerVampire = null;
                
                if (saveData.isVampire)
                {
                    playerVampire = creature.Vampirize(saveData.level, saveData.xp, saveData.skillPoints);

                }
                
                PossessLoadEvent possessEvent = possessLoadEvent;
                if (possessEvent != null) possessEvent(creature, playerVampire);
            }
        }
        #endregion

        #region LOADING/SAVING
        private void LoadSave()
        {
            abilityManager = new AbilityManager();

            VampireSaveData tempData = File.Exists(saveAddress) ? JsonConvert.DeserializeObject<VampireSaveData>(File.ReadAllText(saveAddress)) : null;
            bool saveIsNotNull = tempData != null;

            saveData = new VampireSaveData()
            {
                isVampire = saveIsNotNull ? tempData.isVampire : false,
                level = saveIsNotNull ? tempData.level : 0,
                xp = saveIsNotNull ? tempData.xp : 0,
                skillPoints = saveIsNotNull ? tempData.skillPoints : 0,
                abilityLevels = new Dictionary<string, int>()
            };

            for (int i = 0; i < abilities.Count; i++)
            {
                Ability ability = abilities[i];
                string abilityName = ability.GetType().Name;
                saveData.abilityLevels[abilityName] = saveIsNotNull && tempData.abilityLevels.ContainsKey(abilityName) ? tempData.abilityLevels[abilityName] : 0;
            }

            WriteSave();
        }

        private void WriteSave()
        {
            string contents = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(saveAddress, contents);
        }
        #endregion

        #region DELEGATE DEFINITIONS
        public delegate void PossessLoadEvent(Creature creature, Vampire vampire);
        #endregion


    }
}
