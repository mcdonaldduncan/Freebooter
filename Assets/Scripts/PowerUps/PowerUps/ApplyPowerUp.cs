using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApplyPowerUp : MonoBehaviour
{
    private enum PowerType {Ammo, Health, Speed };
    [SerializeField] private PowerType pt;
    [SerializeField] private GameObject Player;
    [SerializeField] private GunHandler gunHandler;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private float speedboostMultiplier;
    bool app = false, blink = false;
    public float duration, healAmmount, timer, blinktimer;
    float walks;
    float walls;
    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private Image img;
    MeshRenderer msh;
    BoxCollider bxc;
    // Start is called before the first frame update
    void Start()
    {
        msh = this.gameObject.GetComponent<MeshRenderer>();
        bxc = this.gameObject.GetComponent<BoxCollider>();
        timer = duration;
        Player = GameObject.FindWithTag("Player");
        gunHandler = Player.GetComponentInChildren<GunHandler>();
        firstPersonController = Player.GetComponentInChildren<FirstPersonController>();
        walks = firstPersonController.walkSpeed;
        walls = firstPersonController.wallRunSpeed;
    }

    // Update is called once per frame
    private void Update()
    {
        if (app == true) 
        {
            if (timer <0 || timer == duration) { txt.text = $"";}
            else txt.text = $"{timer.ToString("0.00")}"; timer-=Time.deltaTime;

            if (timer < -2)
            {
                Destroy(this.gameObject);
            }
            if (timer < duration / 2.5f && blink == false)
            {
                img.CrossFadeAlpha(0,timer,false);
            }
        }
    }
    private void OnDestroy()
    {
        img.enabled = false;
    }
    void ApplyEffectAmmo()
    {
        if (app == false)
        {
            gunHandler.InfAmmoActive();
            Invoke("RemoveEffectAmmo", duration);
            app = true;
        }
    }
    void RemoveEffectAmmo() 
    { 
        gunHandler.InfAmmoInactive();
        img.enabled = false;
    }
    void ApplyEffectHealth()
    {
        if (app == false)
        {
            Invoke("Heal", 0);
            Invoke("Heal", (duration / 3) + (duration/3) );
            Invoke("Heal", duration / 3);
            Invoke("Heal", duration);
            Invoke("DisableHeal", duration);
            app = true;
        }
    }
    void DisableHeal() { img.enabled = false; }
    void Heal()
    {
        Debug.Log("healed for " + healAmmount);
        firstPersonController.HealthRegen(healAmmount);
    }
    void ApplyEffectSpeed()
    {
        if (app == false)
        {

            firstPersonController.walkSpeed *= speedboostMultiplier;
            firstPersonController.wallRunSpeed *= speedboostMultiplier;    
            Invoke("RemoveEffectSpeed", duration);
            app = true;
        }
    }
    void RemoveEffectSpeed() 
    {
        firstPersonController.walkSpeed = walks; 
        firstPersonController.wallRunSpeed = walls;
        img.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        switch (pt)
        {
            case PowerType.Ammo:
                var txtgo1  = GameObject.Find("AmmoTXT");
                txt = txtgo1.GetComponent<TextMeshProUGUI>();
                img = GameObject.Find("AmmoPowerUp").GetComponent<Image>();
                img.enabled = true;
                ApplyEffectAmmo();
                msh.enabled = false;
                bxc.enabled = false;
                break;
            case PowerType.Health:
                var txtgo2 = GameObject.Find("HealthTXT");
                txt = txtgo2.GetComponent<TextMeshProUGUI>();
                img = GameObject.Find("HealthPowerUp").GetComponent<Image>();
                img.enabled = true;
                ApplyEffectHealth();
                msh.enabled = false;
                bxc.enabled = false;
                break;
            case PowerType.Speed:
                var txtgo3 = GameObject.Find("MSTXT");
                txt = txtgo3.GetComponent<TextMeshProUGUI>();
                img = GameObject.Find("MoveSpeedPowerUp").GetComponent<Image>();
                img.enabled = true;
                ApplyEffectSpeed();
                msh.enabled = false;
                bxc.enabled = false;
                break;
            default:
                break;
        }
    }
}
