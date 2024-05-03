using System.Collections;
using System.Diagnostics;
using System.Linq;
using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Classes;
using CrashKonijn.Goap.Classes.References;
using CrashKonijn.Goap.Enums;
using CrashKonijn.Goap.Interfaces;
using Unity.Mathematics;
using UnityEngine;
public class actionsummonenemy : ActionBase<actionsummonenemy.Data>
{
 
    public override void Created()
    {
    }

    // Called when the action is started for a specific agent.
    public override void Start(IMonoAgent agent, Data data)
    {   
        if(data.actingAnimator == null){
            data.actingAnimator = agent.GetComponentInChildren<Animator>();

        }

        if(data.ac == null){
            data.ac = agent.GetComponentInChildren<AudioSource>();
        }
        // after firing, wait a random time before summoning again 
        data.Timer = UnityEngine.Random.Range(3f, 5f);
        data.cgb.startParticles();
        //UnityEngine.Debug.Log("summoning moster.");

        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
        {
            var movementScript = agent.gameObject.GetComponent<MovingObjectAgentMove>();

            NetworkManager.Instance.AddEnemyThatUsedParticleSystem(movementScript.networkId, true);

            UnityEngine.Debug.Log("Sent particles");
        }

        rotateTowardsThis(agent, data);
    }

    // Called each frame when the action needs to be performed. It is only called when the agent is in range of it's target.
    public override ActionRunState Perform(IMonoAgent agent, Data data, ActionContext context)
    {
        var movementScript = agent.GetComponent<MovingObjectAgentMove>();

        if (movementScript != null && movementScript.isDummyObject) return ActionRunState.Stop;

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
        var movementScript = agent.GetComponent<MovingObjectAgentMove>();

        if (movementScript != null && movementScript.isDummyObject) return;

        if (data.actingAnimator != null){
             data.actingAnimator.Play("Walk Forward In Place");
        }
        data.cgb.stopParticles();

        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
        {
            if (movementScript != null)
            {
                NetworkManager.Instance.AddEnemyThatUsedParticleSystem(movementScript.networkId, false);
                if (data.actingAnimator != null) NetworkManager.Instance.AddEnemyThatChangedAnimationState(movementScript.networkId, "Walk Forward In Place");

                UnityEngine.Debug.Log("Sent Particles");
            }
        }
    }

    private void rotateTowardsThis(IMonoAgent agent, Data data){
        var movementScript = agent.GetComponent<MovingObjectAgentMove>();
        
        if (movementScript != null && movementScript.isDummyObject) return;

        Vector3 dir = (data.Target.Position - agent.transform.position).normalized;
        quaternion lookRot = Quaternion.LookRotation(dir); 
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRot, Time.deltaTime * 10f);
    }
    //NOTE: ITS CALLED FIRE LOGIC,BUT IT ACTUALLY JUST SUMMONS AN ENEMY 
    //technically they dont need to rotate towards the player, but it all makes sense in the end 

    private  void fireLogic(IMonoAgent agent, Data data){
        //1. rotate towards the player
        //2. do a short casting animation that takes x seconds 
        //3. particle effect appears, enemy appears 
        //4. walks a bit, then does it again

        var movementScript = agent.gameObject.GetComponent<MovingObjectAgentMove>();

        if (movementScript != null && movementScript.isDummyObject) return;

       // UnityEngine.Debug.Log("summon logic started!!");

        AudioClip audioClip =  AudioClip.Create("summonnoise.wav",44100,2,44100,false);
        if  (audioClip == null){
           UnityEngine.Debug.Log("its null") ;
        }
        data.ac.PlayOneShot(audioClip);
        
        if(data.ac.isPlaying){
        //    UnityEngine.Debug.Log("Nah its playing alright");
        }

//        UnityEngine.Debug.Log(audioClip.name);    
        data.ac.Play();
        //TODO, this is where id put the animation for casting start 
        data.actingAnimator.Play("charge");
        pauseforaSecond(5);
        
        var objectReference = GameObject.Instantiate(data.cgb.summonthisEnemy, agent.transform.position, quaternion.identity);

//dew added line of code to make sure it doesnt activate a null instance
//added && objectReference != null
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected && objectReference != null)
        {
            Transform objectTransform = objectReference.transform;

            NetworkManager.Instance.AddObjectToSpawnOnNetwork(objectReference, objectReference.GetComponent<MovingObjectAgentMove>().prefabName, objectTransform.position, objectTransform.rotation.eulerAngles);
            NetworkManager.Instance.AddEnemyThatChangedAnimationState(movementScript.networkId, "charge");
        }

        return; 
    }
    public class Data : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }
         [GetComponent]
        public summonBehavior cgb {get; set;}

        public Animator actingAnimator {get; set;}

        public AudioSource ac {get; set;}
    }

    IEnumerator pauseforaSecond(int sec){
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(5);
    }
}
