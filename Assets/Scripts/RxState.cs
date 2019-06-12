using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StringRxStatePair {
    public string key;
    public RxState value;
}

[Serializable]
public class StateTransitionTrigger {
    public string key;
    public string value;
}

public class RxState : MonoBehaviour {

    public List<StateTransitionTrigger> transitionTable;

    public string GetTransitionState(string key) {
        var ret = string.Empty;

        foreach(var item in transitionTable) {
            if (key.Equals(item.key)) {
                ret = item.value;
                break;
            }
        }

        return ret;
    }
}
