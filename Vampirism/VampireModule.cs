using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism.Skill
{
    public abstract class VampireModule
    {
        public Vampire moduleVampire { get; private set; }

        /// <summary>
        /// Optional function for classes derived from VampireModule to override to define behaviour when the skill/module is added
        /// </summary>
        public virtual void ModuleLoaded(Vampire vampire)
        {
            moduleVampire = vampire;

            Type moduleType = GetType();
            if (moduleType != null)
                Debug.Log(moduleType.Name + " vampire module loaded");
        }

        /// <summary>
        /// Optional function for classes derived from VampireModule to override to define behaviour when the skill/module is removed
        /// </summary>
        public virtual void ModuleUnloaded()
        {
            Type moduleType = GetType();
            if (moduleType != null)
                Debug.Log(moduleType.Name + " vampire module unloaded");
        }

        /// <summary>
        /// Optional function for classes derived from VampireModule to override to define behaviour for passive abilities. 
        /// </summary>
        /// <returns>Enumerator for coroutine booted up by vampire when module is added to its skillset</returns>
        public virtual IEnumerator ModulePassive() => null;



        /// <summary>
        /// Abstract-declared method to force all classes derived from VampireModule to define the string for the id of its skill data
        /// </summary>
        /// <returns>ID for the skill data driving the current module</returns>
        public abstract string GetSkillID();

        /// <returns>Skill data from catalog with id matching GetSkillID() definition</returns>
        public SkillData GetSkill() => Catalog.GetData<SkillData>(GetSkillID());
        
        /// <typeparam name="SkillType">Type that derives from </typeparam>
        /// <returns>GetSkill() as SkillType</returns>
        public SkillType GetSkill<SkillType>() where SkillType : SkillData => GetSkill() as SkillType;



        /// <summary>
        /// Provides string to indicate the class/method source for a given line in the debugging log
        /// </summary>
        /// <param name="methodName">Name of the method to print to the log</param>
        /// <returns>"[ClassName]" by default, "[ClassName.methodName]" if methodName is not null, empty, or whitespace</returns>
        protected string GetDebugPrefix(string methodName = null) => this.GetDebugID(methodName);
    }

}
