using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GunHandler m_gunHandler;
    [SerializeField] private Button m_defaultSelectedButton;

    private bool m_isPaused;
    private CanvasGroup m_ui;
    private FirstPersonController m_fpsController;

    private void Start()
    {
        m_ui = GetComponentInChildren<CanvasGroup>();
        m_ui.gameObject.SetActive(false);
        m_fpsController = GetComponentInParent<FirstPersonController>();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        CameraShake.ShakeCamera(0, 0, 0);

        if (!m_isPaused)
        {
            m_fpsController.PlayerUIEventSystem.SetSelectedGameObject(m_defaultSelectedButton.gameObject);
            m_gunHandler.CurrentGun.GunReticle.alpha = 0;
            m_fpsController.enabled = false;
            LevelManager.TogglePause(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            m_ui.gameObject.SetActive(true);
            m_isPaused = true;
        }
        else
        {
            OnResume();
        }
    }

    public void OnResume()
    {
        m_gunHandler.CurrentGun.GunReticle.alpha = 1;
        m_fpsController.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CameraShake.ShakeCamera(0, 0, 0);
        m_ui.gameObject.SetActive(false);
        LevelManager.TogglePause(false);
        m_isPaused = false;
    }

    public void OnRespawnCheckpoint()
    {
        m_gunHandler.CurrentGun.GunReticle.alpha = 1;
        m_ui.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_fpsController.enabled = true;
        m_fpsController.Respawn();
        LevelManager.TogglePause(false);
        m_isPaused = false;
    }

    public void OnRestartLevel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        m_ui.gameObject.SetActive(false);
        LevelManager.TogglePause(false);
        m_isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnLevelSelect()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        m_ui.gameObject.SetActive(false);
        LevelManager.TogglePause(false);
        m_isPaused = false;
        SceneManager.LoadScene(0);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
