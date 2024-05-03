using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class playerInDetectionRadius : MonoBehaviour
{
    public bool playerinRadius;
    //public Transform pt;

    public float detectionRadius = 5f; // Radius within which players are detected


    public GameObject[] players;

    public Transform playerloc;

    private MovingObjectAgentMove movementScript;

    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        movementScript = GetComponent<MovingObjectAgentMove>();

        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
        {
            PersonalNetworkAgent.Instance.OnPlayerConnectedEvent += PersonalNetworkAgent_OnPlayerConnectionRelatedEvent;
            PersonalNetworkAgent.Instance.OnPlayerDisconnectedEvent += PersonalNetworkAgent_OnPlayerConnectionRelatedEvent;
        }
        // if(pt == null){
        //     pt = GameObject.FindGameObjectWithTag("Player").transform; 
        // }

    }

    private void PersonalNetworkAgent_OnPlayerConnectionRelatedEvent(object sender, EventArgs e)
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (movementScript != null && movementScript.isDummyObject) return;

        playerinRadius = IsPlayerInRadius();
    }

    // public bool detectPlayer(){
    //     //pt = GameObject.FindGameObjectWithTag("Player");
    //     // if(UnityEngine.Vector3.Distance(pt.position, transform.position) <= 20f){
    //     //     return true;
    //     // }else{
    //     //     return false;
    //     // }
    // }   
    
    public bool IsPlayerInRadius()
    {
        
        if (players.Length == 0)
        {
            playerloc = null;
            return false;
        }
        else if (players.Length != 2 && players.Length == 1)
        {
            playerloc = players[0].transform;

            return true;
        }
        try
        {
            float p1dist = UnityEngine.Vector3.Distance(players[0].transform.position, transform.position);
            float p2dist = UnityEngine.Vector3.Distance(players[1].transform.position, transform.position);

            if (p1dist < p2dist)
            {
                playerloc = players[0].transform;

                return true;
            }
            else
            {
                playerloc = players[1].transform;

                return true;
            }
        }
        catch (Exception)
        {
            try
            {
                if (players.Length > 0)
                {
                    playerloc = players[0].transform;

                    return true;
                }
                else
                {
                    playerloc = null;
                    return false;
                }
            }
            catch (Exception) { return false; }
        }
    }

    public void SetPlayerLoc(Transform objectTransform)
    {
        playerloc = objectTransform;
    }

    public void UnsubscribeFromEvents()
    {
        PersonalNetworkAgent.Instance.OnPlayerConnectedEvent -= PersonalNetworkAgent_OnPlayerConnectionRelatedEvent;
        PersonalNetworkAgent.Instance.OnPlayerDisconnectedEvent -= PersonalNetworkAgent_OnPlayerConnectionRelatedEvent;
    }
}    

