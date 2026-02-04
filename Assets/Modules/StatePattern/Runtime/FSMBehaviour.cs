using System;
using System.Linq;
using UnityEngine;

namespace Modules.StatePatterns
{
    public abstract class FSMBehaviour : MonoBehaviour
    {
        #region Fields & Properties

        private StateMachine stateMachine;
        protected StateMachine FSM => stateMachine;

        #endregion

        #region Unity Callback

        protected abstract StateMachine InitStateMachine();

        protected virtual void Awake()
        {
            stateMachine = InitStateMachine();
        }

        protected virtual void Start()
        {
            if (FSM.AutoStartFirstState)
            {
                FSM.SetState(FSM.ListOfState.First().Value);
            }
        }

        protected virtual void Update()
        {
            Tick();
        }

        protected virtual void FixedUpdate()
        {
            FixedTick();
        }

        protected virtual void LateUpdate()
        {
            LateTick();
        }

        #endregion

        #region Gameplay

        protected virtual void Tick()
        {
            CheckStateMachine();

            FSM.Current.Tick();

            var transition = FSM.GetTransition();
            if (transition != null)
            {
                SetState(transition.To);
            }
        }

        protected virtual void FixedTick()
        {
            CheckStateMachine();

            FSM.Current.FixedTick();
        }

        protected virtual void LateTick()
        {
            CheckStateMachine();

            FSM.Current.LateTick();
        }

        protected void SetState(IState state)
        {
            SetState(state.GetType());
        }

        protected void SetState(Type type)
        {
            FSM.SetState(type);
        }

        protected void CheckStateMachine()
        {
            if (FSM == null)
                throw new NullReferenceException("StateMachine must not be null.");

            if(FSM.Current == null)
                throw new NullReferenceException("There is no active state.");
        }

        #endregion
    }
}