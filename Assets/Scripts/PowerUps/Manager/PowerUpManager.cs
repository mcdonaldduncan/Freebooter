using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static ApplyPowerUp;

public class PowerUpManager : Singleton<PowerUpManager>
{
    [SerializeField] private List<GameObject> powerups = new List<GameObject>();
    [SerializeField] private float dropRate;
    public bool PowerUPS_On;

    [SerializeField] private GameObject Player;
    [SerializeField] private GunHandler gunHandler;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private float speedboostMultiplier;
    [SerializeField] private bool AmmoApp, SpeedAPP, HealthApp;
    [SerializeField] private float AmmoDuration, HealthDuration, SpeedDuration, healAmmount, AmmoTimer, HealthTimer, SpeedTimer, blinktimer;
    [SerializeField] private TextMeshProUGUI AmmoTXT, HealthTXT, SpeedTXT;
    [SerializeField] private Image Ammoimg, HealthImg, SpeedImg;
    float acr = .4f, alc, hcr = .4f, hlc, scr = .4f, slc;
    float healRate = 1.5f, lastHealed;


    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        gunHandler = Player.GetComponentInChildren<GunHandler>();
        firstPersonController = Player.GetComponentInChildren<FirstPersonController>();
        var txtgo1 = GameObject.Find("AmmoTXT");
        AmmoTXT = txtgo1.GetComponent<TextMeshProUGUI>();
        Ammoimg = AmmoTXT.transform.parent.GetComponent<Image>();
        var txtgo2 = GameObject.Find("HealthTXT");
        HealthTXT = txtgo2.GetComponent<TextMeshProUGUI>();
        HealthImg = HealthTXT.transform.parent.GetComponent<Image>();
        var txtgo3 = GameObject.Find("MSTXT");
        SpeedTXT = txtgo3.GetComponent<TextMeshProUGUI>();
        SpeedImg = SpeedTXT.transform.parent.GetComponent<Image>();


