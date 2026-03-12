using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected State currentState;

    public void SwitchState(State newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    private void FixedUpdate()
    {
        currentState?.Tick(Time.deltaTime);
    }
}

public abstract class PartStateMachine : MonoBehaviour
{
    protected State currentState;
    public Vector3 InitialPosition;
    public Quaternion InitialRotation;
    public void SwitchState(State newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    private void FixedUpdate()
    {
        currentState?.Tick(Time.deltaTime);
    }

    public void ResetTransform()
    {
        transform.SetPositionAndRotation(InitialPosition, InitialRotation);
    }
}