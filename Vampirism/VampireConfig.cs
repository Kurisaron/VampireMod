using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public class VampireConfig : VampireScript<VampireConfig>
    {
        #region CHEAT MODE FIELDS/PROPERTIES
        private bool cheatModeActive;
        public bool CheatModeActive
        {
            get => cheatModeActive;
            set
            {
                cheatModeActive = value;

            }
        }
        #endregion

        #region EYE COLOR FIELDS/PROPERTIES
        private (Color normal, Color min, Color max) irisColor;
        public (Color normal, Color min, Color max) IrisColor { get => irisColor; }
        private (Color min, Color max) scleraColor;
        public (Color min, Color max) ScleraColor { get => scleraColor; }
        #endregion

        #region THUNDERSCRIPT OVERRIDES
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);

            VampireProgression.levelUpEvent += Eyes_LevelUpEvent;
        }
        #endregion

        #region EYE COLOR FUNCTIONS
        private void Eyes_LevelUpEvent(int level)
        {
            if (Utils.CheckError(() => Player.currentCreature == null, "Current player creature is null")) return;

            Player.currentCreature.SetVampireEyes(level);
        }
        #endregion

        #region MODOPTIONS
        [ModOptionCategory("Cheats", 1)]
        [ModOption("Cheat Mode", "Enable cheat mode to raise up to the max level and unlock all abilities. No XP will be earned", "defaultValues", defaultValueIndex = 0, saveValue = false, order = 1)]
        private static void CheatMode(bool active)
        {
            Debug.Log(DebugKey + "Cheat Mode " + (active ? "" : "not ") + "active");
            VampireConfig.Instance.CheatModeActive = active;
        }

        private ModOption GetMyModOption(string folderName, string modOptionName)
        {
            ModOption myModOption = null;
            foreach (ModManager.ModData loadedMod in ModManager.loadedMods)
            {
                foreach (ModOption modOption in loadedMod.modOptions)
                {
                    if (loadedMod.folderName == folderName && modOption.name == modOptionName)
                    {
                        myModOption = modOption;
                        break;
                    }
                }
                if (myModOption != null)
                    break;
            }
            return myModOption;
        }
        #endregion
    }
}
