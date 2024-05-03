using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class explosionlogic : MonoBehaviour
{

    [SerializeField] private bool isFake = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     private void OnCollisionEnter(Collision collision)
    {

       // Debug.Log("" + collision.gameObject.name);
        if (isFake)
        {
          // Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("Target"))
        {
           // print("hit " + collision.gameObject.name + " !");
         //   Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
           // print("hit a wall");
          //  Destroy(gameObject);
        }

        if (collision.gameObject.GetComponent<dewAgentHealth>() != null)
        {
            collision.gameObject.GetComponent<dewAgentHealth>().TakeDamage(WeaponModel.Dragon, -2);
           // Destroy(gameObject);

            // Rigidbody collisionRb = collision.gameObject.GetComponent<Rigidbody>();
            // if (collisionRb != null) collisionRb.velocity = Vector3.zero;
        }

        if (collision.gameObject.GetComponent<playerHealth>() != null)
        {
            collision.gameObject.GetComponent<playerHealth>().health -= 5;


            // Rigidbody collisionRb = collision.gameObject.GetComponent<Rigidbody>();
            // if (collisionRb != null) collisionRb.velocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider collision){
        //Debug.Log("Trigger is + " + collision.gameObject.name);
        if (isFake)
        {
          //  Destroy(gameObject);
            return;
        }


//destroys other enemies in scenes
        if (collision.gameObject.GetComponent<dewAgentHealth>() != null){
            collision.gameObject.GetComponent<dewAgentHealth>().TakeDamage(WeaponModel.Dragon, -2);

         //   Destroy(gameObject);
        }

        if (collision.gameObject.GetComponent<playerHealth>() != null)
        {
            collision.gameObject.GetComponent<playerHealth>().health -= 5;


            // Rigidbody collisionRb = collision.gameObject.GetComponent<Rigidbody>();
            // if (collisionRb != null) collisionRb.velocity = Vector3.zero;
        }
    }
}
