using System.Diagnostics;
using System.Linq;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using Unity.Mathematics;
using UnityEngine;
public class flankTargetAction : ActionBase<flankTargetAction.Data>
{


    public override void Created()
    {
    }

    // Called when the action is started for a specific agent.
    public override void Start(IMonoAgent agent, Data data)
    {   
        // after firing, wait a random time before shooting again
        data.Timer = UnityEngine.Random.Range(.2f, 1f);
       // Debug.Log("shooting the player.");
       //Debug.Log(data.Timer);

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
        fireAndStrafe(agent, data);
        // This action is done, return stop. This will trigger the resolver for a new action.
        return ActionRunState.Stop;
    }

    // Called when the action is ended for a specific agent.
    public override void End(IMonoAgent agent, Data data)
    {
    }



    private void rotateTowardsThis(IMonoAgent agent, Data data){
        Vector3 dir = (data.Target.Position - agent.transform.position).normalized;
        quaternion lookRot = Quaternion.LookRotation(dir); 
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRot, Time.deltaTime * 10f);
    }
    private  void fireAndStrafe(IMonoAgent agent, Data data){
        //So what should happen ideally? 
        //1. animator should lock in and rotate to player, then fire 
        //2. the gun should have some sort of animation/muzzle flash/smoke that appears
        //3. raycast should be firing and aimed towards the player
        //4. if there is a direct hit, damage should be dealt
        // work backwards 
        //UnityEngine.Debug.Log("shoot!");
        //I have 3 down. How should 1. work. Well, no choice but to FINALLY include...... animations
        //UnityEngine.Debug.Log("Fire logic is running!");
        var random =  UnityEngine.Random.insideUnitCircle * 10f;
        agent.transform.position = agent.transform.position + new Vector3(random.x, 0f, random.y);
        
        data.cgb.gunHP -=1;
        UnityEngine.Debug.DrawRay(agent.transform.position, agent.transform.TransformDirection(Vector3.forward)* 1000f, Color.red);
        //in order to send out a raycast, i need to have my object go out and shoot forward from the actual char
        if(Physics.Raycast(agent.transform.position, agent.transform.TransformDirection(Vector3.forward), 1000f, 1)){
           // UnityEngine.Debug.Log("it hit");
        }else{
           // UnityEngine.Debug.Log("It missed");
        }
        return; 
    }
    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }
         [GetComponent]
        public currGunBehaviour cgb {get; set;}
    }
}