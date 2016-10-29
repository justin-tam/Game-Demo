/*  
 *  Project: Phoenix Horizons
 *  Class: CharacterMovement
 *  Written By: Justin Tam
 *  Date: September 2016
 *  Summary: Handle Character Movement
*/

using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour
{
    float speed = 5.0f; //player's movement speed
    float gravity = 10.0f; //amount of gravitational force applied to the player
    private CharacterController controller; //player's CharacterController component
    private GameObject childObject;
    private Actor actor;
    private GameObject meleeRange;
    private HitMe hit;
    private Animation anim;
    private Vector3 moveDirection = Vector3.zero;
    private bool crouching = false; //Determine if you are crouching or not
    private bool attacking = false; //Determine if you are attacking or not
    private string animString = "Idle";

    //Instantiate objects
    void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        childObject = controller.transform.GetChild(0).gameObject;
        actor = transform.GetComponent<Actor>();
        meleeRange = transform.GetChild(2).gameObject;
        hit = meleeRange.GetComponent<HitMe>();
        anim = childObject.GetComponent<Animation>();
    }

    void Update()
    {
        //If esc key, quit game.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        //You can't move if you're dead!
        else if (actor.health > 0)
           PerformAction();
    }

    void PerformAction()
    {
        //APPLY GRAVITY
        if (moveDirection.y > gravity * -1)
        {
            moveDirection.y -= gravity * Time.deltaTime * 1.5f;
        }
        //Move me in the direction +- time acceleration
        controller.Move(moveDirection * Time.deltaTime);
        var left = transform.TransformDirection(Vector3.left);

        //If I am on the ground, do something
        //TODO: Add key assignment option
        if (controller.isGrounded)
        {
            //If space is pressed, jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                moveDirection.y = speed;
                //If you were crouching, you aren't crouching anymore - BURPIES!
                if (crouching)
                {
                    crouching = false;
                    controller.height = controller.height * 2.5f;
                }
            }
            //If left control pressed, crouch
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (!crouching)
                {
                    crouching = true;
                    controller.height = controller.height / 2.5f;
                }
                else
                {
                    crouching = false;
                    controller.height = controller.height * 2.5f;
                }
            }
            //If w pressed, move forward
            if (Input.GetKey("w"))
            {
                if (!crouching)
                {
                    controller.SimpleMove(transform.forward * speed);
                }
                else
                {
                    controller.SimpleMove(transform.forward * speed / 2f);
                }
                animString = "RunFront";
            }
            //If s pressed, move backward
            if (Input.GetKey("s"))
            {
                if (!crouching)
                {
                    controller.SimpleMove(transform.forward * -speed / 1.25f);
                }
                else
                {
                    controller.SimpleMove(transform.forward * -speed / 3f);
                }
                animString = "RunBack";
            }
            //If a pressed, move left
            if (Input.GetKey("a"))
            {
                if (!crouching)
                {
                    controller.SimpleMove(left * speed);
                }
                else
                {
                    controller.SimpleMove(left * speed / 3f);
                }
                animString = "RunLeft";
            }
            //If d pressed, move right
            if (Input.GetKey("d"))
            {
                if (!crouching)
                {
                    controller.SimpleMove(left * -speed);
                }
                else
                {
                    controller.SimpleMove(left * -speed / 3f);
                }
                animString = "RunRight";
            }
            //If no key press and not attacking, run idle anim
            if (Input.anyKey == false && CheckAttack())
            {
                animString = "Idle";
            }
        }
        //If in the air, do something
        else
        {
            Vector3 relative;
            relative = transform.TransformDirection(0, 0, 1);
            animString = "Jump";

            //If w pressed, move forward
            if (Input.GetKey("w"))
            {
                controller.Move(relative * Time.deltaTime * speed / 1.5f);
            }
            //If s pressed, move backward
            if (Input.GetKey("s"))
            {
                controller.Move(relative * Time.deltaTime * -speed / 2f);
            }
            //If a pressed, move left
            if (Input.GetKey("a"))
            {
                controller.SimpleMove(left * Time.deltaTime * speed / 1.5f);
            }
            //If d pressed, move right
            if (Input.GetKey("d"))
            {
                controller.SimpleMove(left * Time.deltaTime * -speed / 1.5f);
            }
        }
        // Attacks
        if (Input.GetMouseButtonDown(0))
        {
            if (CheckAttack())
            {
                attacking = true;
                int attack = Random.Range(0, 1);
                if (attack == 0) { animString = "AttackMelee1"; }
                else { animString = "AttackMelee2"; }
                anim.Play(animString);
                StartCoroutine(FinishAnim());
            }
        }
        //Run the Block cycle
        //Raise shield
        if (Input.GetMouseButtonDown(1))
        {
            animString = "Block_up";
        }
        //Holding shield
        if (Input.GetMouseButton(1))
        {
            speed = 1.0f;
            animString = "Block_loop";
        }
        //Lower shield
        if (Input.GetMouseButtonUp(1))
        {
            speed = 5.0f;
            animString = "Block_down";
        }
        //If not attacking, then fade into next animation
        if (!attacking)
            anim.CrossFade(animString);
    }

    //Wait a time and calculate hit on enemy
    IEnumerator FinishAnim()
    {
        yield return new WaitForSeconds(0.5F);
        hit.Melee();
        attacking = false;
    }

    //Return true if alive and neither attacking or being attacked
    bool CheckAttack()
    {
        return ((actor.health > 0) && !(anim.IsPlaying("AttackMelee1") || anim.IsPlaying("AttackMelee2") || anim.IsPlaying("AttackRange1") || anim.IsPlaying("AttackRange2") || anim.IsPlaying("Hit")));
    }

    //On scene entrance, display scene title
    //TODO: Add level loader
    void OnTriggerEnter(Collider hit)
    {
        if (hit.transform.tag == "LoadNewScene")
        {
            hit.transform.GetComponent<LoadNewScene>().DisplayScene();
        }
    }
    //On scene exit, remove display
    void OnTriggerExit(Collider hit)
    {
        if (hit.transform.tag == "LoadNewScene")
        {
            hit.transform.GetComponent<LoadNewScene>().HideScene();
        }
    }
    //On collision, add force to prevent entering object
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.GetComponent<Rigidbody>())
        {
            hit.transform.GetComponent<Rigidbody>().AddForce(10 * transform.forward);
        }
    }
}