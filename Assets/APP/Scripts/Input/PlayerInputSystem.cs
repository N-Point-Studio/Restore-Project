using Modules.StatePatterns;
using System;
using VContainer.Unity;

public class PlayerInputSystem : IInitializable, IStartable, ITickable, IDisposable
{
    protected GameInput input = new GameInput();
    /// <summary>Underlying input actions instance.</summary>
    public GameInput Input => input;

    protected Modules.StatePatterns.StateMachine stateMachine;

    protected InputStateType state;
    /// <summary>Current logical input mode (updated via <see cref="ChangeInputState"/>).</summary>
    public InputStateType State => state;

    void IInitializable.Initialize()
    {
        // State Initialization
        stateMachine = new Modules.StatePatterns.StateMachine("input")
            .AddState(new InputUIState(input))
            .AddState(new InputPlayerState(input))
            .AddState(new InputDisabledState(input))
            .AddTransition(typeof(InputUIState), () => State == InputStateType.UI)
            .AddTransition(typeof(InputPlayerState), () => State == InputStateType.Player)
            .AddTransition(typeof(InputDisabledState), () => State == InputStateType.Disabled);
    }

    void IStartable.Start()
    {
        stateMachine.SetState(typeof(InputUIState));
    }

    void ITickable.Tick()
    {
        if (stateMachine == null || stateMachine.Current == null) return;

        stateMachine.Current.Tick();

        StateTransition transition = stateMachine.GetTransition();
        if (transition != null)
        {
            stateMachine.SetState(transition.To);
        }
    }

    void IDisposable.Dispose()
    {
        stateMachine.Reset();
    }

    /// <summary>Fires a named trigger on the internal state machine.</summary>
    public void SetTrigger(string trigger)
    {
        stateMachine.Trigger(trigger);
    }

    /// <summary>
    /// Changes the active input mode and raises global <c>InputEvents.OnInputStateChanged</c>.
    /// </summary>
    public void ChangeInputState(InputStateType state, bool hideUIOnInputDisabled = true)
    {
        this.state = state;
        InputEvents.OnInputStateChanged?.Invoke(state);
    }
}

public enum InputStateType
{
    UI,
    Player,
    Disabled
}