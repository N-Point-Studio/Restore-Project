using System;
using System.Collections.Generic;

namespace Modules.StatePatterns
{
    public class StateMachine
    {
        #region Fields & Properties

        protected string stateMachineName;
        /// <summary>
        /// Name of this StateMachine
        /// </summary>
        public string Name => stateMachineName;

        protected bool autoStartFirstState;
        /// <summary>
        /// If true then the first state will be auto started
        /// </summary>
        internal bool AutoStartFirstState => autoStartFirstState;

        protected IState currentState;
        /// <summary>
        /// Currently active state
        /// </summary>
        public IState Current => currentState;

        protected Dictionary<Type, IState> listOfState;
        /// <summary>
        /// List of all registered states
        /// </summary>
        internal Dictionary<Type, IState> ListOfState => listOfState;

        protected Dictionary<Type, List<StateTransition>> listOfTransition;
        protected List<StateTransition> listOfAnyTransition;
        protected Action<Type, Type> OnChangeState;
        protected Action<Type> OnEnterState;
        protected Action<Type> OnExitState;

        #endregion

        #region Class Building

        /// <summary>
        /// Create a new StateMachine
        /// </summary>
        /// <param name="stateMachineName">Name to identify this StateMachine</param>
        /// <param name="autoStartFirstState">If true then the first state of <see cref="AddState(IState)"/> will be auto started</param>
        public StateMachine(string stateMachineName, bool autoStartFirstState = true)
        {
            this.stateMachineName = stateMachineName;
            this.autoStartFirstState = autoStartFirstState;

            listOfState = new Dictionary<Type, IState>();
            listOfTransition = new Dictionary<Type, List<StateTransition>>();
            listOfAnyTransition = new List<StateTransition>();
        }

        /// <summary>
        /// Add a new state for this StateMachine
        /// </summary>
        /// <param name="state">A new state instance derived from <see cref="IState"/> class</param>
        /// <returns></returns>
        public StateMachine AddState(IState state)
        {
            Type type = state.GetType();

            if (!listOfState.ContainsKey(type))
            {
                listOfState.Add(type, state);
            }
            else
            {
                listOfState[type] = state;
            }

            return this;
        }

        /// <summary>
        /// Create a new state-to-state transition that will be executed when Trigger(<paramref name="trigger"/>) is called and <paramref name="condition"/> returns true
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <param name="trigger">Trigger message</param>
        /// <param name="condition">Condition parameter</param>
        /// <returns></returns>
        public StateMachine AddTransition(Type from, Type to, string trigger, Func<bool> condition)
        {
            if (!listOfTransition.ContainsKey(from))
            {
                listOfTransition.Add(from, new List<StateTransition>());
            }

            listOfTransition[from].Add(new StateTransition(from, to).SetTrigger(trigger).SetCondition(condition));
            return this;
        }

        /// <summary>
        /// Create a new state-to-state transition that will be executed when <paramref name="condition"/> returns true
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <param name="condition">Condition parameter</param>
        /// <returns></returns>
        public StateMachine AddTransition(Type from, Type to, Func<bool> condition)
        {
            if (!listOfTransition.ContainsKey(from))
            {
                listOfTransition.Add(from, new List<StateTransition>());
            }

            listOfTransition[from].Add(new StateTransition(from, to).SetCondition(condition));
            return this;
        }

        /// <summary>
        /// Create a new state-to-state transition that will be executed when Trigger(<paramref name="trigger"/>) is called
        /// </summary>
        /// <param name="from">Source state</param>
        /// <param name="to">Target state</param>
        /// <param name="trigger">Trigger message</param>
        /// <returns></returns>
        public StateMachine AddTransition(Type from, Type to, string trigger)
        {
            if (!listOfTransition.ContainsKey(from))
            {
                listOfTransition.Add(from, new List<StateTransition>());
            }

            listOfTransition[from].Add(new StateTransition(from, to).SetTrigger(trigger));
            return this;
        }

        /// <summary>
        /// Create a new any-state transition that will be executed when Trigger(<paramref name="trigger"/>) is called and <paramref name="condition"/> returns true
        /// </summary>
        /// <param name="to">Target state</param>
        /// <param name="trigger">Trigger message</param>
        /// <param name="condition">Condition parameter</param>
        /// <returns></returns>
        public StateMachine AddTransition(Type to, string trigger, Func<bool> condition)
        {
            listOfAnyTransition.Add(new StateTransition(null, to).SetTrigger(trigger).SetCondition(condition));
            return this;
        }

