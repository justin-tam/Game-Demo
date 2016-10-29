/*  
 *  Project: Phoenix Horizons
 *  Class: CameraCollision
 *  Written By: Justin Tam
 *  Date: September 2016
 *  Summary: Handle Character View
*/

using UnityEngine;

public class CameraCollision : MonoBehaviour
{

    bool canScroll = true; //whether or not you can zoom in and out
    Transform focusPoint; //used as the focal rotation point, and raycast point | must be centered on the player(x and z)
    float detectionRadius = 0.15f; //radius detection | used to prevent the camera from peering through when standing up against a wall
    float zoomDistance = 1f; //the distance the camera will zoom per scroll
    int maxZoomOut = 5; //used to limit distance you can zoom out, away from your character
    int maxZoomIn = 3; //used to limit distance you can zoom in, towards your character
    int zoom = 0; //used to limit distance you can zoom in and out
    RaycastHit hit; //used to detect objects in front of camera
    GameObject camFollow; //monitors camera's position
    GameObject camSpot; //camera's destination | used for zooming camera in and out

    void Start()
    {
        //set clipping planes to 0.01
        GetComponent<Camera>().nearClipPlane = 0.01f;

        //set focusPoint
        if (focusPoint == null)
        {
            focusPoint = transform.parent.transform;
        }

        //create camSpot
        camSpot = new GameObject();
        camSpot.transform.name = "CameraSpot";
        camSpot.transform.parent = transform.parent;
        camSpot.transform.position = transform.position;

        //create camFollow
        camFollow = new GameObject();
        camFollow.transform.name = "CameraFollow";
        camFollow.transform.parent = transform.parent;
        camFollow.transform.position = focusPoint.position;
        //make sure the camFollow is looking at the camera
        camFollow.transform.LookAt(transform);
    }
    void Update()
    {
        //If player mouse-scrolls foward
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (canScroll == true)
            {
                //can only zoom in four intervals from camSpot's starting pos
                if (zoom < maxZoomIn)
                {
                    //zoom camSpot in
                    camSpot.transform.position = camSpot.transform.position + 1 * -camFollow.transform.forward;
                    maxZoomOut += 1; maxZoomIn -= 1;
                }
            }
        }
        //If player mouse-scrolls backward
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (canScroll == true)
            {
                //can only zoom out four intervals from camSpot's starting pos
                if (zoom > -maxZoomOut)
                {
                    //zoom camSpot out
                    camSpot.transform.position = camSpot.transform.position - 1 * -camFollow.transform.forward;
                    maxZoomOut -= 1; maxZoomIn += 1;
                }
            }
        }

        //distance between camFollow and camSpot
        float distFromCamSpot = Vector3.Distance(camFollow.transform.position, camSpot.transform.position);
        //distance between camFollow and camera
        float distFromCamera = Vector3.Distance(camFollow.transform.position, transform.position);

        //ShereCast from camFollow to camSpot
        if (Physics.SphereCast(camFollow.transform.position, detectionRadius, camFollow.transform.forward, out hit, distFromCamSpot))
        {
            //**MAKE SURE YOUR PLAYER IS NOT BETWEEN THE FOCUS-POINT AND CAMERA**
            //get distance betwen camFollow and hitPoint of raycast
            var distFromHit = Vector3.Distance(camFollow.transform.position, hit.point);
            //if camera is behind an object, immediately put it in front
            if (distFromHit < distFromCamera)
            {
                //if player is ver close to a wall, bring camera inward, 
                //but do not exceed the camFollow's position (dont put camera in front of player)
                if (distFromCamera > 1)
                {
                    transform.position = hit.point + 1 * -camFollow.transform.forward;
                }
                else
                {
                    transform.position = camFollow.transform.position;
                }
            }
            else
            {
                //if player is ver close to a wall, bring camera inward, 
                //but do not exceed the camFollow's position (dont put camera in front of player)
                if (distFromCamera > 1)
                {
                    transform.position = Vector3.MoveTowards(transform.position, hit.point + 1 * -camFollow.transform.forward, 5 * Time.deltaTime);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, camFollow.transform.position, 5 * Time.deltaTime);
                }
            }
        }
        else
        {
            //ease camera back to camSpot
            transform.position = Vector3.MoveTowards(transform.position, camSpot.transform.position, 5 * Time.deltaTime);
        }
    }
}