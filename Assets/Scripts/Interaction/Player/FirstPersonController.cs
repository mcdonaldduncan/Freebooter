﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

//TODO Abstract into classes that are managed by this class (i.e. defualt movement, wallrun movement, etc.)
public sealed class FirstPersonController : MonoBehaviour, IDamageable
{
    public EventSystem PlayerUIEventSystem { get { return playerUIEventSystem; } }
    public AudioSource PlayerAudioSource { get { return playerAudioSource; } }
    public AudioClip LowHealthAudio { get { return lowHealthAudio; } }
    public AudioClip GunPickupAudio { get { return gunPickupAudio; } }
    public AudioClip KeyPickupAudio { get { return keyPickupAudio; } }
    public Camera PlayerCamera { get { return playerCamera; } }
    public GunHandler PlayerGun { get { return playerGun; } }
    public float MaxHealth { get { return maxHealth; } set { maxHealth = value; } }
    public float Health { get { return health; } set { health = value; } }
    public float DistanceToHeal { get { return distanceToHeal; } }
    public float PercentToHeal { get { return percentToHeal / 100; } }
    public float DashTime { get { return dashTime; } }
    public float DashesAllowed { get { return dashesAllowed; } }
    public float DashesRemaining { get { return dashesRemaining; } }
    public float DashCooldownTime { get { return adjustedCooldown; } }
    public float FinalJumpForce { get { return finalJumpForce; } set { finalJumpForce = value; } }
    public bool PlayerCanMove { get; private set; } = true;
    public bool PlayerIsDashing { get; private set; }
    private bool CanDoNextDash => lastDashEnd + timeBetweenDashes < Time.time;
    public bool DashShouldCooldown => dashesRemaining < dashesAllowed;
    private bool NonZeroVelocity => characterController.velocity.z != 0 || characterController.velocity.x != 0;
    private bool PlayerHasDashes => dashesRemaining > dashesAllowed - dashesAllowed;
    public bool PlayerCanDash => PlayerHasDashes && NonZeroVelocity;
    public bool PlayerCanDashAgain => PlayerCanDash && playerDashing;
    public bool UpdateDashBar { get; set; }
    public float DashTimeDifference { get { return ((dashCooldownStartTime + dashCooldownTime) - Time.time); } }
    public bool CanBeDamaged => !hasIFrames && !invincible;

    //Checks used to see if player is able to use mechanics.
    [Header("Functional Options")]
    [SerializeField]
    private bool invincible = false;
    [SerializeField]
    private float maxHealth, health;
    [SerializeField]
    private float distanceToHeal = 10f;
    [Tooltip("What percent of the enemy's max health should the player heal if they kill them within the distance to heal?")]
    [SerializeField]
    private float percentToHeal = 25f;
    [Tooltip("Is the player in the middle of a special movement, i.e. ladder climbing?")]
    [SerializeField]
    public bool playerOnSpecialMovement = false;
    [SerializeField]
    private EventSystem playerUIEventSystem;
    [SerializeField]
    private AudioClip playerHealSFX;
    [SerializeField]
    private GameObject onKillHealVFX;
    //[SerializeField]
    //private bool playerCanDash = true; Unused!
    //[SerializeField]
    //private bool playerCanHeadbob = true; Unused!
    private bool hasIFrames = false;

    //parameters for different movement speeds
    [Header("Movement Parameters")]
    public float walkSpeed = 6; // Changed to public so powerups can affec this variable
    public float wallRunSpeed = 12f; // Changed to public so powerups can affec this variable
    public float owalkspeed = 6; // Changed to public so powerups can affec this variable
    public float owallspeed = 12f; // Changed to public so powerups can affec this variable
    [SerializeField]
    private float slopeSlideSpeed = 6f;
    [SerializeField]
    private float fovDefault = 60f;
    [SerializeField]
    private float fovSprint = 70f;
    [SerializeField]
    private float fovIncrement = 5f;

    [SerializeField] private float maxSpeedScale;

