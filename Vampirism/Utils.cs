using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public static class Utils
    {
        public static Assembly[] GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();
        public static IEnumerable<Type> GetAllTypes() => GetAssemblies().SelectMany(assembly => assembly.GetTypes());
        public static IEnumerable<Type> GetAllTypes(Func<Type, bool> predicate) => GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(predicate);

        /// <summary>
        /// Collect IEnumerable containing each class type deriving from T
        /// </summary>
        /// <typeparam name="T">Type of base class</typeparam>
        /// <returns>Collection of Types deriving from T</returns>
        public static IEnumerable<Type> GetAllDerived<T>() => GetAllDerived(typeof(T));

        public static IEnumerable<Type> GetAllDerived(Type type)
        {
            return GetAllTypes(check => check.IsSubclassOf(type));
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

        

        #region Func<bool> Extensions
        public static void AddCondition(this Func<bool> existing, Func<bool> newCondition)
        {
            if (newCondition == null) return;

            Func<bool> oldCondition = existing;
            existing = new Func<bool>(() => oldCondition() && newCondition());
        }
        #endregion

        #region Creature Extensions
        public static Vampire Vampirize(this Creature creature, int startingLevel = 1, float startingXP = 0.0f)
        {
            Vampire newVampire = creature.gameObject.GetComponent<Vampire>() ?? creature.gameObject.AddComponent<Vampire>();
            return newVampire.Init(creature, startingLevel, startingXP);
        }

        public static void CureVampirism(this Creature creature)
        {
            Vampire vampire = creature.gameObject.GetComponent<Vampire>();
            if (vampire != null) MonoBehaviour.Destroy(vampire);
            if (creature.isPlayer) VampireManager.Instance.SaveData = new VampireSaveData();
        }

        public static bool IsVampire(this Creature creature, out Vampire vampire)
        {
            vampire = creature.gameObject.GetComponent<Vampire>();
            return vampire != null;
        }
        #endregion

    }
}
