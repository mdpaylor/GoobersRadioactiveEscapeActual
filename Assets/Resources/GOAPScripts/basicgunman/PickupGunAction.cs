using System.Diagnostics;
using System.Linq;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;

public class PickupGunAction : ActionBase<PickupGunAction.Data>
{
    public override void Created()
     {
      }

    // Called when the action is started for a specific agent.
    public override void Start(IMonoAgent agent, Data data)
    {
        // after acting , wait a random time before acting again
        data.Timer = UnityEngine.Random.Range(1f, 2f);
       // Debug.Log("shooting the player.");
       //Debug.Log(data.Timer);
        rotateTowardsThis(agent, data);
    }

    // Called each frame when the action needs to be performed. It is only called when the agent is in range of it's target.
    public override ActionRunState Perform(IMonoAgent agent, Data data, ActionContext context)
    {
        //UnityEngine.Debug.Log("I am in run state");
        //theoretically, I can have the enemy turning animation run here. 
        // Update timer.
        data.Timer -= context.DeltaTime;
        
        // If the timer is still higher than 0, continue next frame.
        if (data.Timer > 0){
            //
            return ActionRunState.Continue;
        }        


        increaseGun(agent, data);
        // This action is done, return stop. This will trigger the resolver for a new action.
        return ActionRunState.Stop;
    }

    // Called when the action is ended for a specific agent.
    public override void End(IMonoAgent agent, Data data)
    {
    }



    private void rotateTowardsThis(IMonoAgent agent, Data data){
        Vector3 dir = (data.Target.Position - agent.transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir); 
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRot, Time.deltaTime * 10f);

    }
    private void increaseGun(IMonoAgent agent, Data data){
        data.cgb.gunHP +=4 ;
    }
    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }
        [GetComponent]
        public currGunBehaviour cgb {get; set;}
    }
}