        HealthImg.enabled = false;
        Ammoimg.enabled = false;
        SpeedImg.enabled = false;
        AmmoApp = false;
        SpeedAPP = false;
        HealthApp = false;
    }
    private void FixedUpdate()
    {
        //Debug.Log("ApplyEffectSpeed!!!" + SpeedDuration + SpeedTimer + SpeedAPP);
    }
    private void Update()
    {
        CheckTimers();
        UpdateTXT();
        Blink();
    }

    // This method is UpdateTXT() but it also handles the timers
    void UpdateTXT()
    {
        // in line statements dont need brackets
        if (AmmoApp == false)
        {
            AmmoTXT.text = $"";
        }
        else
        {
            AmmoTXT.text = $"{AmmoTimer.ToString("0.00")}";
            AmmoTimer -= Time.deltaTime;
            Ammoimg.enabled = true;
        }

        // This is not very readable
        if (HealthApp == false) { HealthTXT.text = $""; }
        else { HealthTXT.text = $"{HealthTimer.ToString("0.00")}"; HealthTimer -= Time.deltaTime; HealthImg.enabled = true; }

        if (SpeedAPP == false) { SpeedTXT.text = $""; }
        else { SpeedTXT.text = $"{SpeedTimer.ToString("0.00")}"; SpeedTimer -= Time.deltaTime; SpeedImg.enabled = true; }

    }
    void CheckTimers()
    {
        if (AmmoTimer > 0 && AmmoApp == true)
        {
            ApplyEffectAmmo();
            Ammoimg.enabled = true;
        }
        else if (AmmoTimer < 0)
        {
            RemoveEffectAmmo();
            Ammoimg.enabled = false;
        }
        if (HealthTimer > 0 && HealthApp == true)
        {
            ApplyEffectHealth();
            HealthImg.enabled = true;
        }
        else if (HealthTimer < 0)
        {
            RemoveHealing();
            HealthImg.enabled = false;
        }
        if (SpeedTimer > 0 && SpeedAPP == true)
        {
            ApplyEffectSpeed();
            Debug.Log("ApplyEffectSpeed();");
            SpeedImg.enabled = true;
        }
        else if (SpeedTimer < 0 && SpeedAPP == true)
        {
            Debug.Log("RemovedxD");
            RemoveEffectSpeed();
            SpeedImg.enabled = false;
        }
    }
    void Blink()
    {
        if (AmmoTimer < AmmoDuration/2.5f)
        {
            if (Time.time > acr + alc && Ammoimg.color.a > 0)
            {
                Ammoimg.color -= new Color(0, 0, 0, 1f);
                alc = Time.time;
            }
            if (Time.time > acr + alc && Ammoimg.color.a <= 0)
            {
                Ammoimg.color += new Color(0, 0, 0, 1f);
                alc = Time.time;
            }
        }
        else if (AmmoTimer > AmmoDuration / 2.5f) { Ammoimg.color = Color.white; }


        if (HealthTimer < HealthDuration / 2.5f)
        {
            if (Time.time > hcr + hlc && HealthImg.color.a > 0)
            {
                HealthImg.color -= new Color(0, 0, 0, 1f);
                hlc = Time.time;
            }
            if (Time.time > hcr + hlc && HealthImg.color.a <= 0)
            {
                HealthImg.color += new Color(0, 0, 0, 1f);
                hlc = Time.time;
            }
        }
        else if (HealthTimer > HealthDuration / 2.5f) { HealthImg.color = Color.white; }


        if (SpeedTimer < SpeedDuration / 2.5f)
        {
            if (Time.time >  scr + slc && SpeedImg.color.a > 0)
            {
                SpeedImg.color -= new Color(0, 0, 0, 1f);
                slc = Time.time;
            }
            if (Time.time > scr + slc && SpeedImg.color.a <= 0)
            {
                SpeedImg.color += new Color(0, 0, 0, 1f);
                slc = Time.time;
            }
        }
        else if (SpeedTimer > SpeedDuration / 2.5f) { SpeedImg.color = Color.white; }
    }
    void ApplyEffectAmmo()
    {
        //gunHandler.InfAmmoActive();
    }
    void ApplyEffectHealth()
    {
        if (Time.time > healRate + lastHealed)
        {
            Heal();
            lastHealed = Time.time;
        }
    }
    void Heal()
    {
        Debug.Log("healed for " + healAmmount);
        firstPersonController.HealthRegen(healAmmount);
    }
    void RemoveHealing()
    {
        HealthApp = false;
    }
    void ApplyEffectSpeed()
    {
        firstPersonController.walkSpeed = firstPersonController.owalkspeed * speedboostMultiplier;
        firstPersonController.wallRunSpeed = firstPersonController.owallspeed * speedboostMultiplier;
    }
    void RemoveEffectSpeed()
    {
        firstPersonController.walkSpeed = firstPersonController.owalkspeed;
        firstPersonController.wallRunSpeed = firstPersonController.owallspeed;
        SpeedAPP = false;
    }
    void RemoveEffectAmmo()
    { 
        //gunHandler.InfAmmoInactive();
        AmmoApp = false;
    }
    public void IntakeData(float d, ApplyPowerUp.PowerType currentType)//d for duration
    {
        switch (currentType)
        {
            case ApplyPowerUp.PowerType.Ammo:
                
                AmmoDuration = d;
                AmmoTimer = d;
                AmmoApp = true;
                break;
            case ApplyPowerUp.PowerType.Health:
              
                HealthDuration = d;
                HealthApp = true;
                HealthTimer = d;
                break;
            case ApplyPowerUp.PowerType.Speed:
               
                SpeedDuration = d;
                SpeedTimer = d;
                SpeedAPP = true;
                Debug.Log("ApplyEffectSpeed!!!" +SpeedDuration+SpeedTimer + SpeedAPP);
                break;
            default:
                break;
        }
    }
    public void CheckForDrops(Transform t)
    {
        var randomNum = Random.RandomRange(0f, 100f);
        if (randomNum < dropRate && PowerUPS_On == true)
        {
            Instantiate(powerups[Random.Range(0, powerups.Count)], t.position, t.rotation);
        }
    }
}
