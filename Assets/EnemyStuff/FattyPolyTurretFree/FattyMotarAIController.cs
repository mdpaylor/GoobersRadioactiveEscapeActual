using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FattyMotarAIController : MonoBehaviour
{

    public GameObject turretPart;

    public GameObject player;

    public GameObject proj;

    public GameObject spawnfromHere;
    public float speed = 3f;

    public bool isAiming = false;

    private float startTime = 0f;

    // Total distance between the markers.
    private float journeyLength = 0f;

    public float launchVelocity = 1000f;
    //NOTE: want to start off by changing around and modifying the Y value of
    //how the turret moves 

    bool isFiring = false;
    void Start()
    {
        startTime = Time.time;
        journeyLength = Vector3.Distance(player.transform.position,turretPart.transform.position);
    }

    // Update is called once per frame
    void Update()
    {   
        if(Vector3.Distance(this.transform.position, player.transform.position) < 30){
                aim();
               if(isFiring == false){
                isFiring=true;
                Invoke("fire", 3);
            }
        }

    }

    void fire(){
        GameObject b1 = Instantiate(proj, spawnfromHere.transform.position, spawnfromHere.transform.rotation);
        if(b1 != null){
         b1.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward*launchVelocity); //new Vector3(0, launchVelocity,0));
        }
        isFiring=false;
    }

    void aim(){
        float distanceCovered = (Time.time - startTime) *speed;

        float fraction = distanceCovered/journeyLength;

        Vector3 direction = (player.transform.position - turretPart.transform.position).normalized;
        Quaternion toRotation = Quaternion.LookRotation(direction, transform.up);
        turretPart.transform.rotation = Quaternion.Lerp(turretPart.transform.rotation, toRotation, fraction);

        //isAiming= true;
        //turretPart.transform.LookAt(player.transform);
    }


}