    //Parameters for looking around with mouse
    [Header("Look Parameters")]
    [SerializeField]
    private bool restrictHorizontal;
    [SerializeField]
    private float controllerLookSensitivity = 2f;
    [SerializeField, Range(1, 10)]
    private float mouseLookSpeedX = 2f;
    [SerializeField, Range(1, 10)]
    private float mouseLookSpeedY = 2f;
    [SerializeField, Range(1, 100)]
    private float upperLookLimit = 80f;
    [SerializeField, Range(1, 100)]
    private float lowerLookLimit = 80f;
    [SerializeField, Range(1, 100)]
    private float leftLookLimit = 80f;
    [SerializeField, Range(1, 100)]
    private float rightLookLimit = 80f;

    //Parameters for jump height and gravity
    [Header("Jumping Parameters")]
    [Tooltip("How many jumps are allowed after the inital one?")]
    [SerializeField]
    public int jumpsAllowed = 1;
    [SerializeField]
    private float maxJumpTime;
    [SerializeField]
    private float jumpForce = 8f;
    [SerializeField]
    private float secondJumpForce = 16f;
    [SerializeField]
    private float gravity = 30f;
    private float finalJumpForce;
    public int jumpsRemaining;
    private bool jumpedOnce;
    private bool jumpStarted;
    private bool holdingJump;
    private float holdJumpTimer;

    //[Header("Wallrunning Parameters")]
    //[SerializeField] private LayerMask whatIsWall;
    //[SerializeField] private LayerMask whatIsGround;
    //[SerializeField] private float wallRunGravity;
    //[SerializeField] private float maxWallRunTime;
    //[SerializeField] private float wallCheckDistance;
    //[SerializeField] private float minJumpHeight;
    //[SerializeField] private Transform orientation;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;
    private float wallRunTimer;
    private float verticalInput;

    [Header("Bob Parameters")]
    [SerializeField]
    private Transform bobObjHolder;
    [SerializeField]
    private float bobSpeed = 14f;
    [SerializeField]
    private float yBobAmount = 0.05f;
    [SerializeField]
    private float xBobAmount = 0.05f;
    private float defaultYPosBobObj = 0;
    private float defaultXPosBobObj = 0;
    private float timer;
    private Vector3 defaultLocalPosition;

    [Header("Dash Parameters")]
    [SerializeField]
    private float dashDamage;
    [SerializeField]
    private float dashRayDistance;
    [SerializeField]
    private int dashesAllowed = 2;
    [SerializeField]
    private int dashesRemaining;
    [SerializeField]
    private float dashSpeed = 0f;
    [Tooltip("How long the player moves at dash speed for after they press the button")]
    [SerializeField]
    private float dashTime;
    [Tooltip("Length in seconds of the dash cooldown")]
    [SerializeField]
    private float dashCooldownTime;
    [Tooltip("If player is holding dash and there are dashes remaining, how much time should there be between the dashes?")]
    [SerializeField]
    private float dashBetweenTime;
    private float dashStartTime;
    private float lastDashEnd;
    private float timeBetweenDashes;
    private float dashCooldownStartTime;

    [Header("State bools")]
    public bool basicMovement;
    public bool wallRunning;

    [Header("Audio")]
    [SerializeField] private AudioClip dashAudio;
    [SerializeField] private AudioClip lowHealthAudio;
    [SerializeField] private AudioClip playerHitAudio;
    [SerializeField] private AudioClip gunPickupAudio;
    [SerializeField] private AudioClip keyPickupAudio;
    [SerializeField] private AudioClip m_DashRechargeAudio;
    [SerializeField] private AudioClip m_JumpAudio;
    private AudioSource playerAudioSource;

    private Camera playerCamera;
    private CharacterController characterController;
    private Rigidbody playerRB;
    private GunHandler playerGun;
    private PauseController pauseController;

    private Vector3 moveDirection;
    private Vector2 currentInput; //Whether player is moving vertically or horizontally along x and z planes
    private Vector2 dashInput;
    private Vector2 lookDelta;
    private Vector2 prevLookDelta;

    public Vector2 MoveInput { get; private set; }

    private float rotationX = 0f; //Camera rotation for clamping
    private float rotationY = 0f;

