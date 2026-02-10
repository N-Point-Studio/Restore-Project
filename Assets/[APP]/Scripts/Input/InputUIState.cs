using UnityEngine;

public class InputUIState : PlayerInputState
{
    public InputUIState(GameInput input)
    {
        this.input = input;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        input.Player.Disable();
        input.UI.Enable();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
