using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSettings : MonoBehaviour {

    public void SetMutationRate(float mutationRate)
    {
        EvolutionOptions.mutationRate = mutationRate;
    }

    public void SetBeginSteps(float beginSteps)
    {
        EvolutionOptions.beginSteps = (int) beginSteps;
    }

    public void SetIncreaseStepsGens(float increaseStepsGens)
    {
        EvolutionOptions.increaseStepsGens = (int)increaseStepsGens;
    }

    public void SetIncreaseSteps(float increaseSteps)
    {
        EvolutionOptions.increaseSteps = (int)increaseSteps;
    }

    public void SetNSquares(float nSquares)
    {
        EvolutionOptions.nSquares = (int)nSquares;
    }

    public void SetBreedWithBest(bool breedWithBest)
    {
        EvolutionOptions.breedWithBest = breedWithBest;
    }

    public void SetNaturalSelection(bool naturalSelection)
    {
        EvolutionOptions.naturalSelection = naturalSelection;
    }

}
