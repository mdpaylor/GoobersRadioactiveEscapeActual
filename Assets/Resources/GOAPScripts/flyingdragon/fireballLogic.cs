using System.Collections;
using System.Collections.Generic;
using CrashKonijn.Goap.Interfaces;
using JetBrains.Annotations;
using UnityEngine;

public class fireballLogic : MonoBehaviour
{

    public GameObject fireball;

    public ITarget target; 

    public float speed = 1.0f;

    public GameObject expl;
    public bool started= false;

    public MeshRenderer mr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move our position a step closer to the target.
     //   var step =  speed * Time.deltaTime; // calculate distance to move
       // transform.position = Vector3.MoveTowards(transform.position, target.Position, step);

        // // Check if the position of the cube and sphere are approximately equal.
        // if (Vector3.Distance(transform.position, target.Position) < 0.001f)
        // {
        //     // Swap the position of the cylinder.
        //     //target.Position *= -1.0f;

        //    Destroy(this.gameObject);
        // }

        if(fireball.transform.position.y < 0){
           // Destroy(this.gameObject);

           startExplosion();
        }
    }

    void startExplosion(){
        if(started){
            return;
        }
        started=true;
        Rigidbody rg = GetComponent<Rigidbody>();
        rg.constraints=RigidbodyConstraints.FreezeAll;

        mr.enabled = false;
        expl.SetActive(true);
        if(expl!=null){
        Destroy(expl,1);
        }
        if(this!=null){
        Destroy(this, 1); 
        }

    }
}
