using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public bool playerControlled = true;
    
    public float speed = 3f;
    
    public List<Vector3> movements;
    Rigidbody2D rb;
    Vector3 initialPos;
    int currStep;
    int maxSteps;
    float mutationRate;
    EvolutionController ec;
    
    System.Random rand; // random number generator used for getting new directions

    float movementDelay = 0.06f; //delay algorithm movement, but make steps larger: less steps for algorithm to learn
    float currDelay;
    public bool won = false;
    bool deathByEnemy = false;
    GoalMarker[] goalMarkers;
    bool[] reachedMarker; //if a particular marker has been reached, true. Same order and size as goalmarkers

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        rand = new System.Random(Guid.NewGuid().GetHashCode());
        initialPos = transform.position;
        

        ec = GameObject.Find("EvolutionController").GetComponent<EvolutionController>();

        //initialize square with ec data

        maxSteps = ec.GetCurrMaxSteps();
        mutationRate = ec.GetMutationRate();

        currStep = 0;
        currDelay = 0;

        //get initial list of movements
        InitializeMovements();
        

    }
	
	// Update is called once per frame
	void FixedUpdate () {
        if (playerControlled)
            MoveInput();
        else
        {
            currDelay += Time.deltaTime;
            if(currDelay> movementDelay)
            {
                currDelay = 0;
                MoveVector();
            }
            
        }
            
	}

    /// <summary>
    /// Initialize directions vector for algorithm controlled player
    /// </summary>
    void InitializeMovements()
    {
        movements = new List<Vector3>();

        for(int i=0; i<maxSteps; i++)
        {
            movements.Add(GetRandomDirection());
        }
    }

    /// <summary>
    /// Get vector in one of 8 possible directions:
    /// up, down, right, left, northeast, southeast, northwest, southwest
    /// </summary>
    /// <returns>Vector3 with direction</returns>
    Vector3 GetRandomDirection()
    {

        
        int dir = rand.Next(8);

        switch (dir)
        {
            case 0:
                //up
                return Vector3.up;
            case 1:
                //down
                return Vector3.down;
            case 2:
                //left
                return Vector3.left;
            case 3:
                //right
                return Vector3.right;
            case 4:
                //northeast
                return new Vector3(1, 1, 0);
            case 5:
                //southeast
                return new Vector3(1, -1, 0);
            case 6:
                //northwest
                return new Vector3(-1, 1, 0);
            case 7:
                //southwest
                return new Vector3(-1, -1, 0);
        }
        return Vector3.zero;
    }

    //Movement for evolutionary algorithm
    void MoveVector()
    {
        if(currStep < maxSteps)
        {
            rb.MovePosition(transform.position + 3*speed * movements[currStep].normalized * Time.deltaTime);
            currStep++;
        }
        else
        {
            gameObject.SetActive(false);
        }
       
    }

    //Movement for human players
    void MoveInput()
    {
        var v3 = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0.0f);
        rb.MovePosition(transform.position + speed * v3.normalized * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //collided with enemy -> dies
        if (collision.gameObject.tag == "Enemy")
        {
            //if player, just send it back to the start
            if(playerControlled)
                transform.position = initialPos;
            else
            {
                //killed by an enemy
                deathByEnemy = true;
                gameObject.SetActive(false); //turn off player
            }

        }
        //collided with something else
        else
        {
            if(collision.gameObject.tag == "GoalMarker")
            {
                SetGoalMarkerAsVisited(collision.transform.position);
            }

            //is it the end of the level?
            if (collision.gameObject.tag == "Goal")
            {
                won = true;
                gameObject.SetActive(false);
                print("I win!");
            }

        }

    }

    void SetGoalMarkerAsVisited(Vector3 pos)
    {
        for(int i=0; i<goalMarkers.Length; i++)
        {
            if(goalMarkers[i].transform.position == pos)
            {
                reachedMarker[i] = true;
                break;
            }
        }
    }

    /// <summary>
    /// Resets player to initial settings
    /// Used when starting a new generation
    /// </summary>
    public void ResetPlayer()
    {
        this.transform.position = initialPos;

        //initialize square with ec data
        maxSteps = ec.GetCurrMaxSteps();

        currStep = 0;
        currDelay = 0;

        deathByEnemy = false;
        ResetMarkers();
        
        won = false;
        gameObject.SetActive(true);
    }

    void ResetMarkers()
    {
        for(int i=0; i<reachedMarker.Length; i++)
        {
            reachedMarker[i] = false;
        }
    }

    /// <summary>
    /// Increase number of movements of square. New movements are randomized.
    /// </summary>
    /// <param name="numberOfMovements">number of movements to be added</param> 
    public void IncreaseMovements(int numberOfMovements)
    {
        for(int i=0; i< numberOfMovements; i++)
        {
            movements.Add(GetRandomDirection());
        }
    }

    /// <summary>
    /// Used for initializing goalMarkers and reachedMarker
    /// Called by EvolutionController
    /// </summary>
    /// <param name="goalMarkers"></param>
    public void SetGoalMarkers(GoalMarker[] goalMarkers)
    {
        this.goalMarkers = goalMarkers;
        reachedMarker = new bool[goalMarkers.Length];
    }


    public float GetFitnessScore()
    {
        //if square has won, score is based on the number of steps
        //should be higher than if square hasn't reached goal
        if (won)
        {
            return (1.0f / 16.0f + 10000.0f / (currStep * currStep));
        }
        else
        {
            //square still hasn't reached goal
            float estimatedDistance = 0;

            //go through markers to get last visited marker
            for(int i = 0; i< reachedMarker.Length; i++)
            {
                //found first not reached marker
                if(reachedMarker[i]== false)
                {
                    
                    //distance from marker to goal
                    estimatedDistance = goalMarkers[i].distanceToGoal;
                    //distance from player to marker
                    estimatedDistance += Vector3.Distance(goalMarkers[i].getPosition(), transform.position);
                    break;
                }
            }

            //give bonus to those who wore on their way and got killed by ball
            if (deathByEnemy)
                estimatedDistance *= 0.9f;

            float fitness = (1.0f / (estimatedDistance * estimatedDistance));
            return (fitness*fitness);
        }
    }

    /// <summary>
    /// Mutate movements
    /// </summary>
    public void Mutate()
    {
        
        for(int i=0; i< movements.Count; i++)
        {
            float value;

            //greater chance to mutating closer to death
            if (i > currStep - 10)
                value = rand.Next(201)/1000.0f;
            else
                value = rand.Next(1001) / 1000.0f;


            if(value < mutationRate)
            {
                movements[i] = GetRandomDirection();
            }
        }

    }

    /// <summary>
    /// Get deep copy of movements
    /// </summary>
    /// <returns>Copy of movements</returns>
    public List<Vector3> CloneMovements()
    {
        List<Vector3> clone = new List<Vector3>();

        for(int i=0; i<movements.Count; i++)
        {
            clone.Add(new Vector3(movements[i].x, movements[i].y, movements[i].z));
        }

        return clone;
    }
}
