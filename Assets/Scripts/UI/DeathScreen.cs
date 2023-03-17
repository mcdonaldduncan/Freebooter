using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    FirstPersonController FirstPersonController;
    [SerializeField]
    private GameObject UI; //ChildObject called UI 

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
        LevelManager.TogglePause(true);
        UI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Respawn()
    {
        UI.SetActive(false);
        FirstPersonController.enabled = true;
        FirstPersonController.Respawn();
        LevelManager.TogglePause(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
}
