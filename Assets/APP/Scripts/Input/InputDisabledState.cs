using UnityEngine;

public class InputDisabledState : PlayerInputState
{
    public InputDisabledState(GameInput input)
    {
        this.input = input;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        input.Player.Disable();
        input.UI.Disable();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}