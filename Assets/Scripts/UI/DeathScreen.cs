using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DeathScreen : MonoBehaviour
{
    FirstPersonController FirstPersonController;
    [SerializeField]
    private GameObject UI; //ChildObject called UI 
    [SerializeField]
    private GunHandler gunHandler;

    bool called = false;

    private void Start()
    {
        FirstPersonController = LevelManager.Instance.Player.GetComponent<FirstPersonController>();
        UI.SetActive(false);
    }

    private void Update()
    {
        StopTimeWhenDead();
    }

    public void StopTimeWhenDead()
    {
        if (FirstPersonController.isDead == false) return;
        if (called == true) return; 
        FirstPersonController.enabled = false;
        CameraShake.ShakeCamera(0, 0, 0);
        LevelManager.TogglePause(true);
        gunHandler.CurrentGun.GunReticle.alpha = 0;
        UI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn()
    {
        LevelManager.TogglePause(false);
        gunHandler.CurrentGun.GunReticle.alpha = 1;
        UI.SetActive(false);
        FirstPersonController.enabled = true;
        FirstPersonController.Respawn();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
}
