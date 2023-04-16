using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    public void StopTimeWhenDead()
    {
        if (called == true) return; 
        FirstPersonController.enabled = false;
        CameraShake.ShakeCamera(0, 0, 0);
        LevelManager.TogglePause(true);
        gunHandler.CurrentGun.GunReticle.alpha = 0;
        UI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        called = true;
    }

    public void Respawn()
    {
        gunHandler.CurrentGun.GunReticle.alpha = 1;
        UI.SetActive(false);    
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        FirstPersonController.enabled = true;
        FirstPersonController.Respawn();
        LevelManager.TogglePause(false);
        called = false;
    }

    public void OnLevelSelect()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UI.gameObject.SetActive(false);
        LevelManager.TogglePause(false);
        SceneManager.LoadScene(0);
        called = false;
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
