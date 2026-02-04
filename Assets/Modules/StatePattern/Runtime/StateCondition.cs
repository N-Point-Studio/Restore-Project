using System;

namespace Modules.StatePatterns {
    public class StateCondition
    {
        public Func<bool> Condition { get; }
        public bool HasCondition => Condition != null;

        public StateCondition(Func<bool> Condition)
        {
            this.Condition = Condition;
        }
    }
}