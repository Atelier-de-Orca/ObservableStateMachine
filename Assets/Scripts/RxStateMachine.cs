using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;


public class RxStateMachine : MonoBehaviour
{
    [Header("Machine Infomation")]
    [SerializeField, ReadOnly]
    private RxState currentState = null;
    private Subject<RxState> currentStateSubject = new Subject<RxState>();
    public IObservable<RxState> stateStream => currentStateSubject.AsObservable();


    [Header("Models")]
    [SerializeField, Tooltip("RxState map for state transition control")]
    private List<StringRxStatePair> stateMap = new List<StringRxStatePair>();


    void Start() {

    }

    public void TriggerStateTransition(string key) {
        var targetStateKey = currentState.GetTransitionState(key);
        var targetState = FindStateByKey(targetStateKey);

        if (string.IsNullOrEmpty(targetStateKey) && targetState != null) {
            currentState = targetState;
            currentStateSubject.OnNext(currentState);
        }
    }

    private RxState FindStateByKey(string key) {
        RxState ret = null;

        foreach(var item in stateMap) {
            if (key.Equals(item.key)) {
                ret = item.value;
                break;
            }
        }

        return ret;
    }

}