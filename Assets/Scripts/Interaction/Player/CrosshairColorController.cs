using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairColorController : MonoBehaviour
{
    [SerializeField] private Color m_defaultColor;
    [SerializeField] private Color m_damageableColor;
    private Image m_crosshairImage;
    private Camera m_playerCamera;
    private GunHandler m_gunHandler;
    private Ray m_cameraRay;
    private RaycastHit m_hitInfo;
    private IDamageable m_damageableObject;
    private Follower m_follower;

    private void Awake()
    {
        m_crosshairImage = GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_playerCamera = LevelManager.Instance.Player.PlayerCamera;
        m_gunHandler = LevelManager.Instance.Player.PlayerGun;
    }

    // Update is called once per frame
    void Update()
    {
        m_crosshairImage.color = m_defaultColor;
        m_cameraRay = m_playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(m_cameraRay, out m_hitInfo, float.MaxValue, ~m_gunHandler.CurrentGun.LayerToIgnore) && gameObject.activeSelf)
        {
            m_damageableObject = m_hitInfo.transform.GetComponent<IDamageable>();
            m_follower = m_hitInfo.transform.GetComponent<Follower>();
            if (m_damageableObject != null || m_follower != null) m_crosshairImage.color = m_damageableColor;
        }
        //else
        //{
        //    m_crosshairImage.color = m_defaultColor;
        //}
    }
}
