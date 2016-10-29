/*  
 *  Project: Phoenix Horizons
 *  Class: HitMe
 *  Written By: Justin Tam
 *  Date: September 2016
 *  Summary: Handle character hit damage, tied to Collision Collider
*/

using UnityEngine;
using System.Collections.Generic;

public class HitMe : MonoBehaviour {
    //List and array of Actors
    private List<GameObject> nearEnemy = new List<GameObject>();
    private List<Actor> enemies = new List<Actor>();
    private List<Actor> allies = new List<Actor>();
    private Actor[] allActors;
    private Actor actor;
    private Animation anim;
    private float nextTime = 0;
    private float timeDelay = 3f;
    private string animString = "";

    // Use this for initialization
    void Start ()
    {
        //Instantiate array of all actors
        allActors = GameObject.FindObjectsOfType<Actor>();
    }
	
	// Update is called once per frame
	void Update () {
	}

    //Calculate Melee Range Attack
    public void Melee()
    {
        //Attack range position (Character Collider)
        Vector3 pos = transform.position;
        //Figure which enemy get hit
        foreach (GameObject e in nearEnemy)
        {
            Actor enemy = e.GetComponent<Actor>();

            //If dead, can't be hit
            if (enemy.health <= 0)
                continue;

            //Enemy position
            Vector3 enemyPos = e.transform.position;
            Vector3 dir = enemyPos - pos;

            //Figure out which enemy is closest
            if (Vector3.Dot(dir, transform.forward) < 0.7)
            {
                actor = enemy;
                //If no enemies or delay exceeded, find other enemies within range
                if (enemies.Count == 0 || (Time.time > nextTime))
                {
                    nextTime = Time.time + (timeDelay * 10);
                    SetEnemy();
                }

                //Get enemy animation
                anim = e.GetComponent<CharacterController>().transform.GetChild(0).gameObject.GetComponent<Animation>();
                //If they aren't blocking, do something
                if (!anim.IsPlaying("Block_loop"))
                {
                    //Lower health 1
                    //TODO: Make this more complicated based on weapon damage/armor reduc
                    enemy.health -= 1;
                    //If enemy is still alive, do something
                    if (enemy.health > 0)
                    {
                        //If not the player then then run AI animations
                        if (enemy.faction != 3)
                        { 
                            int attack = Random.Range(0, 1);
                            if (attack == 0) { animString = "Hit1"; }
                            else { animString = "Hit2"; }
                            //Raise alarm and get allies to fight
                            if (allies.Count == 0 || (Time.time > nextTime))
                            {
                                nextTime = Time.time + (timeDelay);
                                SetAlly();
                                foreach (Actor ally in allies)
                                {
                                    //Get distance between actor and me
                                    float d = Vector3.Distance(actor.transform.position, ally.transform.position);
                                    if (d < 25f)
                                    {
                                        //Call for help!
                                        RaiseAlarm(ally, ally.gameObject);
                                    }
                                }
                            }
                        }
                        //If player, run player animation
                        //TODO: camera movement during hit.
                        else
                        {
                            animString = "Hit";
                        }
                    }
                    //If no health, run death animation and remove character collider
                    else
                    {
                        animString = "Death";
                        e.GetComponent<CharacterController>().enabled = false;
                        //nearEnemy.RemoveAt(i);
                    }
                    //Run the associated animation
                    anim.Play(animString);
                }
                return;
            }
        }
    }

    //Raise alarm to active allies within range
    void RaiseAlarm(Actor target, GameObject targetObj)
    {
        target.mood = 5;
        Seeker script = targetObj.GetComponent<Seeker>();
        if (script == null)
        {
            targetObj.AddComponent<Seeker>();
            targetObj.AddComponent<AIMovement>();
        }
        else
        {
            targetObj.GetComponent<AIMovement>().NextMove();
        }
    }

    //Set Enemy
    void SetEnemy()
    {
        //For all actors, if not my faction and alive, add enemy
        //TODO: figure out hostile faction vs neutral faction
        foreach (Actor e in allActors)
        {
            if ((actor.faction != e.faction) && (e.health > 0))
            {
                enemies.Add(e);
            }
        }
    }

    //Set Ally
    void SetAlly()
    {
        //For all actors, if actor is my faction and alive, add ally
        foreach (Actor e in allActors)
        {
            if ((actor.faction == e.faction) && (e.health > 0))
            {
                allies.Add(e);
            }
        }
    }

    //Object enters attack range
    void OnTriggerEnter(Collider hit)
    {
        //If character, add object to nearest enemy list
        if (hit.gameObject.tag == "Character")
        {
            nearEnemy.Add(hit.gameObject);
        }
    }

    //Object leaves attack range
    void OnTriggerExit(Collider hit)
    {
        //Remove object from nearenemy list
        nearEnemy.Remove(hit.gameObject);
    }
}