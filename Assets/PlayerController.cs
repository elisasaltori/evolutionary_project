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
    int nInitialSteps;
    EvolutionController ec;
    
    System.Random rand; // random number generator used for getting new directions

    float movementDelay = 0.06f; //delay algorithm movement, but make steps larger: less steps for algorithm to learn
    float currDelay;
    bool won = false;
    bool deathByEnemy = false;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        rand = new System.Random(Guid.NewGuid().GetHashCode());
        initialPos = transform.position;
        

        ec = GameObject.Find("EvolutionController").GetComponent<EvolutionController>();

        //initialize square with ec data
        nInitialSteps = ec.beginSteps;
        maxSteps = ec.currMaxSteps;
        print("max:" + maxSteps);
        print("intial:" + nInitialSteps);

        currStep = 0;
        currDelay = 0;

        //get initial list of movements
        InitializeMovements();
        

    }
	
	// Update is called once per frame
	void Update () {
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

        for(int i=0; i<nInitialSteps; i++)
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

    //Movement for 
    void MoveVector()
    {
        if(currStep < maxSteps)
        {
            rb.MovePosition(transform.position + 3*speed * movements[currStep].normalized * Time.deltaTime);
            currStep++;
        }
        else
        {
            print("deactivated");
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
            //is it the end of the level?
            if (collision.gameObject.tag == "Goal")
            {
                won = true;
                print("I win!");
            }

        }

    }

    public void ResetPlayer()
    {
        this.transform.position = initialPos;

        //initialize square with ec data
        nInitialSteps = ec.beginSteps;
        maxSteps = ec.currMaxSteps;

        currStep = 0;
        currDelay = 0;

        deathByEnemy = false;
        
        won = false;
        gameObject.SetActive(true);
    }

}
