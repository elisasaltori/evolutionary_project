using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenText : MonoBehaviour {

    EvolutionController ec;
    Text genText;

	// Use this for initialization
	void Start () {
        ec = GameObject.Find("EvolutionController").GetComponent<EvolutionController>();
        genText = this.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        genText.text = "Gen: " + ec.currGen;
	}
}
