/*  
 *  Project: Phoenix Horizons
 *  Class: AIInteraction
 *  Written By: Justin Tam
 *  Date: September 2016
 *  Summary: Handle AI Interaction with other characters
 *  TODO: Handle AI interaction with non-character objects
*/

using UnityEngine;
using System.Collections;

public class AIInteraction : MonoBehaviour
{
    //Setup objects
    private Actor actor;
    private Quaternion originalRot;
    private bool checkNear = false;
    public float speed = 100f;
    public float turnSpeed = 10f;
    private Animation anim;

    // Use this for initialization
    void Start ()
    {
        actor = transform.GetComponent<Actor>();
        originalRot = transform.rotation;
    }
	
	// Update is called once per frame
	void Update ()
    {
        anim = GetComponent<CharacterController>().transform.GetChild(0).gameObject.GetComponent<Animation>();
        //If either Idle or not aggressive, look at nearest actor
        if (anim.IsPlaying("Idle") && actor.mood != 5)
        {
            LookAgain();
        }
    }

    //Look at nearest Actor
    //TODO: Can make this simpler
    void LookAgain()
    {
        //Array of actors
        Actor[] actors = GameObject.FindObjectsOfType<Actor>();
        Actor nearestActor = null;
        float dist = 2.5f;

        //Run through each actor
        foreach (Actor e in actors)
        {
            //Check if the actor is ME!
            if (transform.position == e.transform.position || (e.health <= 0))
            {
                //If it is me, find next actor
                continue;
            }

            //Get distance between actor and me
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < dist)
            {
                //if distance is less than 2.5f, set nearest actor and nearest distance
                nearestActor = e;
                dist = d;
            }
        }

        //If no near actor, do nothing.
        if (nearestActor == null)
        {
            //Return rotation to original rotation
            if (transform.rotation != originalRot)
            {
                checkNear = false;
                SlowRotate(originalRot);
            }
        }
        //If actor near, rotate character to look at nearest character
        else
        {
            if (!checkNear)
            {
                checkNear = true;
                originalRot = transform.rotation;
            }
            Vector3 dir = nearestActor.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            SlowRotate(targetRotation);
        }
    }

    //Set turn acceleration
    void SlowRotate(Quaternion inputRot)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, inputRot, Time.deltaTime * turnSpeed);
    }
}