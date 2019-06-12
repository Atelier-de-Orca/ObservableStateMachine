using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class StringStateModelPair {
    public string key;
    public StateModel value;
}

[Serializable]
public class StateTransitionTrigger {
    public string key;
    public string value;
}

public class StateModel : MonoBehaviour {

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
