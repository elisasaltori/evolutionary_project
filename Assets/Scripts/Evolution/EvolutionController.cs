using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EvolutionController : MonoBehaviour {

    int currGen = 1; //current generation
    int currMaxSteps; //current number of steps
    int beginSteps; //how many steps players begin with
    int inscreaseStepGens; //how many gens between step increases
    int increaseSteps; //number of steps to be increased
    float mutationRate;
    bool breedWithBestOn;
    bool naturalSelectionOn;

    int nSquares;
    float fitnessSum; //used for selecting parents
    Vector3 spawnPos;
    List<Vector3> bestMovements;
    public GameObject playerPrefab;

    bool levelComplete = false;
    int completedAtGen = -1;
    int bestSquareFromGen = -1;
    int bestSteps;

    //backup for when loading best square from file
    int completedAtGenAux = -1;
    int bestSquareFromGenAux = -1;
    int bestStepsAux;
    int currMaxStepsAux;

    bool evolutionPaused = false;
    GameObject bestSquare;

    //used for saving information about the game
    float[] lastBestFitness;
    float[] lastAverageFitness;
    int[] lastGens;

    GameObject startArea; //object representing starArea where squares will be spawned

    GameObject[] squares; //the players
    GoalMarker[] goalMarkers; //goal markers: secondary and final objectives on screen (marked as a grey square)

    GameObject wonPanel;


    int lastGenBest; //index of the best square of last gen
    private float maxScore;

    // Use this for initialization
    void Start () {


        GetSettings();

        lastGenBest = -1;
        startArea = GameObject.FindWithTag("StartArea");
        //panel that shows statistics about winning
        wonPanel = GameObject.Find("WonPanel");
        wonPanel.SetActive(false);

        spawnPos = startArea.transform.position;
        spawnPos.z = spawnPos.z - 0.1f; //just to put it above level so the picture shows up
        currMaxSteps = beginSteps;


        //used for saving in file
        //delete old file
        SaveScores.deleteCSV();
        lastBestFitness = new float[10];
        lastAverageFitness = new float[10];
        lastGens = new int[10];

        InitializeGoalMarkers();
        //spawn first squares
        SpawnFirstGeneration();


	}

    /// <summary>
    /// Get evolution settings set at menu
    /// </summary>
    void GetSettings()
    {
        beginSteps = EvolutionOptions.beginSteps;
        inscreaseStepGens = EvolutionOptions.increaseStepsGens; //how many gens between step increases
        increaseSteps = EvolutionOptions.increaseSteps; //number of steps to be increased
        mutationRate = EvolutionOptions.mutationRate;
        nSquares = EvolutionOptions.nSquares;
        naturalSelectionOn = EvolutionOptions.naturalSelection;
        breedWithBestOn = EvolutionOptions.breedWithBest;

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
        if (evolutionPaused)
        {
            //best square died
            if (!bestSquare.activeSelf)
            {
                bestSquare.GetComponent<PlayerController>().ResetPlayer();
                ResetEnemies();
            }
        }
        else
        {
            //did everybody die or win?
            if (AllPlayersDead())
            {


                //set best square of last generation back to original color
                SetSquareToNormalColor();

                //find best square and keep it (also change its color)
                FindBestSquare();

                //breed squares
                if(naturalSelectionOn)
                    NaturalSelection();
                if(breedWithBestOn)
                    BreedWithBest();

                //save index of generation
                lastGens[(currGen - 1) % 10] = currGen;
                //every ten generations, save information to file
                if (currGen % 10 == 0)
                {
                    SaveScores.SaveDataToCSV(lastGens, lastBestFitness, lastAverageFitness);
                }

                //increase generation number
                currGen++;

                //add more moves every x gens
                //dont add more moves if a square has already completed the level
                if (currGen % inscreaseStepGens == 0 && !levelComplete)
                    IncreasePlayerSteps();


                //spawn players
                //reset them to initial position, steps etc
                ResetPlayers();

                //reset level between gens
                ResetEnemies();


            }
        }
           


    }

    void BreedWithBest()
    {
        PlayerController aux, auxParent;

        auxParent = squares[lastGenBest].GetComponent<PlayerController>();
        List<Vector3> movements = auxParent.movements;

        for (int i = 0; i < nSquares; i++)
        {
            //dont change the best of the generation
            if (i != lastGenBest)
            {
                squares[i].transform.position = new Vector3(squares[i].transform.position.x, squares[i].transform.position.y, -0.1f);

                aux = squares[i].GetComponent<PlayerController>();

                //crossover
                aux.CrossoverScoreBias(movements, maxScore);

                //mutation
                //aux.Mutate();
            }
        }
    }

    void IncreasePlayerSteps()
    {
        PlayerController aux;

        print("Incrementing in gen " + currGen);
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
                squares[i].transform.position = new Vector3(squares[i].transform.position.x, squares[i].transform.position.y, -0.1f);

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
        maxScore = -1;
        float score;

        //save average fitness for generation
        lastAverageFitness[(currGen-1) % 10] = 0;

        for(int i=0; i< nSquares; i++)
        {
            aux = squares[i].GetComponent<PlayerController>();
            score = aux.GetFitnessScore();
            lastAverageFitness[(currGen-1) % 10] += score;

            //player finished the level
            if (aux.won)
            {

                if (levelComplete)
                {
                    if (aux.getCurrStep() < bestSteps)
                    {
                        bestSteps = aux.getCurrStep();
                        currMaxSteps = bestSteps;
                        bestMovements = squares[i].GetComponent<PlayerController>().CloneMovements();
                        SaveBestSquare();
                        bestSquareFromGen = currGen;
                    }
                        
                }
                else
                {
                    //first player to finish
                    levelComplete = true;
                    bestSteps = aux.getCurrStep();
                    completedAtGen = currGen;
                    bestSquareFromGen = currGen;

                    //activate win panel
                    wonPanel.SetActive(true);

                    //set max steps to number of steps needed to complete
                    //solutions that take more than the current minimum aren't useful
                    currMaxSteps = bestSteps;
                    bestMovements = squares[i].GetComponent<PlayerController>().CloneMovements();
                    SaveBestSquare();
                }
            }

            if(score > maxScore)
            {
                lastGenBest = i;
                maxScore = score;
            }
        }

        print("Gen: "+currGen+" - "+  lastGenBest + ": " + maxScore);
        squares[lastGenBest].GetComponent<SpriteRenderer>().color = Color.green;
        squares[lastGenBest].transform.position = new Vector3(squares[lastGenBest].transform.position.x, squares[lastGenBest].transform.position.y, -0.2f);

        //best fitness of generation and averagefitness
        lastAverageFitness[(currGen-1) % 10] /= nSquares;
        lastBestFitness[(currGen-1) % 10] = maxScore;

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

    public int GetCompletedAtGen()
    {
        return completedAtGen;
    }

    public int GetBestSteps()
    {
        return bestSteps;
    }

    public int GetBestSquareFromGen()
    {
        return bestSquareFromGen;
    }

    /// <summary>
    /// Public function called by reset buttons to
    /// 
    /// </summary>
    public void ResetEvolution()
    {
        evolutionPaused = false;

        //destroy squares
        DeleteSquares();

        //reset parameters
        ResetParameters();

        //hide best gen UI
        wonPanel.SetActive(false);

        //spawn squares again
        SpawnFirstGeneration();
        

    }

    /// <summary>
    /// Reset evolution parameters. Called by ResetEvolution
    /// </summary>
    void ResetParameters()
    {

        lastGenBest = -1;
        levelComplete = false;
        currMaxSteps = beginSteps;
        currGen = 1;


        //used for saving in file
        //delete old file
        SaveScores.deleteCSV();
        lastBestFitness = new float[10];
        lastAverageFitness = new float[10];
        lastGens = new int[10];

        //spawn first squares
        SpawnFirstGeneration();
    }

    /// <summary>
    /// Delete all square game objects
    /// </summary>
    void DeleteSquares()
    {
        for(int i=0; i < squares.Length; i++)
        {
            GameObject.Destroy(squares[i]);
        }
    }

    /// <summary>
    /// Save data of best square to file
    /// </summary>
    void SaveBestSquare()
    {

        string sceneName = SceneManager.GetActiveScene().name;
        BinaryFormatter bf;
        System.IO.FileStream file;

        //check if file exists
        //if so, load beast square
        //check if current square is better
        //if not, abort
        if (File.Exists(Application.persistentDataPath + "/" + sceneName + ".save"))
        {
            bf = new BinaryFormatter();
            file = File.Open(Application.persistentDataPath + "/" + sceneName + ".save", FileMode.Open);
            BestSquare currentBest = (BestSquare)bf.Deserialize(file);
            file.Close();

            if (bestSteps >= currentBest.nSteps)
                return;

        }

        List<SerializableVector3> serializableMovements = new List<SerializableVector3>();

        for (int i = 0; i < bestMovements.Count; i++)
            serializableMovements.Add(new SerializableVector3(bestMovements[i].x, bestMovements[i].y, bestMovements[i].z));

        //save square to file
        BestSquare square = new BestSquare
        {
            //movements
            movements = serializableMovements,
            //gen
            gen = currGen,
            //number of steps
            nSteps = bestSteps
        };


        bf = new BinaryFormatter();
        file = File.Create(Application.persistentDataPath + "/"+ sceneName+ ".save");
        bf.Serialize(file, square);
        file.Close();

        
    }

    /// <summary>
    /// Resume evolution AFTER loading saved square
    /// Use without having paused the evolution and you will be in trouble
    /// </summary>
    public void ResumeEvolution()
    {
        //delete best square
        GameObject.Destroy(bestSquare);

        //update panel
        bestSteps = bestStepsAux;
        bestSquareFromGen = bestSquareFromGenAux;
        completedAtGen = completedAtGenAux;
        currMaxSteps = currMaxStepsAux;


        //unpause evolution
        ResetPlayers();
        ResetEnemies();
        evolutionPaused = false;
        if (!levelComplete)
            wonPanel.SetActive(false);


    }

    /// <summary>
    /// Load best square from file on level
    /// </summary>
    public bool LoadBestSquare()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        BinaryFormatter bf;
        System.IO.FileStream file;
        BestSquare currentBest;

        //load file

        if (File.Exists(Application.persistentDataPath + "/" + sceneName + ".save"))
        {
            bf = new BinaryFormatter();
            file = File.Open(Application.persistentDataPath + "/" + sceneName + ".save", FileMode.Open);
            currentBest = (BestSquare)bf.Deserialize(file);
            file.Close();



        }
        else
        {
            print("no square found!");
            return false;
        }

        //pause evolution
        PauseEvolution();

        //backup current stats
        bestStepsAux = bestSteps;
        bestSquareFromGenAux = bestSquareFromGen;
        completedAtGenAux = completedAtGen;
        currMaxStepsAux = currMaxSteps;

        //update best data
        bestSteps = currentBest.nSteps;
        bestSquareFromGen = currentBest.gen;
        completedAtGen = currentBest.gen;
        currMaxSteps = currentBest.nSteps;
 

        //update panel
        wonPanel.SetActive(true);

        //put best square on screen
        bestSquare = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        bestSquare.GetComponent<PlayerController>().loadedSquare = true;


        List<Vector3> movs = new List<Vector3>();
        for (int i = 0; i < currentBest.movements.Count; i++)
        {
            movs.Add(currentBest.movements[i]);
            print("mov " + i + ": " + currentBest.movements[i].x + "," + currentBest.movements[i].y);
        }
            

        bestSquare.GetComponent<PlayerController>().movements = movs;

        //resetEnemies
        ResetEnemies();
        return true;

    }

    /// <summary>
    /// Pause evolution by hiding squares and setting flag
    /// </summary>
    void PauseEvolution()
    {
        for(int i=0; i<nSquares; i++)
        {
            squares[i].SetActive(false);
        }
        evolutionPaused = true;

    }
}
