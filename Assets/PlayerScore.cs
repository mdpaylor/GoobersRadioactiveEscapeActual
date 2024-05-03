using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerScore : MonoBehaviour
{
    public static PlayerScore Instance;

    [SerializeField] private int playerScore = 0;

    public TextMeshProUGUI playerScoreText;

    private void Awake()
    {
        playerScore = 0;

        Instance = this;
    }

    private void Update()
    {
        if (playerScoreText != null) playerScoreText.text = $"{playerScore}";
    }

    public int getPlayerScore()
    {
        return playerScore;
    }
    
    public void addToPlayerScore(int num)
    {
        playerScore = playerScore + num;
    }

}
