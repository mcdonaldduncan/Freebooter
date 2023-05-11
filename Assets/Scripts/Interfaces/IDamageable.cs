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

    public GameObject DamageTextPrefab { get; }
    public Transform TextSpawnLocation { get; }
    public bool ShowDamageNumbers { get; }
    public float FontSize { get; }

    //void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3? hitPoint = null);

    void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3));

    public void SetupDamageText()
    {
       // Text = DamageTextPrefab.GetComponent<TextMeshPro>();
    }

    public void GenerateDamageInfo(float damageTaken, HitBoxType hitType, TextMeshPro Text)
    {
        switch (hitType)
        {
            case HitBoxType.critical:
                Text.color = Color.red;
                Text.fontSize = FontSize * 2;
                Text.text = damageTaken.ToString("0") + "!";
                break;
            case HitBoxType.armored:
                Text.color = Color.blue;
                Text.text = damageTaken.ToString("0");
                break;
            case HitBoxType.shield:
                //for now we dont have any shielded enemies.
                //TODO : make the shields also show damage numbers
                Text.color = Color.blue;
                Text.text = damageTaken.ToString("0");
                break;
            case HitBoxType.normal:
                Text.color = Color.white;
                Text.text = damageTaken.ToString("0");
                break;
        }
    }

    public void InstantiateDamageNumber(float damageTaken, HitBoxType hitType)
    {
        if (ShowDamageNumbers == true)
        {
            var obj = ProjectileManager.Instance.TakeFromPool(DamageTextPrefab, new Vector3(TextSpawnLocation.transform.position.x + Random.Range(-1f, 1f), TextSpawnLocation.transform.position.y, TextSpawnLocation.transform.position.z + Random.Range(-1f, 1f)));
            GenerateDamageInfo(damageTaken, hitType, obj.GetComponent<TextMeshPro>());
        }
    }

    public void CheckForDeath();
    
}
