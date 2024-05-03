using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentGun : MonoBehaviour
{
    [SerializeField]
    private movement movementScript;

    [SerializeField]
    private GameObject currGun;
    
    public GameObject pistol;
    public GameObject ak74;
    public GameObject bm4;
    public GameObject uzi;

    public string currGunString = "pistol";
    public string previousCurrGunString = "";

    private GameObject ScoreKeeper;

    public int unlockScore = 250;
    private int unlockAK74Score;
    private int unlockBM4Score;
    private int unlockUziScore;

    public void Awake()
    {
        ScoreKeeper = GameObject.Find("PlayerScore");
    }

    void Start()
    {
        currGun = pistol;

        unlockAK74Score = unlockScore * 1;
        unlockBM4Score = unlockScore * 2;
        unlockUziScore = unlockScore * 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected && !movementScript.CanControlObject()) return;

        var scoreScript = ScoreKeeper.GetComponent<PlayerScore>();

        if (scoreScript.getPlayerScore() >= unlockUziScore)
        {
            if (Input.GetMouseButtonUp(1) && currGun == pistol) // AK74 requires 250 points
            {
                currGun = ak74;
                currGunString = "ak74";

                pistol.SetActive(false);
                ak74.SetActive(true);
                bm4.SetActive(false);
                uzi.SetActive(false);
            }
            else if (Input.GetMouseButtonUp(1) && currGun == ak74) // Shotgun requires 500 points
            {
                currGun = bm4;
                currGunString = "bm4";

                pistol.SetActive(false);
                ak74.SetActive(false);
                bm4.SetActive(true);
                uzi.SetActive(false);
            }
            else if (Input.GetMouseButtonUp(1) && currGun == bm4) // Uzi requires 750 points
            {
                currGun = uzi;
                currGunString = "uzi";

                pistol.SetActive(false);
                ak74.SetActive(false);
                bm4.SetActive(false);
                uzi.SetActive(true);
            }
            else if (Input.GetMouseButtonUp(1) && currGun == uzi)
            {
                currGun = pistol;
                currGunString = "pistol";

                pistol.SetActive(true);
                ak74.SetActive(false);
                bm4.SetActive(false);
                uzi.SetActive(false);
            }
            else;
        }
        else if (scoreScript.getPlayerScore() >= unlockBM4Score)
        {
            if (Input.GetMouseButtonUp(1) && currGun == pistol) // AK74 requires 250 points
            {
                currGun = ak74;
                currGunString = "ak74";

                pistol.SetActive(false);
                ak74.SetActive(true);
                bm4.SetActive(false);
                uzi.SetActive(false);
            }
            else if (Input.GetMouseButtonUp(1) && currGun == ak74) // Shotgun requires 500 points
            {
                currGun = bm4;
                currGunString = "bm4";

                pistol.SetActive(false);
                ak74.SetActive(false);
                bm4.SetActive(true);
                uzi.SetActive(false);
            }
            else if (Input.GetMouseButtonUp(1) && currGun == bm4) // Uzi requires 750 points
            {
                currGun = pistol;
                currGunString = "pistol";

                pistol.SetActive(true);
                ak74.SetActive(false);
                bm4.SetActive(false);
                uzi.SetActive(false);
            }
            else;
        }
        else if (scoreScript.getPlayerScore() >= unlockAK74Score)
        {
            if (Input.GetMouseButtonUp(1) && currGun == pistol) // AK74 requires 250 points
            {
                currGun = ak74;
                currGunString = "ak74";

                pistol.SetActive(false);
                ak74.SetActive(true);
                bm4.SetActive(false);
                uzi.SetActive(false);
            }
            else if (Input.GetMouseButtonUp(1) && currGun == ak74) // Shotgun requires 500 points
            {
                currGun = pistol;
                currGunString = "pistol";

                pistol.SetActive(true);
                ak74.SetActive(false);
                bm4.SetActive(false);
                uzi.SetActive(false);
            }
            else;
        }
        else
        {
            currGun = pistol;
            currGunString = "pistol";

            pistol.SetActive(true);
            ak74.SetActive(false);
            bm4.SetActive(false);
            uzi.SetActive(false);
        }

        if (PersonalNetworkAgent.Instance != null && PersonalNetworkAgent.Instance.isConnected && !movementScript.isDummyObject)
        {
            if (previousCurrGunString != currGunString)
            {
                previousCurrGunString = currGunString;
                PersonalNetworkAgent.Instance.SwapWeapon(movementScript.networkId, currGunString);
                Debug.Log("Sent Swap Weapon");
            }
        }
    }

    public GameObject GetCurrentGun()
    {
        return currGun;
    }

    public void SetGunFromNetwork(string weaponString)
    {
        Debug.Log("Literally Switching: "+ weaponString);
        switch (weaponString)
        {
            case "pistol":
                currGun = pistol;

                pistol.SetActive(true);
                ak74.SetActive(false);
                bm4.SetActive(false);
                uzi.SetActive(false);
                break;
            case "ak74":
                currGun = ak74;

                pistol.SetActive(false);
                ak74.SetActive(true);
                bm4.SetActive(false);
                uzi.SetActive(false);
                break;
            case "bm4":
                currGun = bm4;

                pistol.SetActive(false);
                ak74.SetActive(false);
                bm4.SetActive(true);
                uzi.SetActive(false);
                break;
            case "uzi":
                currGun = uzi;

                pistol.SetActive(false);
                ak74.SetActive(false);
                bm4.SetActive(false);
                uzi.SetActive(true);
                break;
        }
    }
}
