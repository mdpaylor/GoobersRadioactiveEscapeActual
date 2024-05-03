using System.Linq;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;

public class PerformMeleeAction : ActionBase<PerformMeleeAction.Data>
{
    // Called when the class is created.
    public override void Created()
    {
    }

    // Called when the action is started for a specific agent.
    public override void Start(IMonoAgent agent, Data data)
    {
        //check if i have a gun before i start firing out 
        //TEMP DISABLED!
        //What should happen - once the timer is over, the action starts 
        // if(data.hasGun == false){
        //     return;
        // }
        // after firing, wait a random time before shooting again
        data.Timer = Random.Range(1f, 2f);
    }

    // Called each frame when the action needs to be performed. It is only called when the agent is in range of it's target.
    public override ActionRunState Perform(IMonoAgent agent, Data data, ActionContext context)
    {
        // Update timer.
        data.Timer -= context.DeltaTime;
        
        // If the timer is still higher than 0, continue next frame.
        if (data.Timer > 0)
            return ActionRunState.Continue;
        
        //I want to run the code for shoot here 
        this.fireLogic();
        // This action is done, return stop. This will trigger the resolver for a new action.
        return ActionRunState.Stop;
    }

    // Called when the action is ended for a specific agent.
    public override void End(IMonoAgent agent, Data data)
    {
    }


    private  void fireLogic(){
        Debug.Log("Fire logic is running!");

    }
    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }

    }
}