    private bool playerDashing;
    private bool playerLooking;
    private bool dashOnCooldown;
    private bool playerShouldDash;
    private bool playerPaused;
    private float adjustedCooldown;
    private float lookAxisValue;

    private Coroutine dashRoutine;

    private float groundRayDistance = 1;
    private RaycastHit slopeHit;
    private WaitForSeconds dashCooldownWait;
    private WaitForSeconds dashBetweenWait;

    public static InputActions _input;
    private InputDevice inputDevice;

    private MovementState state;
    
    public delegate void PlayerDelegate();
    public event PlayerDelegate PlayerDashed;
    //public event PlayerDelegate OnDashCooldown;  Unused!
    public event PlayerDelegate PlayerHealthChanged;

    public delegate void DamageTrackingDelegate(float damage);
    public event DamageTrackingDelegate PlayerDamaged;

    [NonSerialized] public Vector3 surfaceMotion;

    Vector3 startingPos;

    public bool isDead;
    int isDeadFrameCount;

    float speedScale => 1 + ((maxSpeedScale - 1) * (1 - (health / maxHealth)));

    public GameObject DamageTextPrefab => throw new NotImplementedException();

    public Transform TextSpawnLocation => throw new NotImplementedException();

    public float FontSize => throw new NotImplementedException();

    public bool ShowDamageNumbers => throw new NotImplementedException();

    public TextMeshPro Text { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    float originalSpeed, boostSpeedDuration, boostStartedTime;
    bool boostedSpeedEnabled;

    private DeathScreen m_deathScreen;

    public bool isAttached;

    public enum MovementState
    {
        basic
        //wallrunning
    }
    
    void Awake()
    {
        health = maxHealth;
        dashCooldownWait = new WaitForSeconds(dashCooldownTime);
        dashBetweenWait = new WaitForSeconds(dashBetweenTime);

        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        playerRB = GetComponent<Rigidbody>();
        playerGun = GetComponentInChildren<GunHandler>();
        pauseController = GetComponentInChildren<PauseController>();

        //Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerCamera.fieldOfView = fovDefault;

        _input = new InputActions();

        state = MovementState.basic;

        jumpsRemaining = jumpsAllowed;
        dashesRemaining = dashesAllowed;

        playerAudioSource = GetComponent<AudioSource>();
        adjustedCooldown = dashCooldownTime;
    }

    private void Start()
    {
        playerPaused = false;
        startingPos = transform.position;
        //OnWeaponSwitch();
        //defaultLocalPosition = bobObjHolder.localPosition;
        //defaultYPosBobObj = defaultLocalPosition.y;
        //defaultXPosBobObj = defaultLocalPosition.x;
        m_deathScreen = GetComponentInChildren<DeathScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!characterController.enabled) return;

        if (Health > MaxHealth)
        {
            Health = MaxHealth; 
            PlayerHealthChanged?.Invoke();
        }

        if (!playerOnSpecialMovement)
        {
            if (PlayerCanMove)
            {
                if (boostedSpeedEnabled == true)
                {
                    if (boostStartedTime + boostSpeedDuration < Time.time)
                    {
                        BoostDashEnd();
                    }
                }

                adjustedCooldown = dashCooldownTime / speedScale;
                //CheckForWall();
                StateHandler();
            }
        }

        HandleLook();
    }

    private void LateUpdate()
    {
        if (!isDead) return;
        if (++isDeadFrameCount < 2) return;
        isDead = false;
        characterController.enabled = true;
        isDeadFrameCount = 0;
    }

    private void FixedUpdate()
    {
        if (holdingJump && holdJumpTimer < maxJumpTime)
        {
            moveDirection.y = finalJumpForce;
            holdJumpTimer += Time.fixedDeltaTime;
        }
    }

