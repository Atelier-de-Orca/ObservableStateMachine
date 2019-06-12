using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class ObservableStateMachine<T> : MonoBehaviour where T : StateModel
{
    [Header("Machine Infomation")]
    [SerializeField, ReadOnly]
    private T currentState = null;
    private Subject<T> currentStateSubject = new Subject<T>();
    public IObservable<T> stateMachineAsObservable => currentStateSubject.AsObservable();


    [Header("Models")]

    public string initStateKey;
    [SerializeField, Tooltip("RxState map for state transition control")]
    private List<StringStateModelPair> stateMap = new List<StringStateModelPair>();

    public void Start() {

        currentState = FindStateByKey(initStateKey);
        currentStateSubject.OnNext(currentState);
    }

    public void TriggerStateTransition(string key) {
        var targetStateKey = currentState.GetTransitionState(key);
        var targetState = FindStateByKey(targetStateKey);

        if (!string.IsNullOrEmpty(targetStateKey) && targetState != null) {
            currentState = targetState;
            currentStateSubject.OnNext(currentState);
        }
    }

    private T FindStateByKey(string key) {
        T ret = null;

        foreach(var item in stateMap) {
            if (key.Equals(item.key)) {
                ret = item.value as T;
                break;
            }
        }

        return ret;
    }

}