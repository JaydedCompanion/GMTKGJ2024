using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Based on Dan Schatzeder's Cannon Trajectory Method, found at https://github.com/Schatzeder/Trajectory-Prediction

public class ThrowPreview : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] LineRenderer lr;
    public Rigidbody2D held; //object being held

    public float force;
    public float mass;
    public float vel; //Initial Velocity, calculated via V = Force / Mass * fixedTime (0.02)
    //public Vector2 vel;
    public float gravity = 1;
    public float collisionCheckRadius = 0.1f; //Collision radius of last point on SimulationArc, to communicate with it when to stop

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        lr = GetComponent<LineRenderer>();
        lr.startColor = Color.white; //this doesnt work for some reason

        force = playerController.throwForce.y / playerController.throwForce.x * 1000; //good ol' rise over run, because the throw code uses a vector for force. Multiplied so that its not a tiny decimal               
    }

    void Update()
    {
        held = playerController.holdingObject;

        if (held != null)
        {            
            lr.enabled = true;
            mass = held.mass;
            DrawTrajectory();
        }
        else
        {
            lr.enabled = false;
        }
    }

    void DrawTrajectory()
    {
        lr.positionCount = SimulateArc().Count;
        for (int a = 0; a < lr.positionCount; a++)
        {
            lr.SetPosition(a, SimulateArc()[a]); //Add each Calculated Step to a LineRenderer to display a Trajectory. Look inside LineRenderer in Unity to see exact points and amount of them
        }
    }

    private List<Vector2> SimulateArc()
    {
        List<Vector2> lineRendererPoints = new List<Vector2>(); //Reset LineRenderer List for new calculation

        float maxDuration = 5f; //amount of total time for simulation
        float timeStepInterval = 0.05f; //amount of time between each position check
        int maxSteps = (int)(maxDuration / timeStepInterval);//Calculates amount of steps simulation will iterate for
        Vector2 directionVector = playerController.facingDirection;
        Vector2 launchPosition = held.transform.position;       

        vel = force / mass * Time.fixedDeltaTime;

        for (int i = 0; i < maxSteps; ++i)
        {
            //Remember f(t) = (x0 + x*t, y0 + y*t - 9.81t²/2)
            //calculatedPosition = Origin + (transform.up * (speed * which step * the length of a step);
            Vector2 calculatedPosition = launchPosition + (directionVector * vel * i * timeStepInterval); //Move both X and Y at a constant speed per Interval
            calculatedPosition.y += Physics2D.gravity.y/2 * Mathf.Pow(i * timeStepInterval, 2); //Subtract Gravity from Y
            //can't figure out how to get this curve more accurate

            lineRendererPoints.Add(calculatedPosition);

            if (CheckForCollision(calculatedPosition)) //if you hit something, stop adding positions
            {
                break; //stop adding positions
            }
        }
        return lineRendererPoints;
    }

    private bool CheckForCollision(Vector2 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, collisionCheckRadius); //Measure collision via a small circle at the latest position, dont continue simulating Arc if hit
        if (hits.Length > 0) //Return true if something is hit, stopping Arc simulation
        {
            return true;
        }
        return false;
    }
}


