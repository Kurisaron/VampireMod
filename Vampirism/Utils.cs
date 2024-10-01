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

        #region List<Collider> Extensions
        public static void SortByDistance(this List<Collider> colliders, Vector3 position, bool shortestFirst = true)
        {
            
            try
            {
                colliders.Sort((a, b) =>
                {
                    if ((a == null && b == null) || a == b) return 0;
                    if (a == null) return shortestFirst ? -1 : 1;
                    if (b == null) return shortestFirst ? 1 : -1;

                    float aDistance = Vector3.Distance(a.transform.position, position);
                    float bDistance = Vector3.Distance(b.transform.position, position);

                    if (aDistance == bDistance) return 0;
                    if (aDistance < bDistance) return shortestFirst ? -1 : 1;
                    return shortestFirst ? 1 : -1;

                });
            }
            catch (Exception e)
            {
                string exceptionType = e.GetType().Name;
                Debug.LogWarning(exceptionType + " occured during collider sort");
            }
        }
        #endregion

        #region Func<bool> Extensions
        public static void AddCondition(this Func<bool> existing, Func<bool> newCondition)
        {
            if (newCondition == null) return;

            Func<bool> oldCondition = existing;
            existing = new Func<bool>(() => oldCondition() && newCondition());
        }
        #endregion

        #region Creature Extensions
        public static Vampire Vampirize(this Creature creature, float startingPower = 1.0f, Vampire sire = null)
        {
            return Vampire.VampireUtility.Vampirize(creature, startingPower, sire);
        }

        public static void CureVampirism(this Creature creature)
        {
            Vampire.VampireUtility.Cure(creature);
        }

        public static bool IsVampire(this Creature creature, out Vampire vampire)
        {
            return Vampire.VampireUtility.IsVampire(creature, out vampire);
        }

        public static Vampire AffirmVampirism(this Creature creature)
        {
            Vampire vampire = null;
            if (!creature.IsVampire(out vampire))
                vampire = creature.Vampirize();

            return vampire;
        }
        #endregion

    }
}
