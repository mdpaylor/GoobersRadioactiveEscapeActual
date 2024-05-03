using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Interfaces;
using CrashKonijn.Goap.Sensors;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.VisualScripting;

public class PlayerTargetSensor : LocalTargetSensorBase
{
    
    
    GameObject[] pls = null; 
    // Called when the class is created.
 //   GameObject targ = null;

    public override void Created()
    {
        pls = GameObject.FindGameObjectsWithTag("Player");

        if (PersonalNetworkAgent.Instance != null)
        {
            PersonalNetworkAgent.Instance.OnNewConnectionEvent += PersonalNetworkAgent_OnNewConnectionEvent;
        }
    }

    private void PersonalNetworkAgent_OnNewConnectionEvent(object sender, EventArgs e)
    {
        PersonalNetworkAgent.Instance.OnPlayerConnectedEvent += PersonalNetworkAgent_OnPlayerConnection;
        PersonalNetworkAgent.Instance.OnPlayerDisconnectedEvent += PersonalNetworkAgent_OnPlayerConnection;
    }

    private void PersonalNetworkAgent_OnPlayerConnection(object sender, System.EventArgs e)
    {
        pls = GameObject.FindGameObjectsWithTag("Player");

        Debug.Log("pls len: " + pls.Length);
    }

    // Called each frame. This can be used to gather data from the world before the sense method is called.
    // This can be used to gather 'base data' that is the same for all agents, and otherwise would be performed multiple times during the Sense method.
    public override void Update()
    {
        // if(pl.transform.hasChanged == true){
        //     pl = findClosestenemy();
        // }
    }

    

    // Called when the sensor needs to sense a target for a specific agent.
    public override ITarget Sense(IMonoAgent agent, IComponentReference references)
    {
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected && !PersonalNetworkAgent.Instance.isNetworkHost) return null;

        Vector3 mypos = agent.transform.position;
         GameObject closest = null;
       float distance = Mathf.Infinity;

        if (pls == null)
            return null;

        for(int i=0;i<pls.Length;){
            try
            {
                Vector3 diff = pls[i].transform.position - mypos;
                float curDistance = diff.sqrMagnitude;
                
                if (curDistance < distance)
                {
                    closest = pls[i];
                    distance = curDistance;
                }
                i++;
            } catch (Exception)
            {
                pls = GameObject.FindGameObjectsWithTag("Player");
            }
        }

        return new TransformTarget(closest.transform);  

        //   foreach (GameObject go in pls)
        // {
        //     // Vector3 diff = go.transform.position - mypos;
        //     // float curDistance = diff.sqrMagnitude;
        //     // Debug.Log(go.transform.position + "" + curDistance);
        //     // if (curDistance < distance)
        //     // {
        //     //     closest = go;
        //     //     distance = curDistance;
                
        //     // }
            
        //     // return new TransformTarget(closest.transform);
        // }  
        //  }
        //  return null;
    }
    }
