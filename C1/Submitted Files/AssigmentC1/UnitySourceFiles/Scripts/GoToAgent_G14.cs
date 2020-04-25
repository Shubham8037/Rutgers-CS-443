using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class GoToAgent_G14 : Agent
{
    Rigidbody rBody;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }
    
    public int Obstacles;  //number of obstacles
    public Transform Target;  //target object
    public Transform leftSide;  //child leftSide obj
    public Transform rightSide;  //child rightSide obj
    public Transform plane;  // Floor Plane

    List<GameObject> obstacleObjs=new List<GameObject>();
    List<Vector3> obstaclePositions = new List<Vector3>();

    Vector3 initPos;
    float walkedDist;
    public override void AgentReset()
    {
        // Move the player to a random spot 
        this.transform.position = randomPosOnFloor();
        this.transform.eulerAngles = new Vector3(0,Random.Range(-90,90), 90);   //set random rotation
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;

        initPos = transform.position;
        walkedDist = 0;

        // Set static target
        Target.position = randomPosOnFloor();

        obstacleObjs.Clear();
        obstaclePositions.Clear();
        for (int i = 0; i < Obstacles; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = randomPosOnFloor();
            obstacleObjs.Add(cube);
            obstaclePositions.Add(cube.transform.position);
        }
          
        lastPos = transform.position;
    }

    //returns random position on the floor, it should not intersect with other already placed objects.
    public Vector3 randomPosOnFloor()
    { 
        while (true)
        {
              Vector3 pos= new Vector3(Random.value * 8 - 4f, 0.5f, Random.value * 8 - 4f); // for 10 meter x 10 meter
            //Vector3 pos = new Vector3(Random.value * 40- 20, 0.5f, Random.value * 40 - 20);   // for 50 meter x 50 meter

            bool flag = false;
            foreach (Vector3 otherCubePos in obstaclePositions)
            {
                float dist = Vector3.Distance(otherCubePos, pos);
                if (dist < 3) flag = true;
            }
            if (Vector3.Distance(Target.position, pos) < 3) flag = true;
            if (Vector3.Distance(transform.position, pos) < 3) flag = true;
            if (flag == false) return pos;
        } 
    }
    //clear Boards with Obstacles
    public void removeObstacles()
    {
        for (int i = 0; i < obstacleObjs.Count; i++)
        {
           if (obstacleObjs[i]!=null) DestroyImmediate(obstacleObjs[i]);
        }
        obstacleObjs.Clear();
    }
    
    //return current PAO
    public float currentPAO( )
    {
        Vector3 curPosition = transform.position;
        float shortestDist = Vector3.Distance(initPos, curPosition);
        float PAO = (walkedDist - shortestDist) / (shortestDist * 1.0f) * 100;
         
        if (initPos == curPosition) PAO = 0; 
        return PAO;
    }

    //Convert vector3 to vector2 (ignore Y) and scale board coordinates to 1x1
    private Vector2 V3to2DBoardData(Vector3 vec)
    {
        Vector2 vec2 = new Vector2(vec.x, vec.z);
        //-plane.localScale.x*5 plane.localScale.x*5
        vec2.x = vec2.x / (plane.localScale.x * 5.0f);
        vec2.y = vec2.y / (plane.localScale.y * 5.0f);
        return vec2;
    }

    public override void CollectObservations()
    { 
        //Player position
        AddVectorObs(V3to2DBoardData(this.transform.position));

        //Target Relative Position
        AddVectorObs(V3to2DBoardData(Target.position - this.transform.position));  //2 space, Ignore Y

        //Obstacles Relative Positions
        for (int i = 0; i < obstacleObjs.Count; i++)
            AddVectorObs(V3to2DBoardData(obstacleObjs[i].transform.position- this.transform.position));   //2 space, Ignore Y    3X obstacle 
    }

    //move forward speed
    public float forwardSpeed = 10;
    //rotation speed
    public float rotationSpeed = 3;
     
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
    float t = 0;
    public override void AgentAction(float[] vectorAction, string textAction)
    { 
        walkedDist += Vector3.Distance(transform.position, lastPos);
        lastPos = transform.position;
         
        Vector2 forceVector = new Vector2(vectorAction[0], vectorAction[1]);

        Vector3 forw = getForwardVector();  //3D forward vector
        Vector2 forwardVector = new Vector2(forw.x, forw.z);  //2D forward vector
        
        //angle between forward vector and force vector 
        float DotResult = AngleDir(forwardVector.normalized, forceVector.normalized);
        if (forceVector != Vector2.zero)  //we should not apply any force if there is not input
        {
            float localRotSpeed = rotationSpeed;
            
            if (Mathf.Abs(DotResult) < 0.1) localRotSpeed/=4;

            if (DotResult > 0)   // if our direction is to the right
            { 
                rBody.AddForceAtPosition(localRotSpeed * forw, leftSide.position);  //apply forward force at left point
                rBody.AddForceAtPosition(-localRotSpeed * forw, rightSide.position);  //apply backward force at right point
            }
            else if (DotResult < 0)  // if our direction is to the left
            { 
                rBody.AddForceAtPosition(-localRotSpeed * forw, leftSide.position); //apply backward force at left point
                rBody.AddForceAtPosition(localRotSpeed * forw, rightSide.position);  //apply forward force at right point
            } 
        }

        //apply force when direction is already near correct
        if (Mathf.Abs(DotResult) < 0.1 && forceVector != Vector2.zero) rBody.AddForce(forwardSpeed * forw);

        //disable angular velocity
        rBody.angularVelocity = Vector3.zero;


        // Rewards



        bool obstacleFlag = false;
        for (int i = 0; i < obstaclePositions.Count; i++)
        {
            Vector3 obstaclePos = obstaclePositions[i] ;
            float dist = Vector3.Distance(this.transform.position, obstaclePos);
            //Set negative rewards, when player hits obstacle or stayed too long in front of them.
            if (dist < 2f)
            {
                if (dist < 1.3f)
                {
                    SetReward(-0.1f);
                    obstacleFlag = true;
                }
                else
                if (dist < 1.5f)
                    SetReward(-0.5f);
                else
                    SetReward(-0.25f); 
            } 
        }

        //if player stayed too long in front of obstacle
        if (obstacleFlag)
        {
            t += Time.deltaTime;
            if (t>1f)
            {
                SetReward(-1f);
                removeObstacles();
                Done();
                t = 0;
            }
        }
        else t = 0;

        float centerDistanceToTarget = Vector3.Distance(this.transform.position, Target.position);
         

        // Reached target
        if (centerDistanceToTarget < 1.5f)
        {
            float bonus = 2 * (100 - Mathf.Min(100, currentPAO())) / 100.0f;  //Give Bonus for shortest route
            SetReward(2 + bonus);
            removeObstacles();
            Done(); 
        }
        
         // Fell off platform
        if (this.transform.position.y < 0)
        {
            SetReward(-0.5f);
            removeObstacles(); 
            Done();
        } 
          
    }
    Vector3 lastPos;
}


