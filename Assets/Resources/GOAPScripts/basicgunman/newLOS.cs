
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class newLOS : MonoBehaviour
{
    public GameObject player;
    public bool seeingPlayer = false;
    // Start is called before the first frame update

    private MovingObjectAgentMove movementScript;

    void Start()
    {
        movementScript = GetComponent<MovingObjectAgentMove>();

        if (player == null){
            player = findClosestenemy();
        }

        if (PersonalNetworkAgent.Instance != null && NetworkManager.Instance != null)
        {
            PersonalNetworkAgent.Instance.OnPlayerDisconnectedEvent += PersonalNetworkAgent_OnPlayerDisconnected;
        }
    }

    private void PersonalNetworkAgent_OnPlayerDisconnected(object sender, System.EventArgs e)
    {
        player = findClosestenemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (movementScript != null && movementScript.isDummyObject) return;

        if(hasLOS(this.gameObject, player)){
            seeingPlayer = true;
        }else{
            seeingPlayer = false;
        }
        
    }

    GameObject findClosestenemy(){
        GameObject[] gos;
        GameObject closest = null;
        gos = GameObject.FindGameObjectsWithTag("Player");

        if(gos==null){
            return null;
        }
        else{
            float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
        }

    }

    bool hasLOS(GameObject source, GameObject dest){
   // firstly cast a ray between the two objects and see if there are any
   // obstacles inbetween (some obstacles have "partial visibility" in which
   // case we may or may not want to include as a "hit")

    //RaycastHit[] hits;
    RaycastHit[] hits; 
    bool obj_hit = false;

        if (dest == null)
        {
            player = findClosestenemy();
            dest = player;
        }

    Vector3 dir = dest.transform.position - source.transform.position;

    Ray ry = new Ray ();
    ry.origin = source.transform.position;
    ry.direction = dir;

   hits = Physics.RaycastAll (ry, dir.magnitude);
        // Debug.DrawRay (source.transform.position, dir, Color.cyan, 4.0f);
        if(hits.Length == 0)
        {
            return false;
        }
   if(hits[0].transform.position == player.transform.position){
    obj_hit = true;
   }else{
    obj_hit = false;
   }

  //  foreach(RaycastHit hit in hits){
  //     // here we could look at an attached script (if one exists) on the object and
  //     // decide whether or not this should actually constitute a hit
  //    // Debug.Log("LOS test hit from "+source.transform.position+" to "+dest.transform.position+" = "+hit.transform.parent.gameObject.name);
  //     //obj_hit = true;
  //     if(hit.transform.position != player.transform.position){
  //       obj_hit = false;
  //     } else if(hit.transform.position == player.transform.position){
  //       obj_hit = true;
  //     }else{
  //       obj_hit = false;
  //     }
  //  }

   return(obj_hit);
}

    public void UnsubscribeFromEvents()
    {
        PersonalNetworkAgent.Instance.OnPlayerDisconnectedEvent -= PersonalNetworkAgent_OnPlayerDisconnected;
    }
}
