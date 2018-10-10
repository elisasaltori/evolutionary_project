using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionController : MonoBehaviour {

    public int currGen = 1; //current generation
    public int currMaxSteps; //current number of steps
    public int beginSteps; //how many steps players begin with
    int inscreaseStepGens = 5; //how many gens between step increases
    int increaseSteps = 5; //number of steps to be increased

    int nSquares = 100;
    Vector3 spawnPos;

    public GameObject playerPrefab;

    GameObject startArea; //object representing starArea where squares will be spawned

    GameObject[] squares; //the players


    int lastGenBest; //index of the best square of last gen

   // Use this for initialization
   void Start () {

        lastGenBest = -1;
        startArea = GameObject.FindWithTag("StartArea");
        spawnPos = startArea.transform.position;
        currMaxSteps = beginSteps;
        //spawn first squares
        SpawnFirstGeneration();
	}

    void SpawnFirstGeneration()
    {
        //spawn all the squares
        for(int i=0; i<nSquares; i++)
            Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

	
	// Update is called once per frame
	void Update () {
        //spawn players
        //did everybody die or someone win?
            //reset level between gens
            //find best square and keep it (also change its color)
            //breed squares
            //reset them to initial position, steps etc

        //add more moves every x gens

    }
}
