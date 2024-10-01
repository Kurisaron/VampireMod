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
        public Vampire moduleVampire;

        public virtual void ModuleLoaded(Vampire vampire)
        {
            moduleVampire = vampire;
        }

        public virtual void ModuleUnloaded() { }
        public virtual IEnumerator ModulePassive() => null;

        public abstract string GetSkillID();
        public SkillData GetSkill() => Catalog.GetData<SkillData>(GetSkillID());
        public SkillType GetSkill<SkillType>() where SkillType : SkillData => GetSkill() as SkillType;
    }

}
