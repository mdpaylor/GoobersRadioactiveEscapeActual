using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class Bullet : MonoBehaviour
{

    [SerializeField] private bool isFake = false;

    public WeaponModel originWeapon;
    public int playerId = -1;

    private void OnCollisionEnter(Collision objectWeHit)
    {
        if (isFake)
        {
            Destroy(gameObject);
            return;
        }

        if (objectWeHit.gameObject.CompareTag("Target"))
        {
            print("hit " + objectWeHit.gameObject.name + " !");

            CreateBulletImpactEffect(objectWeHit);

            Destroy(gameObject);
        }

        if (objectWeHit.gameObject.CompareTag("Wall"))
        {
            print("hit a wall");

            CreateBulletImpactEffect(objectWeHit);

            Destroy(gameObject);
        }

        if (objectWeHit.gameObject.GetComponent<dewAgentHealth>() != null)
        {
            Rigidbody collisionRb = objectWeHit.gameObject.GetComponent<Rigidbody>();
            if (collisionRb != null) collisionRb.velocity = Vector3.zero;

            objectWeHit.gameObject.GetComponent<dewAgentHealth>().TakeDamage(originWeapon, playerId);
            Destroy(gameObject);
        }

        if (objectWeHit.gameObject.GetComponent<dragAgentHealth>() != null)
        {
            Rigidbody collisionRb = objectWeHit.gameObject.GetComponent<Rigidbody>();
            if (collisionRb != null) collisionRb.velocity = Vector3.zero;

            objectWeHit.gameObject.GetComponent<dragAgentHealth>().TakeDamage(originWeapon, playerId);

            Destroy(gameObject);
        }
    }

    void CreateBulletImpactEffect(Collision objectWeHit)
    {
        ContactPoint contact = objectWeHit.contacts[0];

        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)

            ) ;

        hole.transform.SetParent(objectWeHit.gameObject.transform);
    }

    private void OnTriggerEnter(Collider collision){
        if (isFake)
        {
            Destroy(gameObject);
            return;
        }

        if (collision.gameObject.GetComponent<dewAgentHealth>() != null)
        {
            Rigidbody collisionRb = collision.gameObject.GetComponent<Rigidbody>();
            if (collisionRb != null) collisionRb.velocity = Vector3.zero;

            collision.gameObject.GetComponent<dewAgentHealth>().TakeDamage(originWeapon, playerId);

            Destroy(gameObject);
        }

        if (collision.gameObject.GetComponent<dragAgentHealth>() != null)
        {
            Rigidbody collisionRb = collision.gameObject.GetComponent<Rigidbody>();
            if (collisionRb != null) collisionRb.velocity = Vector3.zero;

            collision.gameObject.GetComponent<dragAgentHealth>().TakeDamage(originWeapon, playerId);

            Destroy(gameObject);
        }
    }
}
