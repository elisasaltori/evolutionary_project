using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalMarker : MonoBehaviour{

    Vector3 pos;
    public float distanceToGoal = 0f; //acummulated distance to final goal
    public float areaRadius = 1f; //radius around which the square can pass to count as goal reached

    private void Start()
    {
        this.pos = transform.position;
    }

    /// <summary>
    /// Get acummulated distance to goal. 
    /// </summary>
    /// <param name="next">next GoalMarker. If null, current goalmarker is considered to be the last one</param>
    /// <returns></returns>
    public void SetDistanceToGoal(GoalMarker next)
    {
        if (next == null)
            distanceToGoal = 0f;
        else
        {
            //distance to goal is the distance to the next goalmarker + distancetogoal from next goalmarker
           distanceToGoal = Vector3.Distance(pos, next.pos) + next.distanceToGoal;
        }
           
    }

    public Vector3 getPosition()
    {
        return pos;
    }
}
