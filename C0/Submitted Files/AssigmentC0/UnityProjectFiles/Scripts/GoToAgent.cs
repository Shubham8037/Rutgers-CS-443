using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class GoToAgent : Agent
{
    Rigidbody rBody;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target;

    public Transform leftSide;
    public Transform rightSide;

    public override void AgentReset()
    {
        // Move the player to a random spot 
        this.transform.position = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        this.transform.eulerAngles = new Vector3(0,Random.Range(-90,90), 90);   //set random rotation
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;


        // Set static target
        Target.position = new Vector3(0, 0.5f, 0);
         
    }

    public override void CollectObservations()
    {
        // Target and Agent positions
        AddVectorObs(Target.position);
        AddVectorObs(this.transform.position);
         
        AddVectorObs(getForwardVector());    // cylinder direction
        AddVectorObs(rBody.velocity.magnitude);   //velocity strength
    }

    //move forward speed
    public float forwardSpeed = 10;
    //rotation speed
    public float rotationSpeed = 3;

    public float debug;


    //return forward vector of the cylinder based on leftSite and rightSide child objects.
    public Vector3 getForwardVector()
    { 
        Vector3 vec = rightSide.position - leftSide.position;
        Vector2 vec2D = Vector2.Perpendicular(new Vector2(vec.x, vec.z));

        return new Vector3(vec2D.x, 0, vec2D.y).normalized;
    }

    public static float AngleDir(Vector2 A, Vector2 B)
    {
        return -A.x * B.y + A.y * B.x;
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        Vector2 forceVector = new Vector2(vectorAction[0], vectorAction[1]);

        Vector3 forw = getForwardVector();  //3D forward vector
        Vector2 forwardVector = new Vector2(forw.x, forw.z);  //2D forward vector
        
        //angle between forward vector and force vector

        float DotResult = AngleDir(forwardVector.normalized, forceVector.normalized);
        if (forceVector != Vector2.zero)  //we should not apply any force if there is not input
        {
            if (DotResult > 0)   // if our direction is to the right
            { 
                rBody.AddForceAtPosition(rotationSpeed * forw, leftSide.position);  //apply forward force at left point
                rBody.AddForceAtPosition(-rotationSpeed * forw, rightSide.position);  //apply backward force at right point
            }
            else if (DotResult < 0)  // if our direction is to the left
            { 
                rBody.AddForceAtPosition(-rotationSpeed * forw, leftSide.position); //apply backward force at left point
                rBody.AddForceAtPosition(rotationSpeed * forw, rightSide.position);  //apply forward force at right point
            } 
        }
        //apply force when direction is already near correct
        if (Mathf.Abs(DotResult) < 0.1) rBody.AddForce(forwardSpeed * forw);
        //disable angular velocity
        rBody.angularVelocity = Vector3.zero;
          
        // Rewards
        float centerDistanceToTarget = Vector3.Distance(this.transform.position, Target.position);
        float leftDistanceToTarget = Vector3.Distance(leftSide.transform.position, Target.position);
        float rightDistanceToTarget = Vector3.Distance(rightSide.transform.position, Target.position);

        float minDist = Mathf.Min(Mathf.Min(centerDistanceToTarget, leftDistanceToTarget), rightDistanceToTarget);

        // Reached target
        if (minDist < 1.4f)
        {
            SetReward(1f);
            Done();
        }
        
         // Fell off platform
        if (this.transform.position.y < 0)
        { 
            Done();
        }

    }
}


