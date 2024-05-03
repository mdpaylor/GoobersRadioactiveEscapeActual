using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;

public class summonerbrain : MonoBehaviour
{
    private AgentBehaviour agent;
    private newLOS losscript; 
    private dewAgentHealth myHealth;
    private MovingObjectAgentMove movementScript;

    private void Awake()
    {
        this.movementScript = this.GetComponent<MovingObjectAgentMove>();
        this.agent = this.GetComponent<AgentBehaviour>();
        this.losscript = this.GetComponent<newLOS>();
        this.myHealth = this.GetComponent<dewAgentHealth>(); 
    }

    private void OnEnable()
        {
            this.agent.Events.OnActionStop += this.OnActionStop;
            this.agent.Events.OnNoActionFound += this.OnNoActionFound;
            this.agent.Events.OnGoalCompleted += this.OnGoalCompleted;
        }

        private void OnDisable()
        {
            this.agent.Events.OnActionStop -= this.OnActionStop;
            this.agent.Events.OnNoActionFound -= this.OnNoActionFound;
            this.agent.Events.OnGoalCompleted -= this.OnGoalCompleted;
        }

    private void Start()
    {
        //ideally, should have 2 and only 2 goals
        //1. wander until a player is seen 
        //2. find the player and start summoning things that go towards
        this.agent.SetGoal<KillPlayerGoal>(false);
        this.agent.SetGoal<WanderGoal>(false);
    }

    private void OnNoActionFound(IGoalBase goal)
    {
        if (movementScript != null && movementScript.isDummyObject) return;

        this.agent.SetGoal<WanderGoal>(false);
    }

    private void OnActionStop(IActionBase action)
    {
        if (movementScript != null && movementScript.isDummyObject) return;

        if (this.agent.CurrentGoal is KillPlayerGoal) return;
            
        this.DetermineGoal();
    }

    private void DetermineGoal()
    {
        this.agent.SetGoal<KillPlayerGoal>(false);
        this.agent.SetGoal<WanderGoal>(false);
        
        if(this.losscript.seeingPlayer == true){
            this.agent.SetGoal<KillPlayerGoal>(true);
        }
    }

    private void OnGoalCompleted(IGoalBase goal)
    {
        if (movementScript != null && movementScript.isDummyObject) return;

        this.agent.SetGoal<KillPlayerGoal>(false);  

        DetermineGoal();
    }  


}