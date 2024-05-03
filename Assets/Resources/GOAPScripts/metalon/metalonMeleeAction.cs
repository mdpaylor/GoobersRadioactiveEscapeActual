using System.ComponentModel.Design;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;
using System;

public class metalonMeleeAction : ActionBase<metalonMeleeAction.Data>
{
    // Called when the class is created.
    public GameObject thisobject;
    public override void Created()
    {
        //thisobject = this;
    }

    // Called when the action is started for a specific agent.
    public override void Start(IMonoAgent agent, Data data)
    {
        var movementScript = agent.GetComponent<MovingObjectAgentMove>();
        if (movementScript != null && movementScript.isDummyObject) return;

        // When the agent is at the target, wait a random amount of time before moving again.
        data.Timer = UnityEngine.Random.Range(.8f, 1f);
        //here we would have the actual melee attack logic. 11/14 lets do this. 
        //Animator aa = this.GetComponent<Animator>();

        
    }

    // Called each frame when the action needs to be performed. It is only called when the agent is in range of it's target.
    public override ActionRunState Perform(IMonoAgent agent, Data data, ActionContext context)
    {
        var movementScript = agent.GetComponent<MovingObjectAgentMove>();
        if (movementScript != null && movementScript.isDummyObject) return ActionRunState.Stop;

        // Update timer.
        data.Timer -= context.DeltaTime;
        
        // If the timer is still higher than 0, continue next frame.
        if (data.Timer > 0)
            return ActionRunState.Continue;
        

      //  Debug.Log("Time to attack!");
        

        //rotation logic moved to movement script
        //attack code should happen now 
        data.actingAnimator.Play("Smash Attack");
        Collider[] hitColliders = Physics.OverlapSphere(agent.transform.position, 3f);
        foreach (var hitCollider in hitColliders)
        {
            if(hitCollider.CompareTag("Player") == true){
                int damageDealt = -1;
                
                var playerHealthScript = hitCollider.GetComponent<playerHealth>();

                if (playerHealthScript != null)
                {
                    playerHealthScript.health += damageDealt;

                    if(hitCollider.gameObject.GetComponentInChildren<UIStuff>() != null){
                    var playerHealthSlider = hitCollider.gameObject.GetComponentInChildren<UIStuff>();
                    playerHealthSlider.healthChange(damageDealt);
                    }
                }

                if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
                {
                    var playerMovementScript = hitCollider.gameObject.GetComponent<movement>();

                    if (hitCollider.gameObject.GetComponent<movement>().playerId != PersonalNetworkAgent.Instance.GetUserNetworkId())
                    {
                        PersonalNetworkAgent.Instance.DamagePlayerOnNetwork(playerMovementScript.networkId);
                    }
                }

                Debug.Log("Metalon attacked and reduced player health");
            }else{
                Debug.Log("Metalon missed");
            }
        }
        // This action is done, return stop. This will trigger the resolver for a new action.

        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
        {
            if (movementScript != null) NetworkManager.Instance.AddEnemyThatChangedAnimationState(movementScript.networkId, "Smash Attack");
        }

        return ActionRunState.Stop;
    }

    // Called when the action is ended for a specific agent.
    public override void End(IMonoAgent agent, Data data)
    {
      //  Animator thisAnimator = data.actingAnimator.GetComponent<Animator>();
        var movementScript = agent.GetComponent<MovingObjectAgentMove>();
        if (movementScript != null && movementScript.isDummyObject) return;

        if(data.actingAnimator != null){
            try
            {
                data.actingAnimator.Play("Idle");
            }
            catch (Exception) {}
            if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
            {
                if (movementScript != null) NetworkManager.Instance.AddEnemyThatChangedAnimationState(movementScript.networkId, "Idle");
            }
        }
    }

    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }

        [GetComponent]
        public Animator actingAnimator {get; set;}

        public BoxCollider attackcollider;
    }
}