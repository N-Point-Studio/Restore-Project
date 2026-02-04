using System;

namespace Modules.StatePatterns
{
    public class StateTransition
    {
        protected Type from;
        public Type From => from;

        private Type to;
        public Type To => to;

        private StateCondition condition;
        public StateCondition Condition => condition;
        public bool HasCondition => condition != null;

        private string trigger;
        public string Trigger => trigger;
        public bool HasTrigger => !string.IsNullOrEmpty(trigger);

        public StateTransition(Type from, Type to)
        {
            this.from = from;
            this.to = to;
        }

        public StateTransition SetCondition(Func<bool> condition)
        {
            this.condition = new StateCondition(condition);
            return this;
        }

        public StateTransition SetTrigger(string trigger)
        {
            this.trigger = trigger;
            return this;
        }

        public bool IsConditionMet()
        {
            return condition != null && condition.Condition();
        }
    }
}