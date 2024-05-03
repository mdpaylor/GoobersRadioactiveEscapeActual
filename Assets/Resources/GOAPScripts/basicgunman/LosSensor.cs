using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;


public class LosSensor : LocalWorldSensorBase
{
    public GameObject player;
    public bool seeingPlayer = false;
    public override void Created()
    {
        if (player ==null){
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public override void Update()
    {
        
    }

    public override SenseValue Sense(IMonoAgent agent, IComponentReference references)
    {
        // References are cached by the agent.
        if (player == null) Debug.Log("IS NULL");

        bool playerSeen = hasLOS(agent.transform.position, player);
        if(playerSeen){
            return 5;
        }else{
            return 0;
        }
       //return(this.gameObject, player);
    }

       bool hasLOS(Vector3 sourcepos, GameObject dest){
   // firstly cast a ray between the two objects and see if there are any
   // obstacles inbetween (some obstacles have "partial visibility" in which
   // case we may or may not want to include as a "hit")

    //RaycastHit[] hits;
    RaycastHit[] hits; 
    bool obj_hit = false;

    Vector3 dir = dest.transform.position - sourcepos;

    Ray ry = new Ray ();
    ry.origin = sourcepos;
    ry.direction = dir;

   hits = Physics.RaycastAll (ry, dir.magnitude);
   Debug.DrawRay (sourcepos, dir, Color.cyan, 4.0f);

   foreach(RaycastHit hit in hits){
      // here we could look at an attached script (if one exists) on the object and
      // decide whether or not this should actually constitute a hit
     // Debug.Log("LOS test hit from "+source.transform.position+" to "+dest.transform.position+" = "+hit.transform.parent.gameObject.name);
      //obj_hit = true;
      if(hit.transform.position == player.transform.position){
        obj_hit = true;
      }else{
        obj_hit = false;
      }
   }
   return(obj_hit);
    }
}