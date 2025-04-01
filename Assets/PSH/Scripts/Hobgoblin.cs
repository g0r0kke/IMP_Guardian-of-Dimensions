using System.Collections.Generic;
using UnityEngine;

public abstract class Hobgoblin : MonoBehaviour
{
    protected IState currentState;
    protected Dictionary<System.Type, IState> states = new Dictionary<System.Type, IState>();

    [SerializeField] public float idel;


}
