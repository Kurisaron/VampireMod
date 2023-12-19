﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public static class Utils
    {
        /// <summary>
        /// Collect IEnumerable of base class type T containing instances for each class type deriving from T. Use only for running virtual functions
        /// </summary>
        /// <typeparam name="T">Type of base class</typeparam>
        /// <returns>Collection of new instances of classes deriving from T, stored as T</returns>
        public static IEnumerable<T> GetAllDerived<T>() where T : class
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(T))).Select(type => Activator.CreateInstance(type) as T);
        }
        
        /// <summary>
        /// Check if the error condition is true, print message to log if so
        /// </summary>
        /// <param name="errorCondition">Condition that error would occur from</param>
        /// <param name="errorMessage">Message to log for the error</param>
        /// <returns>True = error occurs</returns>
        public static bool CheckError(Func<bool> errorCondition, string errorMessage)
        {
            Func<bool> condition = errorCondition;
            if (condition == null)
            {
                Debug.LogError("Error Condition Invalid (CheckError)");
                return true;
            }

            bool error = condition();
            if (error) Debug.LogError(errorMessage);
            return error;
        }

        //============
        // EXTENSIONS
        //============

        public static void AddCondition(this Func<bool> existing, Func<bool> newCondition)
        {
            if (newCondition == null) return;

            Func<bool> oldCondition = existing;
            existing = new Func<bool>(() => oldCondition() && newCondition());
        }

        public static Vampire Vampirize(this Creature creature, int startingLevel = 1, float startingXP = 0.0f, int startingPoints = 0)
        {
            Vampire newVampire = creature.gameObject.GetComponent<Vampire>() ?? creature.gameObject.AddComponent<Vampire>();
            newVampire.Init(creature, startingLevel, startingXP, startingPoints);
            return newVampire;
        }

        public static void CureVampirism(this Creature creature)
        {
            Vampire vampire = creature.gameObject.GetComponent<Vampire>();
            if (vampire != null) MonoBehaviour.Destroy(vampire);
        }

        public static void SetVampireEyes(this Creature creature, int level = 1)
        {
            (Color normal, Color min, Color max) irisColor = VampireConfig.Instance.IrisColor;
            (Color min, Color max) scleraColor = VampireConfig.Instance.ScleraColor;

            VampireProgression progression = VampireMaster.Instance.Progression;
            float invLerp = Mathf.InverseLerp(1, progression.Level.max, progression.Level.current);
            Color iris = level > 0 ? Color.Lerp(irisColor.min, irisColor.max, invLerp) : irisColor.normal;
            Color sclera = level > 0 ? Color.Lerp(scleraColor.min, scleraColor.max, invLerp) : scleraColor.min;

            creature.SetColor(iris, Creature.ColorModifier.EyesIris, true);
            creature.SetColor(sclera, Creature.ColorModifier.EyesSclera, true);
        }
    }
}
