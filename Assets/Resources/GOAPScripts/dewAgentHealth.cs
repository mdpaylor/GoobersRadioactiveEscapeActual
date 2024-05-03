using System.Collections;
using System.Collections.Generic;
using QFSW.QC.Actions;
using UnityEngine;
using static Weapon;
using System;

public class dewAgentHealth : MonoBehaviour
{
    public int hp = 10;

    private GameObject ScoreKeeper;
    public int pointsUponDeath = 1;

    public bool playing = false;
    public ParticleSystem myparts;
    public int lastIdToDamageSelf = -1;

    [SerializeField]
    private MovingObjectAgentMove movementScript;

    private bool isDead = false;
    
    public void Awake()
    {
        ScoreKeeper = GameObject.Find("PlayerScore");
    }
    
    public void Update(){
        if(hp <= 0 && !isDead){
            isDead = true;
            //play particle effect system
            //dissapear 

            var scoreScript = ScoreKeeper.GetComponent<PlayerScore>();

            if (gameObject.GetComponent<ParticleSystem>() != null && playing==false){
                myparts.Play();
                playing = true;
            }

            
            if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
            {
                if (movementScript == null)
                {
                    movementScript = gameObject.GetComponent<MovingObjectAgentMove>();
                }

                try { 
                    movementScript.UnsubscribeFromEvents(); 
                } catch (Exception) {}

                PersonalNetworkAgent.Instance.DeleteObjectOnNetwork(movementScript.networkId);

                if (lastIdToDamageSelf == PersonalNetworkAgent.Instance.GetUserNetworkId()) scoreScript.addToPlayerScore(pointsUponDeath);
            }
            else scoreScript.addToPlayerScore(pointsUponDeath);
            Destroy(gameObject, 1);
        }
    }

    public void TakeDamage(WeaponModel weapon, int playerId)
    {
        string weaponString = "";
        if (playerId != -2) lastIdToDamageSelf = playerId;

        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                hp = hp - 50;
                weaponString = "pistol";
                break;

            case WeaponModel.AK47:
                hp = hp - 30;
                weaponString = "ak74";
                break;

            case WeaponModel.BM4:
                hp = hp - 250;
                weaponString = "bm4";
                break;

            case WeaponModel.Uzi:
                hp = hp - 20;
                weaponString = "uzi";
                break;
            case WeaponModel.Dragon:
                hp = hp - 100;
                weaponString = "dragon";
                break;
        }

        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected && NetworkManager.Instance != null)
        {
            NetworkManager.Instance.AddEnemyThatTookDamage(movementScript.networkId, weaponString, playerId);
        }
    }

    public void TakeDamageFromNetwork(string weaponString, int playerId)
    {
        if (playerId != -2) lastIdToDamageSelf = playerId;

        switch (weaponString)
        {
            case "pistol":
                hp = hp - 50;
                break;
           case "ak74":
                hp = hp - 30;
                break;
            case "bm4":
                hp = hp - 250;
                break;
            case "uzi":
                hp = hp - 20;
                break;
            case "dragon":
                hp = hp - 100;
                break;
        }
    }

    public void KillSelfFromNetwork()
    {
        hp = 0;
    }
}


