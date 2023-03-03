using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public interface IDamageable
{
    public float Health { get; set; }
    public GameObject DamagePopUpPrefab { get; }
    public Transform PopupFromHere { get; }
    public float fontSize { get; }

    bool showDamageNumbers { get; }

    void TakeDamage(float damageTaken){}

    public void GenerateDamageInfo(float damageTaken, HitBoxType hitType)
    {
        if (showDamageNumbers == false) { return; }
        DamageNumbers(damageTaken, hitType);
    }
    public void DamageNumbers(float DamageNumber, HitBoxType hitType)
    {//if special hitbox use this one
        var txtpro = DamagePopUpPrefab.GetComponent<TextMeshPro>();
        ResetDamageNumberValuers();
        if (hitType != null)
        {
            switch (hitType)
            {
                case HitBoxType.critical:
                    txtpro.color = Color.red;
                    txtpro.fontSize = fontSize * 2;
                    txtpro.text = DamageNumber.ToString("0") + "!";
                    break;
                case HitBoxType.armored:
                    txtpro.color = Color.blue;
                    txtpro.text = DamageNumber.ToString("0");
                    break;
                case HitBoxType.shield:
                    //for now we dont have any shielded enemies.
                    //TODO : make the shields also show damage numbers
                    txtpro.color = Color.blue;
                    txtpro.text = DamageNumber.ToString("0");
                    break;
                case HitBoxType.normal:
                    txtpro.color = Color.white;
                    txtpro.text = DamageNumber.ToString("0");
                    break;
            }
        }
        else if (hitType == null)
        {
            txtpro.color = Color.gray;
        }
        InstantiateDamageNumber();
    }

    public void InstantiateDamageNumber()
    {
        ProjectileManager.Instance.TakeFromPool(DamagePopUpPrefab, new Vector3(PopupFromHere.transform.position.x + UnityEngine.Random.Range(-1f, 1f), PopupFromHere.transform.position.y, PopupFromHere.transform.position.z + UnityEngine.Random.Range(-1f, 1f)));
    }

    public void ResetDamageNumberValuers()
    {
        var txtpro = DamagePopUpPrefab.GetComponent<TextMeshPro>();
        txtpro.color = Color.gray;
        txtpro.fontSize = fontSize;
    }
    public void CheckForDeath();
    
}