    private void OnEnable()
    {
        //Subscribe methods to the input actions
        _input.Enable();

        //HumanoidLand
        _input.HumanoidLand.Walk.performed += HandleWalkInput;
        _input.HumanoidLand.Walk.canceled += HandleWalkInput;
        _input.HumanoidLand.Look.performed += HandleLookInput;
        _input.HumanoidLand.Look.canceled += HandleLookInput;
        _input.HumanoidLand.Dash.started += HandleDashInput;
        _input.HumanoidLand.Dash.canceled += HandleDashInput;
        _input.HumanoidLand.Jump.started += HandleJump;
        _input.HumanoidLand.Jump.canceled += HandleJump;
        _input.HumanoidLand.Restart.performed += ReloadScene;
        _input.HumanoidLand.Pause.performed += HandlePause;

        //HumanoidWall
        //_input.HumanoidWall.Forward.performed += HandleWallrunInput;
        //_input.HumanoidWall.Forward.canceled += HandleWallrunInput;
        //_input.HumanoidWall.Jump.performed += HandleJump;

        //GunHandler
        _input.Gun.Shoot.performed += playerGun.Shoot;
        _input.Gun.Shoot.canceled += playerGun.Shoot;
        _input.Gun.SwitchWeapon.performed += playerGun.SwitchWeapon;
        //_input.Gun.Reload.performed += playerGun.Reload;
        _input.Gun.AlternateFire.performed += playerGun.AlternateShoot;

        //if (LevelManager.Instance.Player == null) LevelManager.Instance.Player = this;
        UpdateDash.DashCooldownCompleted += DashCooldown;

        //Subscribe to weapon switch event
        GunHandler.weaponSwitched += OnWeaponSwitch;
    }

    private void OnDisable()
    {
        //Unsubscribe methods from the input actions
        _input.Disable();

        //HumanoidLand
        _input.HumanoidLand.Walk.performed -= HandleWalkInput;
        _input.HumanoidLand.Walk.canceled -= HandleWalkInput;
        _input.HumanoidLand.Look.performed -= HandleLookInput;
        _input.HumanoidLand.Look.canceled -= HandleLookInput;
        _input.HumanoidLand.Dash.started -= HandleDashInput;
        _input.HumanoidLand.Dash.canceled -= HandleDashInput;
        _input.HumanoidLand.Jump.started -= HandleJump;
        _input.HumanoidLand.Jump.canceled -= HandleJump;
        _input.HumanoidLand.Restart.performed -= ReloadScene;
        _input.HumanoidLand.Pause.performed -= pauseController.OnPause;

        //HumanoidWall
        //_input.HumanoidWall.Forward.performed -= HandleWallrunInput;
        //_input.HumanoidWall.Forward.canceled -= HandleWallrunInput;
        //_input.HumanoidWall.Jump.performed -= HandleJump;

        //GunHandler
        _input.Gun.Shoot.performed -= playerGun.Shoot;
        _input.Gun.Shoot.canceled -= playerGun.Shoot;
        _input.Gun.SwitchWeapon.performed -= playerGun.SwitchWeapon;
        //_input.Gun.Reload.performed -= playerGun.Reload;
        _input.Gun.AlternateFire.performed -= playerGun.AlternateShoot;

        UpdateDash.DashCooldownCompleted -= DashCooldown;

        GunHandler.weaponSwitched -= OnWeaponSwitch;
    }

    private void StateHandler()
    {
        // Mode - Basic Movement
        if (state == MovementState.basic)
        {
            //state = MovementState.basic;
            _input.HumanoidWall.Disable();
            _input.HumanoidLand.Enable();
            ApplyFinalBasicMovements();
        }

        // Mode - Wallrunning
        //if (state == MovementState.wallrunning)
        //{
        //    //state = MovementState.wallrunning;
        //    _input.HumanoidLand.Disable();
        //    _input.HumanoidWall.Enable();
        //    ApplyFinalWallrunMovements();
        //}
    }

    private void HandlePause(InputAction.CallbackContext context)
    {
        pauseController.OnPause(context);

        playerPaused = !playerPaused;
    }