        /// <summary>
        /// Create a new any-state transition that will be executed when <paramref name="condition"/> returns true
        /// </summary>
        /// <param name="to">Target state</param>
        /// <param name="condition">Condition parameter</param>
        /// <returns></returns>
        public StateMachine AddTransition(Type to, Func<bool> condition)
        {
            listOfAnyTransition.Add(new StateTransition(null, to).SetCondition(condition));
            return this;
        }

        /// <summary>
        /// Create a new any-state transition that will be executed when Trigger(<paramref name="trigger"/>) is called
        /// </summary>
        /// <param name="to">Target state</param>
        /// <param name="trigger">Trigger message</param>
        /// <returns></returns>
        public StateMachine AddTransition(Type to, string trigger)
        {
            listOfAnyTransition.Add(new StateTransition(null, to).SetTrigger(trigger));
            return this;
        }

        /// <summary>
        /// Hook an event that will be called when state changed
        /// </summary>
        /// <param name="action">Callback function containing <c>from</c> state and <c>to</c> state</param>
        /// <returns></returns>
        public StateMachine OnChange(Action<Type, Type> action)
        {
            OnChangeState = action;
            return this;
        }

        /// <summary>
        /// Hook an event that will be called when entering a state
        /// </summary>
        /// <param name="action">Callback function containing entered state</param>
        /// <returns></returns>
        public StateMachine OnEnter(Action<Type> action)
        {
            OnEnterState = action;
            return this;
        }

        /// <summary>
        /// Hook an event that will be called when exit a state
        /// </summary>
        /// <param name="action">Callback function containing exited state</param>
        /// <returns></returns>
        public StateMachine OnExit(Action<Type> action)
        {
            OnExitState = action;
            return this;
        }

        #endregion

        #region Gameplay

        public T GetState<T>() where T : IState
        {
            return listOfState[typeof(T)] as T;
        }

        public void SetState(IState state)
        {
            SetState(state.GetType());
        }

        public void SetState(Type type)
        {
            if (!listOfState.ContainsKey(type))
            {
                throw new MissingMemberException($"State with type {type} not found. Are you forget to AddState?");
            }

            if (currentState?.GetType() == type)
            {
                return;
            }

            if (currentState != null)
            {
                currentState.OnExit();
                OnExitState?.Invoke(currentState.GetType());
                OnChangeState?.Invoke(currentState?.GetType(), type);
            }
            
            currentState = listOfState[type];
            currentState.OnEnter();
            OnEnterState?.Invoke(type);
        }

        public StateTransition GetTransition()
        {
            if (listOfTransition.ContainsKey(currentState.GetType()))
            {
                var transitions = listOfTransition[currentState.GetType()];

                foreach (var transition in transitions)
                {
                    if (IsValidTransition(transition, string.Empty))
                    {
                        return transition;
                    }
                }
            }

            foreach (var transition in listOfAnyTransition)
            {
                if (IsValidTransition(transition, string.Empty))
                {
                    return transition;
                }
            }

            return null;
        }

        /// <summary>
        /// Send trigger message
        /// </summary>
        /// <param name="trigger"></param>
        public void Trigger(string trigger)
        {
            if (listOfTransition.ContainsKey(currentState.GetType()))
            {
                foreach (var transition in listOfTransition[currentState.GetType()])
                {
                    if (IsValidTransition(transition, trigger))
                    {
                        SetState(transition.To);
                    }
                }
            }

            foreach (var transition in listOfAnyTransition)
            {
                if (IsValidTransition(transition, trigger))
                {
                    SetState(transition.To);
                }
            }
        }

        private bool IsValidTransition(StateTransition transition, string trigger)
        {
            if (transition.To != currentState.GetType())
            {
                if (transition.HasCondition && transition.HasTrigger)
                {
                    return transition.IsConditionMet() && transition.Trigger == trigger;
                }
                else if (transition.HasCondition)
                {
                    return transition.IsConditionMet();
                }
                else if (transition.HasTrigger)
                {
                    return transition.Trigger == trigger;
                }
            }

            return false;
        }

        public void Reset()
        {
            listOfState.Clear();
            listOfTransition.Clear();
            listOfAnyTransition.Clear();
        }

        #endregion
    }
}