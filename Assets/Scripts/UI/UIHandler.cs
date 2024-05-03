using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{

    [SerializeField] private Button joinButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button showNetworkMapButton;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject personalNetworkAgentPrefab;
    [SerializeField] private GameObject networkManagerPrefab;
    [SerializeField] private GameObject summonerPrefab;
    [SerializeField] private bool canSpawnObject;
    
    void Start()
    {
        joinButton.onClick.AddListener(() => {
            JoinButtonLogic();
        });
        quitButton.onClick.AddListener(() => {
            QuitButtonLogic();
        });
        showNetworkMapButton.onClick.AddListener(() =>
        {
            ShowNetworkMapLogic();
        });
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            JoinButtonLogic();
        }
        else if (Input.GetKeyUp(KeyCode.O))
        {
            QuitButtonLogic();
        }
        else if (Input.GetKeyUp(KeyCode.M))
        {
            ShowNetworkMapLogic();
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (!canSpawnObject) return;

            if (PersonalNetworkAgent.Instance == null || !PersonalNetworkAgent.Instance.isNetworkHost) return;

            Vector3 position = new Vector3(5.58f, 0.07f, -27.6f);

            GameObject reference = Instantiate(summonerPrefab, position, Quaternion.identity);

            Transform referenceTransform = reference.transform;

            NetworkManager.Instance.AddObjectToSpawnOnNetwork(reference, "Summoner", position, referenceTransform.rotation.eulerAngles);
        }
    }

    private void JoinButtonLogic()
    {
        if (PersonalNetworkAgent.Instance != null) return;
        //Instantiate(networkManagerPrefab);
        Instantiate(personalNetworkAgentPrefab);
    }

    private void QuitButtonLogic()
    {
        try
        {
            if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected)
            {
                PersonalNetworkAgent.Instance.Disconnect();
                Application.Quit(0);
            }
            else return;
        }
        catch (Exception e) { Debug.Log("Exception...\n" + e.Message); }
    }

    private void ShowNetworkMapLogic()
    {
        foreach (var reference in NetworkManager.Instance.GetNetworkObjectMap())
        {
            Debug.Log("ID: " + reference.Key + ", Value: " + reference.Value);
        }
    }
}
