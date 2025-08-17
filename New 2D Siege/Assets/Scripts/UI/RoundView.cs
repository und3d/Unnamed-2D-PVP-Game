using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundView : View
{
    [Header("References")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text roundTimerText;
    [SerializeField] private TMP_Text blueTeamRoundCount;
    [SerializeField] private TMP_Text redTeamRoundCount;
    [SerializeField] private TMP_Text ammoCounter;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text gadgetPrimaryText;
    [SerializeField] private TMP_Text gadgetSecondaryText;
    [SerializeField] private TMP_Text gadgetPrimaryCount;
    [SerializeField] private TMP_Text gadgetSecondaryCount;
    [SerializeField] private List<Image> RedTeamPlayerIcons = new List<Image>();
    [SerializeField] private List<Image> BlueTeamPlayerIcons = new List<Image>();
    
    [Header("Variables")]
    [SerializeField] private Color redTeamIconDead;
    [SerializeField] private Color redTeamIconAlive;
    [SerializeField] private Color blueTeamIconDead;
    [SerializeField] private Color blueTeamIconAlive;

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

    public void UpdateGadgetPrimaryText(string gadgetName)
    {
        gadgetPrimaryText.text = gadgetName + ":";
    }

    public void UpdateGadgetSecondaryText(string gadgetName)
    {
        gadgetSecondaryText.text = gadgetName + ":";
    }

    public void UpdateGadgetPrimaryCount(int gadgetCount)
    {
        gadgetPrimaryCount.text = gadgetCount.ToString();
    }

    public void UpdateGadgetPrimaryCountToggleTimer(float duration)
    {
        Debug.Log("Setting text to timer");
        seconds = Mathf.FloorToInt(duration % 60);
        gadgetPrimaryCount.text = seconds.ToString();
    }

    public void SetGadgetPrimaryCountInfinite()
    {
        Debug.Log("Setting text to infinite");
        gadgetPrimaryCount.text = "\u221E";
    }

    public void UpdateGadgetPrimaryCountToolGun(int currentAmmo, int remainingAmmo)
    {
        gadgetPrimaryCount.text = $"{currentAmmo,0}/{remainingAmmo,0}";
    }

    public void UpdateGadgetSecondaryCount(int gadgetCount)
    {
        gadgetSecondaryCount.text = gadgetCount.ToString();
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
