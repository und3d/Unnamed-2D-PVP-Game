using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundView : View
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text roundTimerText;
    [SerializeField] private TMP_Text blueTeamRoundCount;
    [SerializeField] private TMP_Text redTeamRoundCount;
    [SerializeField] private TMP_Text ammoCounter;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private Color redTeamIconDead;
    [SerializeField] private Color redTeamIconAlive;
    [SerializeField] private List<Image> RedTeamPlayerIcons = new List<Image>();
    [SerializeField] private Color blueTeamIconDead;
    [SerializeField] private Color blueTeamIconAlive;
    [SerializeField] private List<Image> BlueTeamPlayerIcons = new List<Image>();

    private int minutes, seconds, centiSeconds;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this); //A safer singleton; needs a OnDestroy
    }
    
    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<RoundView>();
    }
    
    public override void OnShow()
    {
        
    }

    public override void OnHide()
    {
        
    }

    public void UpdateRoundTimer(float roundTimer)
    {
        minutes = Mathf.FloorToInt(roundTimer / 60);
        seconds = Mathf.FloorToInt(roundTimer % 60);
        centiSeconds = Mathf.FloorToInt((roundTimer % 1f) * 100);

        if (minutes <= 0 && seconds <= 0 && centiSeconds <= 0)
        {
            roundTimerText.text = "00:00";
        }
        else if (minutes <= 0 && seconds < 10)
        {
            roundTimerText.text = string.Format("{0:00}:{1:00}", seconds, centiSeconds);
        }
        else
        {
            roundTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void UpdateBlueTeamRoundCount(int roundCount)
    {
        blueTeamRoundCount.text = roundCount.ToString();
    }

    public void UpdateRedTeamRoundCount(int roundCount)
    {
        redTeamRoundCount.text = roundCount.ToString();
    }

    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
    }

    public void UpdateAmmoCounter(int ammo, int reserveAmmo)
    {
        ammoCounter.text = $"{ammo,0}/{reserveAmmo,0}";
    }

    public void UpdateWeaponText(string gunName)
    {
        weaponNameText.text = gunName;
    }
    
    public void SetRedPlayerIconOnDeath(int iconID)
    {
        RedTeamPlayerIcons[iconID].color = redTeamIconDead;
    }

    public void SetRedPlayerIconAlive(int iconID)
    {
        RedTeamPlayerIcons[iconID].color = redTeamIconAlive;
    }
    
    public void SetBluePlayerIconOnDeath(int iconID)
    {
        BlueTeamPlayerIcons[iconID].color = blueTeamIconDead;
    }

    public void SetBluePlayerIconAlive(int iconID)
    {
        BlueTeamPlayerIcons[iconID].color = blueTeamIconAlive;
    }

    public void ResetAllPlayerIcons()
    {
        //Debug.Log("RoundView is setting all icons to dead.");
        
        foreach (var icon in RedTeamPlayerIcons)
        {
            icon.color = redTeamIconDead;
        }

        foreach (var icon in BlueTeamPlayerIcons)
        {
            icon.color = blueTeamIconDead;
        }
    }
}
