using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepsText : MonoBehaviour {

    EvolutionController ec;
    Text stepsText;

    // Use this for initialization
    void Start()
    {
        ec = GameObject.Find("EvolutionController").GetComponent<EvolutionController>();
        stepsText = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        stepsText.text = "Steps: " + ec.GetCurrMaxSteps();
    }
}
