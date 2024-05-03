using System.Collections;
using System.Collections.Generic;
using CrashKonijn.Goap.Classes.References;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class playerHealth : MonoBehaviour
{
    public orientation orientationScript;
    public int health = 10;

    [SerializeField] private bool isDummy = false;

    private GameObject deathCam;
    private GameObject personalReference;

    private movement movementScript;

    void Start()
    {
        personalReference =  this.gameObject;

        movementScript = personalReference.GetComponent<movement>();

        Debug.Log("canControl: "+ movementScript.CanControlObject());

        if (deathCam == null && movementScript.CanControlObject()) {
            Debug.Log("Entered");
            deathCam = GameObject.FindGameObjectWithTag("DeathCam");

            if (deathCam != null) deathCam.SetActive(false);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (isDummy) return;

        if (health <= 0){
            runDeath();
        }

        if (deathCam == null && movementScript.CanControlObject())
        {
            Debug.Log("Entered");
            deathCam = GameObject.FindGameObjectWithTag("DeathCam");

            if (deathCam != null) deathCam.SetActive(false);
        }
    }

    private void runDeath(){
        if (deathCam != null)
        {
            deathCam.SetActive(true);
            deathCam.GetComponent<Camera>().enabled = true;
        }
        
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
        {
            PersonalNetworkAgent.Instance.KillSelfOnNetwork();
            gameObject.GetComponent<movement>().UnsubscribeFromEvents();
        }

        Destroy(personalReference);
    }
}
