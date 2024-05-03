using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class seenoplayersendgame : MonoBehaviour
{

    public AISpawns mysp;
    
    private void Update()
    {
        if(mysp.runthismanytimes <= 0 ){
            if(GameObject.Find("GoapRedDragon")){
                return;
            }else{
                SceneManager.LoadScene(4);
            }
        }
        if (GameObject.FindGameObjectWithTag("Player")) {
            return; 
        } 
        else {
            if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
            {
                PersonalNetworkAgent.Instance.Disconnect();
            }

            endTheGame();
        }
    }

    private void endTheGame(){
        SceneManager.LoadScene(1);
    }
}
