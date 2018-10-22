using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestSquareButton : MonoBehaviour {

    bool loadText = true;
    Text text;
    EvolutionController ec;

	// Use this for initialization
	void Start () {
        text = transform.GetChild(0).GetComponent<Text>();
        ec = GameObject.Find("EvolutionController").GetComponent<EvolutionController>();
	}
	
    public void LoadUnloadSquare()
    {
        if (loadText)
        {
            text.text = "Resume";
            ec.LoadBestSquare();
            loadText = false;
        }
        else
        {
            text.text = "Load Square";
            ec.ResumeEvolution();
            loadText = true;
        }
    }
}
