using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class AnalogStickRotation : IInputInteraction<Vector2>
{
    public float duration = .2f;

    public void Process(ref InputInteractionContext context)
    {
        
        
        switch (context.phase)
        {
            case InputActionPhase.Waiting:
                
                
                break;
            case InputActionPhase.Started:
                Debug.Log("Started");
                break;
            default:
                break;
        }
        context.SetTotalTimeoutCompletionTime(3 * duration);
        context.SetTimeout(2);
    }

    static AnalogStickRotation()
    {
        InputSystem.RegisterInteraction<AnalogStickRotation>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {

    }

    public void Reset()
    {

    }
}
