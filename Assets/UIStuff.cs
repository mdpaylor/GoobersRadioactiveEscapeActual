using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIStuff : MonoBehaviour
{
    public static UIStuff Instance { get; set; }

    // Timer
    
    [SerializeField] private TextMeshProUGUI timerText;
    
    private float elapsedTime;

    // Health

    private playerHealth healthOfPlayer;
    
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    
    void Awake()
    {
        Instance = this;

        healthOfPlayer = this.GetComponentInParent<playerHealth>();

        timerText = GameObject.FindGameObjectWithTag("TimerText").GetComponent<TextMeshProUGUI>();
    }
    
    void Start()
    {
        slider.maxValue = healthOfPlayer.health;
        fill.color = gradient.Evaluate(1f);

        slider.value = slider.maxValue;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime = elapsedTime + Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public int getHealth()
    {
        return (int)slider.value;
    }
    
    public void setHealth(int health)
    {
        slider.value = health;
    }
    public void healthChange(int healthChange)
    {
        Debug.Log("Health changed");

        slider.value = slider.value + healthChange;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void setMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;

        fill.color = gradient.Evaluate(1f);
    }
}
