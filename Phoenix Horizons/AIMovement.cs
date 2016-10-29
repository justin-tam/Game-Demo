/*  
 *  Project: Phoenix Horizons
 *  Class: AIMovement
 *  Written By: Justin Tam
 *  Date: September 2016
 *  Summary: Handle AI Movement
*/

using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class AIMovement : MonoBehaviour {
    //The point to move to
    private List<Vector3> targetPosition = new List<Vector3>();
    //Array of actors
    private Actor[] allActors;
    private List<Actor> enemies = new List<Actor>();
    private List<Actor> allies = new List<Actor>();
    private Actor nearestActor;
    private Actor actor;
    //Ranges
    private GameObject meleeRange;
    private float aggroRange = 25f;
    private float attackRange = 1.25f;
    //Time
    private float nextTime = 0;
    private float timeDelay = 2.5f;
    //Objects
    private HitMe hit;
    GameObject pathWaypoint;
    Vector3 targetWaypoint;
    private Seeker seeker;
    private CharacterController controller;
    private Animation anim;
    //The calculated path
    public Path path;
    //The AI's speed per second
    public float speed = 100f;
    public float turnSpeed = 10f;
    //The max distance from the AI to a waypoint for it to continue to the next waypoint
    public float nextWaypointDistance = 3;
    //The waypoint we are currently moving towards
    private int currentWaypoint = 0;
    private bool trigger = false;
    //Current animation string
    private string animString = "Idle";

    void Start()
    {
        //Instantiate objects
        allActors = GameObject.FindObjectsOfType<Actor>();
        actor = transform.GetComponent<Actor>();
        meleeRange = transform.GetChild(1).gameObject;
        hit = meleeRange.GetComponent<HitMe>();
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
        anim = controller.transform.GetChild(0).gameObject.GetComponent<Animation>();
        //Make attacks only fire only
        anim["Mine1/Attack1"].wrapMode = WrapMode.Once;
        anim["Mine2/Attack2"].wrapMode = WrapMode.Once;
        //Find random Waypoint
        pathWaypoint = GameObject.Find("Waypoint");
        //Start a new path to the targetPosition, return the result to the OnPathComplete function
        GoPath();
    }

    void Update()
    {
        //If alive and neither attacking nor under attack, do something
        //This allows attack/hit animation to play out
        if (CheckAttack())
            PerformMove();
    }

    int indexWaypoint
    {
        get
        {
            return Random.Range(0, pathWaypoint.transform.childCount - 1);
        }
    }

    //Return random position based on input
    Vector3 randPosition(Vector3 original, float randRange)
    {
        return new Vector3(original.x + Random.Range(-randRange, randRange), 0, original.z + Random.Range(-randRange, randRange));
    }

    void OnPathComplete(Path p) {
        if (!p.error)
        {
            path = p;
            //Reset the waypoint counter
            currentWaypoint = 0;
        }
    }

    //Get target, either a waypoint or a character
    void FindTarget()
    {
        //If not aggressive, move normally
        if (actor.mood != 5)
        {
            if (pathWaypoint != null)
                targetWaypoint = pathWaypoint.transform.GetChild(indexWaypoint).position;
            else
                targetPosition.Add(randPosition(transform.position, 15f));
        }
        //Aggressive, find enemy
        else
        {
            //Find Enemies if nearestActor is null or there are multiple enemies + random 33% chance
            if (nearestActor == null || (Vector3.Distance(transform.position, nearestActor.transform.position) < aggroRange) && (enemies.Count > 1))
                FindEnemy();
            //If enemy is near and alive, chase
            if (CheckEnemy())
                targetWaypoint = nearestActor.transform.position;
            //If enemy is dead or nonexistant, follow waypoints
            else
            {
                //nearestActor is either nonexistant or dead, set to null
                nearestActor = null;
                targetWaypoint = pathWaypoint.transform.GetChild(indexWaypoint).position;
            }
        }
        targetPosition.Add(targetWaypoint);
    }

    //Check for living enemies
    bool CheckEnemy()
    {
        return (nearestActor != null && nearestActor.health > 0);
    }

    //Get nearest enemy
    void FindEnemy()
    {
        float dist = 25f;

        SetEnemy();

        foreach (Actor e in enemies)
        {
            //Get distance between actor and me
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < dist)
            {
                //if distance is less than 2.5f, set nearest actor and nearest distance
                nearestActor = e;
                dist = d;
            }
        }
    }

    //Set all enemies in scene
    void SetEnemy()
    {
        foreach (Actor e in allActors)
        {
            if ((actor.faction != e.faction) && (e.health > 0))
            {
                enemies.Add(e);
            }
            else
            {
                enemies.Remove(e);
            }
        }
    }

    //Set all allies in scene
    void SetAlly()
    {
        foreach (Actor e in allActors)
        {
            if ((actor.faction == e.faction) && (e.health > 0))
            {
                allies.Add(e);
            }
            else
            {
                allies.Remove(e);
            }
        }
    }
    
    //Perform Attack
    void PerformAction()
    {
        int attack = Random.Range(0, 1);
        if (attack == 0) { animString = "Mine1/Attack1"; }
        else { animString = "Mine2/Attack2"; }
        anim.Play(animString);
        //Wait for attack anim to finish
        StartCoroutine(FinishAnim());
    }

    IEnumerator FinishAnim()
    {
        yield return new WaitForSeconds(0.5F);
        //Deal damage effect after animation plays
        hit.Melee();
    }

    //Move character
    void PerformMove()
    {
        if (path == null)
        {
            //We have no path to move after yet
            return;
        }
        //If current path is finished, do something
        if (currentWaypoint >= path.vectorPath.Count)
        {
            //If aggressive and enemy is within range, do something.
            if (actor.mood == 5 && CheckEnemy())
            {
                //If enemy wihtin attack range, attack
                if (Vector3.Distance(meleeRange.transform.position, nearestActor.transform.position) < attackRange)
                {
                    PerformAction();
                }
            }
            //If no enemy within range, stand idle
            else
            {
                anim.CrossFade("Idle");
            }
            //Figure out next move
            NextMove();
            return;
        }
        //If aggressive and no enemy in range, determine next move.
        else if (actor.mood == 5)
        {
            if (Time.time > nextTime)
            {
                nextTime = Time.time + Random.Range(0.5f, timeDelay);
                NextMove();
            }
        }
        //Direction to the next waypoint
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        dir *= speed * Time.deltaTime;
        //Perform character model rotation towards destination
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        //Run animation
        anim.CrossFade("WalkFront");
        //Move character
        controller.SimpleMove(dir);
        //Check if we are close enough to the next waypoint
        //If we are, proceed to follow the next waypoint
        if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
        {
            //Advance to the next waypoint on Node grid
            currentWaypoint++;
            return;
        }
    }

    //Move along path
    void GoPath()
    {
        FindTarget();
        //Start along the grid path
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, targetPosition[0], OnPathComplete);
            trigger = true;
        }
    }

    //Find next move
    public void NextMove()
    {
        if (targetPosition.Count > 0)
        {
            targetPosition.RemoveAt(0);
            if (targetPosition.Count > 0)
            {
                GoPath();
            }
            else if (trigger)
            {
                //Go somewhere else after a while
                trigger = false;
                if (actor.mood != 5)
                    Invoke("GoPath", Random.Range(3f, 25f));
                else
                    Invoke("GoPath", 0.25f);
            }
        }
        else { FindTarget(); }
    }

    //Return true if character is alive and not attacking or being attacked
    bool CheckAttack()
    {
        return ((actor.health > 0) && !(anim.IsPlaying("Mine1/Attack1") || anim.IsPlaying("Mine2/Attack2") || anim.IsPlaying("Hit1") || anim.IsPlaying("Hit2")));
    }

    //Do something when hitting an obstacle
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        int layer = hit.gameObject.layer;
        //If encountering building or clutter, try to move around it
        if ((layer == 9 || layer == 10) && targetPosition.Count > 0)
        {
            //If not aggressive move a random distance away
            if (actor.mood != 5)
            {
                targetPosition.Insert(0, randPosition(transform.position, 2.5f));
                GoPath();
            }
            //If aggressive, move based on nearest actor
            else
            {
                targetPosition.Insert(0, randPosition(nearestActor.transform.position, 1.5f));
                GoPath();
            }
        }
        //Force to prevent you from going INTO obstacles
        if (hit.transform.GetComponent<Rigidbody>())
        {
            hit.transform.GetComponent<Rigidbody>().AddForce(10 * transform.forward);
        }
    }
}