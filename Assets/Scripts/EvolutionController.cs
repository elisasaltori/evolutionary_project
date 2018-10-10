using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionController : MonoBehaviour {

    int currGen = 1; //current generation
    int currMaxSteps; //current number of steps
    int beginSteps = 10; //how many steps players begin with
    int inscreaseStepGens = 5; //how many gens between step increases
    int increaseSteps = 5; //number of steps to be increased

    int nSquares = 100;
    Vector3 spawnPos;

    public GameObject playerPrefab;

    GameObject startArea; //object representing starArea where squares will be spawned

    GameObject[] squares; //the players
    GoalMarker[] goalMarkers; //goal markers: secondary and final objectives on screen (marked as a grey square)


    int lastGenBest; //index of the best square of last gen

   // Use this for initialization
   void Start () {

        lastGenBest = -1;
        startArea = GameObject.FindWithTag("StartArea");
        spawnPos = startArea.transform.position;
        spawnPos.z = spawnPos.z - 0.1f;
        currMaxSteps = beginSteps;
        
  
        InitializeGoalMarkers();
        //spawn first squares
        SpawnFirstGeneration();
	}

    /// <summary>
    /// Get goal markers to calculate distance to the finish goal, which is used for fitness function
    /// 
    /// Get all GoalMarker objects
    /// Order is given by their order as children of the object GoalMarkers
    /// </summary>
    void InitializeGoalMarkers()
    {
        GameObject goalMarkersParent = GameObject.Find("GoalMarkers");

        int n = goalMarkersParent.transform.childCount;

        goalMarkers = new GoalMarker[n];

        //last one
        goalMarkers[n - 1] = goalMarkersParent.transform.GetChild(n - 1).GetComponent<GoalMarker>();

        goalMarkers[n - 1].SetDistanceToGoal(null);

        //get each child, backwards
        //calculating accumulated distances
        for (int i=n-2; i >=0; i--)
        {
            
            goalMarkers[i] = goalMarkersParent.transform.GetChild(i).GetComponent<GoalMarker>();
            goalMarkers[i].SetDistanceToGoal(goalMarkers[i+1]);

        }
    }

    void SpawnFirstGeneration()
    {
        GameObject obj;
        //spawn all the squares
        for(int i=0; i<nSquares; i++)
        {
            //spawn square
            obj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            //set goal markers
            obj.GetComponent<PlayerController>().SetGoalMarkers(goalMarkers);
        }
            
    
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

    public int GetCurrMaxSteps()
    {
        return currMaxSteps;
    }
    
    public int GetCurrGen()
    {
        return currGen;
    }
}
