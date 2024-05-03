using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class summonBehavior : MonoBehaviour
{
    public GameObject summonthisEnemy;
    
    public ParticleSystem myParticles;

    public void startParticles(){
        myParticles.Play();
    }

    public void stopParticles(){
        myParticles.Stop();
    }

    public GameObject myEnemy(){
        return summonthisEnemy;
    }
      private void Awake()
        {
           

        }

        private void FixedUpdate()
        {
            //logic called per frame
        }
}
