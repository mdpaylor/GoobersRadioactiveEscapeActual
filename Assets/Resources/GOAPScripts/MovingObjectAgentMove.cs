using CrashKonijn.Goap.Behaviours;
using CrashKonijn.Goap.Interfaces;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class MovingObjectAgentMove : MonoBehaviour
{
    private AgentBehaviour agent;
    private ITarget currentTarget;
    private bool shouldMove;

    public Animator myAnimator;

    public GameObject pl; 

    public bool hasAnim = false;

    // Network Variables
    [SerializeField]
    private newLOS losScript;
    [SerializeField]
    private playerInDetectionRadius playerInDetectionRadiusScript;

    public int networkId = -1;
    public bool isDummyObject = false;
    public string prefabName = "";

    private int currentTargetNetworkId = -1;

    private ParticleSystem personalParticleSystem;
    private NetworkTypes.NetworkObject selfReference;
    private float timeBeforePositionCorrection = 3f;
    private float passedTimeBeforePositionCorrection = 0f;

    private void Awake()
    {
        this.agent = this.GetComponent<AgentBehaviour>();
        personalParticleSystem = GetComponent<ParticleSystem>();

        //new addition
        if(this.gameObject.GetComponent<Animator>() != null){
            this.myAnimator = this.GetComponent<Animator>();
            hasAnim=true;
        }

        pl = GameObject.FindWithTag("Player");
    }

    private void PersonalNetworkAgent_OnPlayerDisconnected(object sender, System.EventArgs e)
    {
        pl = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        if (NetworkManager.Instance != null)
        {
            selfReference = NetworkManager.Instance.FindGameObjectInNetworkMap(networkId);

            NetworkManager.Instance.OnFixedNetworkUpdateEvent += NetworkManager_OnFixedNetworkUpdate;
        }
        if (PersonalNetworkAgent.Instance != null)
        {
            PersonalNetworkAgent.Instance.OnPlayerDisconnectedEvent += PersonalNetworkAgent_OnPlayerDisconnected;
        }
    }

    private void NetworkManager_OnFixedNetworkUpdate(object sender, System.EventArgs e)
    {
        if (isDummyObject) return;

        Vector3 position = transform.position;
        Vector3 rotation = transform.rotation.eulerAngles;

        NetworkManager.Instance.AddChangedEnemyObject(networkId, currentTargetNetworkId, position, rotation, shouldMove);
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
        if (isDummyObject) return;

        this.shouldMove = false;
    }

    private void OnTargetChanged(ITarget target, bool inRange)
    {
        if (isDummyObject) return;

        this.currentTarget = target;
        this.shouldMove = !inRange;
    }

    private void OnTargetOutOfRange(ITarget target)
    {
        if (isDummyObject) return;

        //dew new addition
        if (hasAnim){
            myAnimator.Play("Walk Forward In Place");

            if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
            {
                NetworkManager.Instance.AddEnemyThatChangedAnimationState(networkId, "Walk Forward In Place");
            }
        }
       // myAnimator.Play("Walk Forward In Place");
      //  Debug.Log("playThis");  
        this.shouldMove = true;
       
    }

    public void Update()
    {
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected && !PersonalNetworkAgent.Instance.isNetworkHost)
        {
            NetworkMovementUpdate();
            return;
        }

        if (!this.shouldMove)
            return;
        
        if (this.currentTarget == null)
            return;
        

        Vector3 targetDirection = this.currentTarget.Position - this.transform.position;
        float singleStep = 1.0f * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(this.transform.forward, targetDirection, singleStep, 0.0f);
       
       //dew change 3/31 might not be needed
       //  Vector3 newDirection = Vector3.RotateTowards(this.transform.forward, new Vector3(0, targetDirection.y, 0), singleStep, 0.0f);

        this.transform.rotation = Quaternion.LookRotation(newDirection);
        this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(this.currentTarget.Position.x, this.transform.position.y, this.currentTarget.Position.z), (Time.deltaTime) * 3);
    }

    private void NetworkMovementUpdate()
    {
        try
        {
            this.passedTimeBeforePositionCorrection += Time.deltaTime;
            if (this.passedTimeBeforePositionCorrection >= this.timeBeforePositionCorrection)
            {
                this.passedTimeBeforePositionCorrection -= this.timeBeforePositionCorrection;

                this.transform.position = selfReference.actualPosition;
                this.transform.rotation = Quaternion.Euler(selfReference.actualRotation);
                return;
            }

            float distance = Vector3.Distance(transform.position, selfReference.actualPosition);

            Vector3 targetDirection = this.selfReference.actualPosition - this.transform.position;
            float singleStep = 1.0f * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(this.transform.forward, targetDirection, singleStep, 0.0f);
            this.transform.rotation = Quaternion.LookRotation(newDirection);
            this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(selfReference.actualPosition.x, this.transform.position.y, selfReference.actualPosition.z), distance / NetworkManager.Instance.finalNetworkTime / Time.deltaTime);
        }
        catch (Exception) {}
    }

    public void SetDataFromNetwork(int currentTargetId, bool shouldMove)
    {
        this.shouldMove = shouldMove;
        this.currentTargetNetworkId = currentTargetId;
    }

    public void SetIsDummyObject(bool condition)
    {
        isDummyObject = condition;
    }

    public void ActivateAnimation(string animationName)
    {
        myAnimator.Play(animationName);
    }

    public void SetParticles(bool condition)
    {
        if (personalParticleSystem == null)
        {
            Debug.Log("Was null");
            return;
        }

        Debug.Log("Setting particles");

        if (condition)
        {
            try
            {
                personalParticleSystem.Play();

                Debug.Log("Playing - "+ Time.time);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        else
        {
            try
            {
                personalParticleSystem.Stop();
                Debug.Log("Stopping - "+ Time.time);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
    public void UnsubscribeFromEvents()
    {
        try
        {
            NetworkManager.Instance.OnFixedNetworkUpdateEvent -= NetworkManager_OnFixedNetworkUpdate;
        } catch (Exception) {}
        try
        {
            if (losScript != null) losScript.UnsubscribeFromEvents();
        } catch (Exception) {}
        try
        {
            if (playerInDetectionRadiusScript != null) playerInDetectionRadiusScript.UnsubscribeFromEvents();
        } catch (Exception) {}
    }
}