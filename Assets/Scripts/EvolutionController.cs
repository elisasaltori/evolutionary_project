using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionController : MonoBehaviour {

    int currGen = 1; //current generation
    int currMaxSteps; //current number of steps
    int beginSteps = 10; //how many steps players begin with
    int inscreaseStepGens = 5; //how many gens between step increases
    int increaseSteps = 5; //number of steps to be increased
    float mutationRate = 0.1f;

    int nSquares = 100;
    float fitnessSum; //used for selecting parents
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
        spawnPos.z = spawnPos.z - 0.1f; //just to put it above level so the picture shows up
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
        squares = new GameObject[nSquares];
        //spawn all the squares
        for(int i=0; i<nSquares; i++)
        {
            //spawn square
            squares[i] = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            //set goal markers
            squares[i].GetComponent<PlayerController>().SetGoalMarkers(goalMarkers);
        }
            
    
    }

	
	// Update is called once per frame
	void Update () {

        //did everybody die or win?
        if (AllPlayersDead())
        {
            //reset level between gens
            ResetEnemies();

            //set best square of last generation back to original color
            SetSquareToNormalColor();

            //find best square and keep it (also change its color)
            FindBestSquare();

            //breed squares
            NaturalSelection();

            //increase generation number
            currGen++;

            //add more moves every x gens
            if (currGen % inscreaseStepGens == 0)
                IncreasePlayerSteps();


            //spawn players
            //reset them to initial position, steps etc
            ResetPlayers();

      
        }

    }

    void IncreasePlayerSteps()
    {
        PlayerController aux;

        currMaxSteps += increaseSteps;

        for(int i=0; i<nSquares; i++)
        {
            aux = squares[i].GetComponent<PlayerController>();
            aux.IncreaseMovements(increaseSteps);
        }
    }
    
    /// <summary>
    /// Reset all the squares:
    ///     -sets them as active
    ///     -bring them back to original position
    ///     -reset number of steps
    /// </summary>
    void ResetPlayers()
    {
        PlayerController aux;
        for(int i=0; i<nSquares; i++)
        {
            aux = squares[i].GetComponent<PlayerController>();
            aux.ResetPlayer();
        }
    }

    /// <summary>
    /// Get the next generation of squares
    /// </summary>
    void NaturalSelection()
    {
        int parent;
        PlayerController aux, auxParent;

        //fitness sum is used for selecting parents
        GetFitnessSum();

        for(int i=0; i<nSquares; i++)
        {
            //dont change the best of the generation
            if(i != lastGenBest)
            {

                aux = squares[i].GetComponent<PlayerController>();

                //crossover
                parent = SelectParent();
                auxParent = squares[parent].GetComponent<PlayerController>();

                //get movements
                List<Vector3> movements = auxParent.CloneMovements();
                aux.movements = movements;

                //mutation
                aux.Mutate();
            }
        }
        
        
    }

    void GetFitnessSum()
    {
        PlayerController aux;

        fitnessSum = 0;

        for (int i = 0; i < nSquares; i++)
        {
            aux = squares[i].GetComponent<PlayerController>();
            fitnessSum += aux.GetFitnessScore();
        }
    }

    /// <summary>
    /// Get parent, giving a better chance to those with greater fitness
    /// </summary>
    /// <returns>Index of selected parent</returns>
    int SelectParent()
    {
        float score = Random.Range(0, fitnessSum);
        PlayerController aux;
        float sum = 0;

        for (int i = 0; i < nSquares; i++)
        {
            aux = squares[i].GetComponent<PlayerController>();
            sum += aux.GetFitnessScore();
            if (sum > score)
                return i;
        }

        //unreachable
        return 0;

    }


    /// <summary>
    /// Find square with highest fitness score.
    /// Save index to lastGenBest.
    /// Color best square green.
    /// </summary>
    void FindBestSquare()
    {
        PlayerController aux;
        float maxScore = -1;
        float score;

        for(int i=0; i< nSquares; i++)
        {
            aux = squares[i].GetComponent<PlayerController>();
            score = aux.GetFitnessScore();

            if(score > maxScore)
            {
                lastGenBest = i;
                maxScore = score;
            }
        }

        squares[lastGenBest].GetComponent<SpriteRenderer>().color = Color.green;

    }

    /// <summary>
    /// Set square to normal color.
    /// Used to make the square of last gen back into normal color
    /// </summary>
    void SetSquareToNormalColor()
    {
        if(lastGenBest>=0 && lastGenBest < nSquares)
        {
            SpriteRenderer aux = squares[lastGenBest].GetComponent<SpriteRenderer>();
            aux.color = Color.white;
        }
    }

    /// <summary>
    /// Find all enemies in level and reset it
    /// </summary>
    void ResetEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        EnemyController aux;

        for (int i = 0; i < enemies.Length; i++)
        {
            aux = enemies[i].GetComponent<EnemyController>();
            aux.ResetEnemy();
        }
    }

    /// <summary>
    /// Return true if all players are dead or have completed the course
    /// </summary>
    /// <returns></returns>
    bool AllPlayersDead()
    {
        //check if any player is active
        //if so, return false
        for(int i=0; i<nSquares; i++)
        {
            if (squares[i].activeSelf)
                return false;
        }

        //all players inactive
        return true;
    }

    public int GetCurrMaxSteps()
    {
        return currMaxSteps;
    }
    
    public int GetCurrGen()
    {
        return currGen;
    }

    public float GetMutationRate()
    {
        return mutationRate;
    }
}
