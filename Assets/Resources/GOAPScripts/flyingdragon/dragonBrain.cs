using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using UnityEngine;

public class dragonBrain : MonoBehaviour
{
    private AgentBehaviour agent;
    //private newLOS losscript; 
    private dragAgentHealth myHealth;
    private MovingObjectAgentMove movementScript;

    

    private void Awake()
    {
        this.movementScript = this.GetComponent<MovingObjectAgentMove>();
        this.agent = this.GetComponent<AgentBehaviour>();
      //  this.losscript = this.GetComponent<newLOS>();
        this.myHealth = this.GetComponent<dragAgentHealth>(); 
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
        this.agent.SetGoal<KillPlayerGoal>(false);
       // this.agent.SetGoal<WanderGoal>(false);
    }

    private void OnNoActionFound(IGoalBase goal)
    {
        if (movementScript != null && movementScript.isDummyObject) return;

       // this.agent.SetGoal<WanderGoal>(false);
    }

    private void OnActionStop(IActionBase action)
    {
        if (movementScript != null && movementScript.isDummyObject) return;

        if (this.agent.CurrentGoal is KillPlayerGoal)
            return;
            
        this.DetermineGoal();
    }

    private void DetermineGoal()
    {
        this.agent.SetGoal<KillPlayerGoal>(true);  
        //this.agent.SetGoal<WanderGoal>(false);



        // if(myHealth.hp < 10){
        //     this.agent.SetGoal<preserveLifeGoal>(true);
        // }
            // if (this.agent.LOSWorldKey)
            // {
            //     this.agent.SetGoal<PickupItemGoal<Pickaxe>>(false);
            //     return;
            // }
            
            // if (this.itemCollection.Get<Iron>().Length <= 2)
            // {
            //     this.agent.SetGoal<GatherItemGoal<Iron>>(false);
            //     return;
            //}
        // if(this.losscript.seeingPlayer == true){
        //     this.agent.SetGoal<KillPlayerGoal>(true);
        // }    


    }

    private void OnGoalCompleted(IGoalBase goal)
    {
        if (movementScript != null && movementScript.isDummyObject) return;

        this.agent.SetGoal<KillPlayerGoal>(false);  

        DetermineGoal();
    }  


}