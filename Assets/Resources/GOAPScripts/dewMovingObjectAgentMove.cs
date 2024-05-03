using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

public class dewMovingObjectAgentMove : MonoBehaviour
{
    private AgentBehaviour agent;
    private ITarget currentTarget;
    private bool shouldMove;

   public Animator myAnimator;

    public GameObject pl; 

    public bool hasAnim = false;
    private void Awake()
    {
        this.agent = this.GetComponent<AgentBehaviour>();

        //new addition
        if(this.gameObject.GetComponent<Animator>() != null){
            this.myAnimator = this.GetComponent<Animator>();
            hasAnim=true;
        }
        //


        pl = GameObject.FindWithTag("Player");
    }
    private void OnEnable()
    {
        this.agent.Events.OnTargetInRange += this.OnTargetInRange;
        this.agent.Events.OnTargetChanged += this.OnTargetChanged;
        this.agent.Events.OnTargetOutOfRange += this.OnTargetOutOfRange;
    }

    private void OnDisable()
    {
        this.agent.Events.OnTargetInRange -= this.OnTargetInRange;
        this.agent.Events.OnTargetChanged -= this.OnTargetChanged;
        this.agent.Events.OnTargetOutOfRange -= this.OnTargetOutOfRange;
    }

    private void OnTargetInRange(ITarget target)
    {
        this.shouldMove = false;
    }

    private void OnTargetChanged(ITarget target, bool inRange)
    {
        this.currentTarget = target;
        this.shouldMove = !inRange;
    }

    private void OnTargetOutOfRange(ITarget target)
    {

        //dew new addition
        if(hasAnim){
            myAnimator.Play("Walk Forward In Place");
        }
       // myAnimator.Play("Walk Forward In Place");
      //  Debug.Log("playThis");  
        this.shouldMove = true;
       
    }

    public void Update()
    {
        if (!this.shouldMove)
            return;
        
        if (this.currentTarget == null)
            return;
        

        Vector3 targetDirection = this.currentTarget.Position - this.transform.position;
       float singleStep = 1.0f * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(this.transform.forward, targetDirection, singleStep, 0.0f);
        this.transform.rotation = Quaternion.LookRotation(newDirection);
        //dew update 2, lets make it not rotate in the Z dir 
       this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(this.currentTarget.Position.x, this.transform.position.y, this.currentTarget.Position.z), (Time.deltaTime) * 3);
    //this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(this.currentTarget.Position.x, this.transform.position.y, 0), (Time.deltaTime) * 3);

    }
}