using UnityEngine;

public class InputPlayerState : PlayerInputState
{
    public InputPlayerState(GameInput input)
    {
        this.input = input;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        input.Player.Enable();
        input.UI.Disable();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
