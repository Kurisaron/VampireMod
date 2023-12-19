using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace Vampirism
{
    public enum DurationMode
    {
        Before,
        After
    }
    
    public class ControlStep
    {
        //=========================
        // VARIABLES FOR ALL STEPS
        //=========================
        protected ControlStep skipTo;
        protected ControlStep parent;
        public Root root;
        private Condition condition;
        private List<ControlStep> children;
        private Action action;
        private Action onStateChange;
        private string actionName;
        private string name;
        public string label;
        private float lastChangedToTime;
        private float delay;
        private bool repeatable;
        private float repeatDelay;

        //===========
        // FUNCTIONS
        //===========
        protected ControlStep(
            ControlStep.Condition condition,
            ControlStep parent = null,
            Action onStateChange = null)
        {
            this.parent = parent;

            // Set variables based on if step is root or not
            bool isRoot = this.parent == null;
            this.root = isRoot ? new Root(this) : parent.root;
            if (isRoot) root.active = false;

            this.condition = condition;
            this.onStateChange = onStateChange ?? parent?.onStateChange;
            children = new List<ControlStep>();
        }

        public static ControlStep Start(Action onStateChange = null)
        {
            return new ControlStep(
                new Condition(() => true),
                onStateChange: onStateChange)
            {
                name = nameof(Start),
            };
        }

        public ControlStep Repeatable(
            bool repeatable = true, 
            float delay = 0.3f)
        {
            this.repeatable = repeatable;
            this.repeatDelay = delay;
            return this;
        }

        //================
        // THEN FUNCTIONS
        //================
        public ControlStep Then(
            Func<bool> startCondition,
            string name = "",
            float duration = 0.0f,
            Func<bool> endCondition = null,
            DurationMode mode = DurationMode.After,
            bool runOnChange = true,
            bool toggle = false)
        {
            ControlStep step = new ControlStep(new Condition(startCondition, toggle ? (Func<bool>)(() => !startCondition()) : endCondition, duration, mode, runOnChange), this)
            {
                name = name
            };
            children.Add(step);
            return step;
        }

        public ControlStep Then(params Tuple<string, Func<bool>>[] conditions)
        {
            if (conditions.Length == 0) return this;
            ControlStep step = Then(conditions[0].Item2, conditions[0].Item1);
            return conditions.Length > 1 ? step.Then(((IEnumerable<Tuple<string, Func<bool>>>)conditions).Skip(1).ToArray()) : step;
        }

        public ControlStep Then(params Tuple<string, Func<bool>[]>[] conditions)
        {
            if (conditions.Length == 0) return this;
            ControlStep step = Then(conditions[0].Item1, conditions[0].Item2);
            return conditions.Length > 1 ? step.Then(((IEnumerable<Tuple<string, Func<bool>[]>>)conditions).Skip(1).ToArray()) : step;
        }

        public ControlStep Then(
            string name = "",
            params Func<bool>[] conditions)
        {
            Queue<Func<bool>> source = new Queue<Func<bool>>(conditions);
            ControlStep step = Then(source.Dequeue(), name);
            while (source.Any()) step = step.Then(source.Dequeue());
            return step;
        }


        //===============
        // AND FUNCTIONS
        //===============
        public ControlStep And(
            string name,
            Func<bool> newCondition)
        {
            condition.And(newCondition);
            this.name = this.name + " AND " + name;
            return this;
        }

        public ControlStep And(params Tuple<string, Func<bool>>[] conditions)
        {
            for (int i = 0; i < conditions.Length; ++i)
            {
                Tuple<string, Func<bool>> condition = conditions[i];
                this.condition.And(condition.Item2);
                this.name = this.name + " AND " + condition.Item1;
            }
            return this;
        }

        public ControlStep AndNot(
            string name,
            Func<bool> newCondition)
        {
            condition.AndNot(newCondition);
            this.name = this.name + " NOT " + name;
            return this;
        }

        public ControlStep AndNot(params Tuple<string, Func<bool>>[] conditions)
        {
            for (int i = 0; i < conditions.Length; ++i)
            {
                Tuple<string, Func<bool>> condition = conditions[i];
                this.condition.AndNot(condition.Item2);
                this.name = this.name + " NOT " + condition.Item1;
            }
            return this;
        }

        //==============
        // OR FUNCTIONS
        //==============
        public ControlStep Or(
            string name,
            Func<bool> newCondition)
        {
            condition.Or(newCondition);
            this.name = this.name + " OR " + name;
            return this;
        }

        public ControlStep Or(params Tuple<string, Func<bool>>[] conditions)
        {
            for (int i = 0; i < conditions.Length; ++i)
            {
                Tuple<string, Func<bool>> condition = conditions[i];
                this.condition.Or(condition.Item2);
                this.name = this.name + " OR " + condition.Item1;
            }
            return this;
        }

        //=====================
        // MISC STEP FUNCTIONS
        //=====================
        public ControlStep Do(
            Action action,
            string actionName = "")
        {
            this.action = action;
            this.actionName = actionName;
            return this;
        }

        public ControlStep After(float delay)
        {
            ControlStep step = Then(() => true, string.Format("Wait {0:0.##}s", delay));
            step.delay = delay;
            return step;
        }

        public void Reset()
        {
            skipTo = null;
            condition.Reset();
            for (int index = 0; index < children.Count; ++index)
                children[index].Reset();
        }

        protected bool Check() => condition.Evaluate();

        protected void DoUpdate()
        {
            if (skipTo == null && (parent == null || (Time.time - parent.lastChangedToTime) > delay))
            {
                foreach(ControlStep child in children)
                {
                    if(child.Check())
                    {
                        Action action = child.action;
                        if (action != null) action();

                        if (child.condition.runOnChange)
                        {
                            Action onStateChange = this.onStateChange;
                            if (onStateChange != null) onStateChange();
                        }
                        skipTo = child;
                        lastChangedToTime = Time.time;
                        break;
                    }
                }
            }
            if (skipTo == null) return;
            skipTo.DoUpdate();
        }

        public void Update()
        {
            if (parent == null)
            {
                DoUpdate();
            }
            else
            {
                parent.Update();
            }
        }

        public string GetCurrentPath()
        {
            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(actionName)) return skipTo != null ? skipTo.GetCurrentPath() : "";
            if (string.IsNullOrEmpty(name)) return "[" + actionName + "]" + (skipTo != null ? " > " + skipTo.GetCurrentPath() : "");
            return string.IsNullOrEmpty(actionName) ? name + (skipTo != null ? " > " + skipTo.GetCurrentPath() : "") : "";
        }

        public bool AtEnd()
        {
            ControlStep skipTo = this.skipTo;
            return skipTo != null ? skipTo.AtEnd() : children.Count == 0;
        }

        public string DisplayTree(int indent = 0)
        {
            string str1 = "";
            bool flag = true;

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(actionName))
            {
                str1 = str1 + new string(' ', indent * 2) + "- " + name + "\n" + new string(' ', (indent + 1) * 2) + "- True Action: " + actionName + "\n";

            }
            else if (!string.IsNullOrEmpty(name))
            {
                str1 = str1 + new string(' ', indent * 2) + "- " + name + "\n";
            }
            else if (!string.IsNullOrEmpty(actionName))
            {
                str1 = str1 + new string(' ', indent * 2) + "- Action: " + this.actionName + "\n";

                flag = false;
            }
            else
            {
                flag = false;
            }

            foreach (ControlStep child in children)
            {
                string str2 = child.DisplayTree(indent + (flag ? 1 : 0));
                if (!string.IsNullOrWhiteSpace(str2)) str1 += str2;
            }

            return str1;
        }

        // CLASSES
        public class Root
        {
            public ControlStep rootStep;

            public bool active;
            // Angle Turned (For root steps)
            public Vector3 angleTurned_Left;
            public Vector3 angleTurned_Right;
            // Last direction for each hand (For root steps)
            public Vector3 lastForward_Left;
            public Vector3 lastUp_Left;
            public Vector3 lastRight_Left;
            public Vector3 lastForward_Right;
            public Vector3 lastUp_Right;
            public Vector3 lastRight_Right;

            public Root(ControlStep root)
            {
                this.rootStep = root;
            }
        }


        protected class Condition
        {
            // VARIABLES
            protected Func<bool> start;
            protected Func<bool> end;
            protected float duration;
            protected bool hasStarted;
            protected float startTime;
            public bool runOnChange;
            public DurationMode mode;

            // FUNCTIONS
            public Condition(
                Func<bool> start, 
                Func<bool> end = null, 
                float duration = 0.0f,
                DurationMode mode = DurationMode.After,
                bool runOnChange = true)
            {
                this.start = start;
                this.end = end;
                this.duration = duration;
                this.mode = mode;
                this.runOnChange = runOnChange;
            }

            public Condition And(Func<bool> test)
            {
                Func<bool> old = start.Clone() as Func<bool>;
                start = (() =>
                {
                    Func<bool> func = old;
                    return (func != null ? (func() ? 1 : 0) : 1) != 0 && test();
                });
                return this;
            }

            public Condition AndNot(Func<bool> test)
            {
                Func<bool> old = start.Clone() as Func<bool>;
                start = (() =>
                {
                    Func<bool> func = old;
                    return (func != null ? (func() ? 1 : 0) : 1) != 0 && !test();
                });
                return this;
            }

            public Condition Or(Func<bool> test)
            {
                Func<bool> old = start.Clone() as Func<bool>;
                start = (() =>
                {
                    Func<bool> func = old;
                    return (func != null ? (func() ? 1 : 0) : 0) != 0 || test();
                });
                return this;
            }

            public bool Evaluate()
            {
                if (!hasStarted && start())
                {
                    hasStarted = true;
                    startTime = Time.time;
                }
                if (!hasStarted)
                    return false;
                if (duration == 0.0)
                {
                    Func<bool> end = this.end;
                    return end == null || end();
                }
                float num = Time.time - startTime;
                Func<bool> end1 = end;
                bool flag = end1 == null || end1();
                if (((mode != DurationMode.Before ? 0 : (num > duration ? 1 : 0)) & (flag ? 1 : 0)) == 0)
                    return (mode == DurationMode.After ? num >= duration : num <= duration) & flag;
                hasStarted = false;
                return false;
            }

            public void Reset() => hasStarted = false;
        }
    }
}
