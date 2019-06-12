using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class TestPanelPresenter : MonoBehaviour {

    private static string FORMAT_STRING = "State Machine Info\nColor : {0}";
    public Text textPanel;

    public Button redButton;
    public Button greenButton;
    public Button blueButton;

    public ColorStateMachine machine;


    public void Start() {

        redButton.OnClickAsObservable()
            .Subscribe(e => {
                machine.TriggerStateTransition("To Red");
            });

        greenButton.OnClickAsObservable()
            .Subscribe(e => {
                machine.TriggerStateTransition("To Green");
            });

        blueButton.OnClickAsObservable()
            .Subscribe(e => {
                machine.TriggerStateTransition("To Blue");
            });

        machine.stateMachineAsObservable
            .Subscribe(e => {
                textPanel.text = string.Format(FORMAT_STRING, e.color.ToString());
            });
    }
}
