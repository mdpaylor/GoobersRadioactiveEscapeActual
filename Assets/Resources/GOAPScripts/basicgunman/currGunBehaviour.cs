using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class currGunBehaviour : MonoBehaviour
{
    

    public int gunHP =0; 
    
    public int gunID = 0;

    public void increaseGunHP(){

    }

    public int retGunID(){
      return gunID;
    }

    public void resetGunHP(){
      gunHP = 5;
    }

    public void decreaseGunCon(){
      gunID--;
    }
      private void Awake()
        {
           //logic that would start when code when behavuiour is called into scene
           //need to have some time to 

        }

        private void FixedUpdate()
        {
            //logic called per frame
        }
}