    private void ReloadScene(InputAction.CallbackContext context)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        LevelManager.TogglePause(false);
        SceneManager.LoadScene(0);
    }

    private void AdaptFOV()
    {
        if (playerDashing && playerCamera.fieldOfView < fovSprint)
        {
            playerCamera.fieldOfView += fovIncrement * Time.deltaTime;
        }
        else if (!playerDashing && playerCamera.fieldOfView > fovDefault)
        {
            playerCamera.fieldOfView -= fovIncrement * Time.deltaTime;
        }
    }

    private void HandleWalkInput(InputAction.CallbackContext context)
    {
        //when the player presses W and S or A and D
        currentInput = (context.ReadValue<Vector2>());
        MoveInput = new Vector2(currentInput.x, currentInput.y);

    }

    private void HandleDashInput(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            playerShouldDash = false;
            playerDashing = false;
            hasIFrames = false;
            if (dashRoutine != null)
            {
                StopCoroutine(dashRoutine);
            }
        }
        if (context.started)
        {
            playerShouldDash = true;

            if (PlayerCanDash)
            {
                dashRoutine = StartCoroutine(Dash());
            }
        }
    }

    private IEnumerator Dash()
    {
        dashesRemaining--;
        PlayerDashed?.Invoke();
        float startTime = Time.time;

        moveDirection = (transform.TransformDirection(Vector3.right) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);

        //While loop where the dash happens
        hasIFrames = true; //turn on iframes
        playerAudioSource.Stop();
        playerAudioSource.PlayOneShot(dashAudio);
        while (Time.time < startTime + dashTime)
        {
            UpdateDashBar = true;
            playerDashing = true;
            characterController.Move(moveDirection * dashSpeed * Time.deltaTime);
            moveDirection.y = 0;

            RaycastHit hitInfo;

            //Raycast infront of player to see if they hit an IDamagable, and should thus deal damage
            if (Physics.Raycast(transform.position, moveDirection, out hitInfo, dashRayDistance))
            {
                if (hitInfo.transform.name != "Player")
                {
                    var damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                    if (damageableTarget != null)
                    {
                        damageableTarget.TakeDamage(dashDamage, HitBoxType.normal);
                    }
                }
            }

            //lastDashEnd = Time.time;

            yield return null;
        }
        UpdateDashBar = false;
        hasIFrames = false; //turn off iframes now that dash is finished

        if (PlayerCanDashAgain)
        {
            yield return dashBetweenWait;
            if (playerShouldDash)
            {
                StopCoroutine(dashRoutine);
                dashRoutine = StartCoroutine(Dash());
            }
        }
        playerDashing = false;
    }

    private void DashCooldown()
    {
        if (DashShouldCooldown)
        {
            playerAudioSource.PlayOneShot(m_DashRechargeAudio);
            dashesRemaining++;
        }
    }

    private void HandleLookInput(InputAction.CallbackContext context)
    {
        inputDevice = context.control.device;

        //if(inputDevice is Gamepad)
        //{
        //    lookAxisValue = context.ReadValue<float>();
        //}
    }

    private void HandleLook()
    {
        //lookDelta = ApplyDeadzone(lookDelta, deadzone);

        if (inputDevice is Mouse)
        {
            rotationX -= Input.GetAxis("Mouse Y") * mouseLookSpeedY;
            rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit); //clamp camera
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseLookSpeedX, 0);

            //rotationX -= Input.GetAxis("Mouse Y") * mouseLookSpeedY;
            //rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);

            //if (restrictHorizontal)
            //{
            //    //rotate camera around X and Y axis, and rotate player around x axis
            //    rotationY += Input.GetAxis("Mouse X") * mouseLookSpeedX;
            //    rotationY = Mathf.Clamp(rotationY, -leftLookLimit, rightLookLimit);//clamp camera
            //    playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
            //}
            //else
            //{
            //    playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            //    transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * mouseLookSpeedX, 0);
            //}
        }
        else if (inputDevice is Gamepad)
        {
            Vector2 rightStickInput = Gamepad.current.rightStick.ReadValue();
            rotationX -= rightStickInput.y * mouseLookSpeedY;
            rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit); //clamp camera
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, rightStickInput.x * mouseLookSpeedX, 0);
        }
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        inputDevice = context.control.device;

        if (context.canceled)
        {
            holdingJump = false;
            holdJumpTimer = 0;
            playerPaused = false;
        }
        if (context.started && jumpsRemaining > 0)
        {
            if (inputDevice is Gamepad && playerPaused) return;

            playerAudioSource.PlayOneShot(m_JumpAudio);
            jumpsRemaining--;
            holdingJump = true;
            if (!jumpedOnce)
            {
                finalJumpForce = jumpForce;
                jumpedOnce = true;
            }
            else if (jumpedOnce)
            {
                finalJumpForce = secondJumpForce;
                jumpedOnce = false;
            }
        }
    }

    public void BoostJump(float jumpForceMultiplier)
    {
        moveDirection = transform.TransformDirection(Vector3.up) * jumpForceMultiplier;
    }

    public void BoostDash(float DashMultiplier, float duration)
    {
        if (boostedSpeedEnabled) { return; }
        boostStartedTime = Time.time;
        boostSpeedDuration = duration;
        originalSpeed = walkSpeed;
        walkSpeed *= DashMultiplier;
        boostedSpeedEnabled = true;
    }

    void BoostDashEnd()
    {
        walkSpeed = originalSpeed;
        boostedSpeedEnabled = false;
    }

    private void OnWeaponSwitch()
    {
        bobObjHolder = playerGun.CurrentGun.GunModel.transform;
        defaultLocalPosition = bobObjHolder.localPosition;
        defaultYPosBobObj = defaultLocalPosition.y;
        defaultXPosBobObj = defaultLocalPosition.x;
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded && state == MovementState.basic)
        {
            return;
        }

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (bobSpeed);
            bobObjHolder.localPosition = new Vector3(
                defaultXPosBobObj + Mathf.Sin(timer) * (xBobAmount),
                defaultYPosBobObj + Mathf.Sin(timer) * (yBobAmount),
                bobObjHolder.localPosition.z);
        }
        else
        {
            bobObjHolder.localPosition = defaultLocalPosition;
        }
    }

    private void ApplyFinalBasicMovements()
    {
        HandleHeadbob();
        AdaptFOV();

        //Stop the player from floating along the ceiling
        if (characterController.velocity.y == 0 && !characterController.isGrounded)
        {
            if(holdingJump) holdingJump = false;
            moveDirection.y = 0;
        }

        //make sure the player is on the ground if applying gravity (after pressing Jump)
        if (!characterController.isGrounded && !playerDashing)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        //The direction in which the player moves based on input
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.right) * (MoveInput.x * walkSpeed * speedScale)) + (transform.TransformDirection(Vector3.forward) * (MoveInput.y * walkSpeed * speedScale));
        moveDirection.y = moveDirectionY;

        if (OnSteepSlope()) SteepSlopeMovement();

        // move the player based on the parameters gathered in the "Handle-" functions

        // Apply accumulated motion from attached platforms before clearing the vector
        characterController.Move(surfaceMotion + (moveDirection * Time.deltaTime));
        surfaceMotion = Vector3.zero;

        if (jumpsRemaining < jumpsAllowed && characterController.isGrounded && !holdingJump)
        {
            jumpedOnce = false;
            jumpsRemaining = jumpsAllowed;
        }
    }

    private bool OnSteepSlope()
    {
        if (!characterController.isGrounded) return false;

        if (!characterController.isGrounded && moveDirection.y == 0) return true;

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, (characterController.height / 2) + groundRayDistance))
        {
            float slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
            if (slopeAngle > characterController.slopeLimit) return true;
        }

        return false;
    }

    private void SteepSlopeMovement()
    {
        Vector3 slopeDirection = Vector3.up - slopeHit.normal * Vector3.Dot(Vector3.up, slopeHit.normal);
        float slideSpeed = walkSpeed + slopeSlideSpeed + Time.deltaTime;

        moveDirection = slopeDirection * -slideSpeed;
        moveDirection.y = moveDirection.y - slopeHit.point.y;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    //TODO: Consider using OverlapSpheres instead of raycasts as this might help with stutter
    //private void CheckForWall()
    //{
    //    //Parameters in order: start point, direction, store hit info, distance, layermask
    //    wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
    //    wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);

    //    verticalInput = Input.GetAxisRaw("Vertical");

    //    //If the raycast has detected a wall and the player is not touching the ground
    //    if ((wallLeft || wallRight) && verticalInput > 0 && !characterController.isGrounded)
    //    {
    //        if (!holdingJump)
    //        {
    //            //Reset the jumps as if the player has touched the ground
    //            jumpedOnce = false;
    //            jumpsRemaining = jumpsAllowed;
    //        }

    //        //If the player is not currently in the wallRunning state
    //        if (state != MovementState.wallrunning)
    //        {
    //            //Make sure the player can't climb the wall
    //            moveDirection.y = 0;
    //            //Set the state to wallRunning
    //            state = MovementState.wallrunning;
    //        }
    //    }
    //    else
    //    {
    //        //If a wall is not detected and if the player is currently in the wallrunning state
    //        if (state == MovementState.wallrunning)
    //        {
    //            //Set the state back to the basic movement state
    //            state = MovementState.basic;
    //        }
    //    }
    //}

    //private void HandleWallrunInput(InputAction.CallbackContext context)
    //{
    //    currentInput = (context.ReadValue<Vector2>());
    //    MoveInput = new Vector2(0, currentInput.y * wallRunSpeed); //Make sure the player can't move up the wall
    //}

    //private void ApplyFinalWallrunMovements()
    //{
    //    HandleHeadbob();
    //    AdaptFOV();

    //    //Get of the normal of the surface ray hit on the right or left wall
    //    Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

    //    //Get the cross product of the wallNormal and the up direction of the player transform
    //    Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

    //    //apply gravity
    //    moveDirection.y -= wallRunGravity * Time.deltaTime;

    //    //The direction in which the player moves based on input
    //    float moveDirectionY = moveDirection.y;
    //    moveDirection = (transform.TransformDirection(wallForward) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);
    //    moveDirection.y = -Mathf.Abs(moveDirectionY);

    //    //move the player based on the parameters gathered in the "Handle-" functions
    //    characterController.Move(moveDirection * Time.deltaTime);

    //    //if the player is on the ground, and they have less than max jumps, reset the remaining jumps to the maximum (for double jumping)
    //    if (characterController.isGrounded && jumpsRemaining < jumpsAllowed && !holdingJump)
    //    {
    //        jumpedOnce = false;
    //        jumpsRemaining = jumpsAllowed;
    //    }
    //}

    public void TakeDamage(float damageTaken, HitBoxType hitBox, Vector3 hitPoint = default(Vector3))
    {
        if (CanBeDamaged)
        {
            Health -= damageTaken;
            PlayerHealthChanged?.Invoke();
            PlayerDamaged?.Invoke(damageTaken);
            playerAudioSource.PlayOneShot(playerHitAudio);
            //Debug.Log($"Player Health: { health }");
            CheckForDeath();
        }
    }


    /// <summary>
    /// For pick up system which is currently not in use
    /// </summary>
    /// <param name="heal"></param>
    public void HealthRegen(float heal, Vector3 enemyPos)
    {
        if (health >= maxHealth) return;
        ProjectileManager.Instance.TakeFromPool(onKillHealVFX, transform.position);
        health += heal;

        if (health >= maxHealth) health = maxHealth;
        playerAudioSource.PlayOneShot(playerHealSFX);

        if (health > MaxHealth) health = MaxHealth;
        PlayerHealthChanged?.Invoke();
        //Debug.Log($"Player healed. Current health is {health}");
    }

    public void HealthRegen(float heal)
    {
        HealthRegen(heal, transform.position);
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            PlayerHealthChanged?.Invoke();
            //Debug.Log("Player died!");
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);


            OnDeath();
            
        }
    }

    void OnDeath()
    {
        isDead = true;
        characterController.enabled = false;
        m_deathScreen.StopTimeWhenDead();
    }

    public void Respawn()
    {
        transform.SetParent(null, true);

        transform.position = LevelManager.Instance.CurrentCheckPoint?.transform.position ?? startingPos;
        transform.rotation = LevelManager.Instance.CurrentCheckPoint?.transform.rotation ?? Quaternion.identity;

        health = maxHealth;
        PlayerHealthChanged?.Invoke();

        LevelManager.Instance.FirePlayerRespawn();
        isDead = false;
        characterController.enabled = true;
    }
}