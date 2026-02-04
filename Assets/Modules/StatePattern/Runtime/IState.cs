namespace Modules.StatePatterns
{
    public abstract class IState
    {
        /// <summary>
        /// Tick is called every Update()
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// FixedTick is called every FixedUpdate()
        /// </summary>
        public virtual void FixedTick()
        {

        }

        /// <summary>
        /// LateTick is called every LateUpdate()
        /// </summary>
        public virtual void LateTick()
        {

        }

        /// <summary>
        /// OnEnter is called when this state is started
        /// </summary>
        public abstract void OnEnter();

        /// <summary>
        /// OnExit is called when this state is ended
        /// </summary>
        public abstract void OnExit();
    }
}