using System.Diagnostics;
using System.Linq;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using Mono.CSharp;
using Unity.Mathematics;
using UnityEngine;
using System;
public class ShootAction : ActionBase<ShootAction.Data>
{


    //NOTE 10/7 

    //i have the turning and shooting logic down
    //i dont have any animations tho, and i dont have any interactivity with other scripts
    
    //10/8 
    //shooting/turning works. need to see how thisll look on a 3d char. ill do a melee action
    // Called when the class is created.

    //10/12 
    //when I shoot a gun, I want the gun health to go down 
    //

    //10/30 can have iterator in currGunBehaviour script to reduce the number 
    public override void Created()
    {
    }

    // Called when the action is started for a specific agent.
    public override void Start(IMonoAgent agent, Data data)
    {   
        // after firing, wait a random time before shooting again
        data.Timer = UnityEngine.Random.Range(1f, 2f);
       // Debug.Log("shooting the player.");
       //Debug.Log(data.Timer);
       if(data.ac == null){
            data.ac = agent.GetComponentInChildren<AudioSource>();
        }

        rotateTowardsThis(agent, data);
    }

    // Called each frame when the action needs to be performed. It is only called when the agent is in range of it's target.
    public override ActionRunState Perform(IMonoAgent agent, Data data, ActionContext context)
    {
            // Update timer.
        data.Timer -= context.DeltaTime;
        
        // If the timer is still higher than 0, continue next frame.
        if (data.Timer > 0){
            //
            return ActionRunState.Continue;
        }        
        //I want to run the code for shoot here       
        fireLogic(agent, data);
        // This action is done, return stop. This will trigger the resolver for a new action.
        return ActionRunState.Stop;
    }

    // Called when the action is ended for a specific agent.
    public override void End(IMonoAgent agent, Data data)
    {
    }



    private void rotateTowardsThis(IMonoAgent agent, Data data){

        //rotation aint working. i think the logic why goes here. 
        Vector3 dir = (data.Target.Position - agent.transform.position).normalized;
       //Vector3 dir = (agent.transform.position - data.Target.Position ).normalized;
        quaternion lookRot = Quaternion.LookRotation(dir); 
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRot, Time.deltaTime * 100f);
    }
    private  void fireLogic(IMonoAgent agent, Data data){
        data.ac.Play();
        //So what should happen ideally? 
        //1. animator should lock in and rotate to player, then fire 
        //2. the gun should have some sort of animation/muzzle flash/smoke that appears
        //3. raycast should be firing and aimed towards the player
        //4. if there is a direct hit, damage should be dealt
        // work backwards 
      //  UnityEngine.Debug.Log("shoot!");
        //I have 3 down. How should 1. work. Well, no choice but to FINALLY include...... animations
        //UnityEngine.Debug.Log("Fire logic is running!");
        data.cgb.gunHP -=1;

        //old shoot logic here. its a little ugly, can xplain further
        //UnityEngine.Debug.DrawRay(agent.transform.position, agent.transform.TransformDirection(Vector3.forward)* 1000f, Color.red);
        //var rr = Physics.Raycast(agent.transform.position, agent.transform.TransformDirection(Vector3.forward), 1000f);
        //in order to send out a raycast, i need to have my object go out and shoot forward from the actual char
        // if(Physics.Raycast(agent.transform.position, agent.transform.TransformDirection(Vector3.forward), 1000f)){
        //      UnityEngine.Debug.Log("it hit");

       // RaycastHit hit;
       //UnityEngine.Debug.Log(data.Target.Position.ToString());
       //UnityEngine.Debug.DrawRay(agent.transform.position, agent.transform.TransformDirection(Vector3.forward)* 1000f, Color.red);
       
       if(agent.GetComponent<LineRenderer>()==null){
       LineRenderer ll = agent.gameObject.AddComponent<LineRenderer>();
       ll.widthMultiplier = .2f;
            ll.SetPosition(0,agent.transform.position + (Vector3.up * 1.3f));
       ll.SetPosition(1,data.Target.Position);
        }else{
            LineRenderer le = agent.GetComponent<LineRenderer>();
            le.SetPosition(0,agent.transform.position + (Vector3.up * 1.3f));
       le.SetPosition(1,data.Target.Position);
        }
      
       RaycastHit[] hits; 
       hits = Physics.RaycastAll(agent.transform.position, agent.transform.TransformDirection(Vector3.forward)* 1000f);
       UnityEngine.Debug.Log("" + hits.Length);
       if(hits.Length>0){ 
            foreach(RaycastHit hit in hits){
                //UnityEngine.Debug.Log("it hit");
                UnityEngine.Debug.Log("" + hit.collider.gameObject.name);  
                if(hit.collider.GetComponent<playerHealth>() != null){
                    int damageDealt = -1;
                    
                    var playerHealthScript = hit.collider.GetComponent<playerHealth>();
                    playerHealthScript.health += damageDealt;

                    if(hit.collider.GetComponent<UIStuff>() != null){
                    var playerHealthSlider = hit.collider.gameObject.GetComponent<UIStuff>();
                    playerHealthSlider.healthChange(damageDealt);
                    }

                    if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
                    {
                        var playerMovementScript = playerHealthScript.gameObject.GetComponent<movement>();

                        if (playerHealthScript.gameObject.GetComponent<movement>().playerId != PersonalNetworkAgent.Instance.GetUserNetworkId())
                        {
                            PersonalNetworkAgent.Instance.DamagePlayerOnNetwork(playerMovementScript.networkId);
                        }
                    }
                }
                else{
                UnityEngine.Debug.Log("it missed");
            }
       } 

        return; 
    }
    }
    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }
         [GetComponent]
        public currGunBehaviour cgb {get; set;}
        public AudioSource ac {get; set;}
    }
}