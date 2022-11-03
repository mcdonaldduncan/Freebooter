using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//TODO I'll abstract all this at some point lol
public class FirstPersonController : MonoBehaviour, IDamageable
{
    //properties used to help check whether player can use certain mechanics. These are mostly to keep the code clean and organized
    //Kind of a rudimentary/crude state machine
    public float MaxHealth { get { return maxHealth; } set { maxHealth = value; } }
    public float Health { get { return health; } set { health = value; } }
    public float FinalJumpForce { get { return finalJumpForce; } set { finalJumpForce = value; } }
    public bool PlayerCanMove { get; private set; } = true;
    public bool PlayerIsDashing { get; private set; }
    private bool CanDoNextDash => lastDashEnd + timeBetweenDashes < Time.time;
    private bool DashShouldCooldown => dashesRemaining < dashesAllowed;
    private bool NonZeroVelocity => characterController.velocity.z != 0 || characterController.velocity.x != 0;
    private bool PlayerHasDashes => dashesRemaining > dashesAllowed - dashesAllowed;
    public bool PlayerCanDash => PlayerHasDashes && NonZeroVelocity;
    //private bool PlayerIsSprinting => playerCanSprint && Input.GetKey(sprintKey) && !playerIsCrouching;
    //private bool PlayerShouldCrouch => Input.GetKeyDown(crouchKey) && !playerInCrouchAnimation && characterController.isGrounded;

    //Checks used to see if player is able to use mechanics.
    [Header("Functional Options")]
    [SerializeField]
    private float maxHealth, health;
    [Tooltip("Is the player in the middle of a special movement, i.e. ladder climbing?")]
    [SerializeField]
    public bool playerOnSpecialMovement = false;
    //[SerializeField]
    //private bool playerCanSprint = true;
    //[SerializeField]
    //private bool playerCanJump = true;
    [SerializeField]
    private bool playerCanDash = true;
    //[SerializeField]
    //private bool playerCanCrouch = true;
    [SerializeField]
    private bool playerCanHeadbob = true;

    //parameters for different movement speeds
    [Header("Movement Parameters")]
    public float walkSpeed = 6; // Changed to public so powerups can affec this variable
    public float wallRunSpeed = 12f; // Changed to public so powerups can affec this variable
    public float owalkspeed = 6; // Changed to public so powerups can affec this variable
    public float owallspeed = 12f; // Changed to public so powerups can affec this variable
    //[SerializeField]
    //private float sprintSpeed = 6f;
    //[SerializeField]
    //private float crouchSpeed = 1.5f;
    [SerializeField]
    private float slopeSlideSpeed = 6f;
    [SerializeField]
    private float fovDefault = 60f;
    [SerializeField]
    private float fovSprint = 70f;
    [SerializeField]
    private float fovIncrement = 5f;

    //Parameters for looking around with mouse
    [Header("Look Parameters")]
    [SerializeField]
    private bool restrictHorizontal;
    [SerializeField, Range(1, 10)]
    private float lookSpeedX = 2f;
    [SerializeField, Range(1, 10)]
    private float lookSpeedY = 2f;
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
    private int jumpsAllowed = 1;
    [SerializeField]
    private float maxJumpTime;
    [SerializeField]
    private float jumpForce = 8f;
    [SerializeField]
    private float secondJumpForce = 16f;
    [SerializeField]
    private float gravity = 30f;
    private float finalJumpForce;
    private int jumpsRemaining;
    private bool jumpedOnce;
    private bool jumpStarted;
    private bool holdingJump;
    private float holdJumpTimer;

    [Header("Wallrunning Parameters")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float maxWallRunTime;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    [SerializeField] private Transform orientation;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;
    private float wallRunTimer;
    private float verticalInput;

    //KEEPING THIS INCASE WE WANT TO ADD CROUCHING
    //Parameters for crouching. The height and center will directly affect the CharacterController height and center.
    //[Header("Crouch Parameters")]
    //[SerializeField]
    //private float crouchingHeight = 0.5f;
    //[SerializeField]
    //private float standingHeight = 2f;
    //[SerializeField]
    //private float timeToCrouch = 0.25f; //How long should the crouching animation take?
    //[SerializeField]
    //private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    //[SerializeField]
    //private Vector3 standingCenter = new Vector3(0, 0, 0); //Didn't use Vector3.Zero so that it would be customizable in inspector
    //private bool playerIsCrouching; //Is the player currently crouched?
    //private bool playerInCrouchAnimation; //Is the player currently in the middle of the crouching animation?

    [Header("Headbob Parameters")]
    [SerializeField]
    private float walkBobSpeed = 14f;
    [SerializeField]
    private float walkBobAmount = 0.05f;
    //[SerializeField]
    //private float sprintBobSpeed = 18f;
    //[SerializeField]
    //private float sprintBobAmount = 0.1f;
    //[SerializeField]
    //private float crouchBobSpeed = 8f;
    //[SerializeField]
    //private float crouchBobAmount = 0.025f;
    private float defaultYPosCamera = 0;
    private float timer;

    [Header("Dash Parameters")]
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

    private Camera playerCamera;
    private CharacterController characterController;
    private Rigidbody playerRB;
    private GunHandler playerGun;

    private Vector3 moveDirection;
    private Vector2 currentInput; //Whether player is moving vertically or horizontally along x and z planes
    private Vector2 dashInput;
    public Vector2 MoveInput { get; private set; }

    private float rotationX = 0f; //Camera rotation for clamping
    private float rotationY = 0f;

    //private bool playerIsSprinting;
    private bool playerDashing;
    private bool dashOnCooldown;
    private bool playerShouldDash;

    private float groundRayDistance = 1;
    private RaycastHit slopeHit;
    private WaitForSeconds dashCooldownWait;
    private WaitForSeconds dashBetweenWait;

    public static InputActions _input;

    private MovementState state;

    public enum MovementState
    {
        basic,
        wallrunning
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

        defaultYPosCamera = playerCamera.transform.localPosition.y;

        //Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerCamera.fieldOfView = fovDefault;

        _input = new InputActions();

        state = MovementState.basic;

        jumpsRemaining = jumpsAllowed;
        dashesRemaining = dashesAllowed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerOnSpecialMovement)
        {
            if (PlayerCanMove)
            {
                //if (playerShouldDash && PlayerHasDashes)
                //{
                //    BeginDash();
                //    if (playerDashing)
                //    {
                //        Dash();
                //    }
                //}
                if (DashShouldCooldown)
                {
                    DashCooldown();
                }
                CheckForWall();
                StateHandler();
            }
        }

        HandleMouseLook();
    }

    private void FixedUpdate()
    {
        if (holdingJump && holdJumpTimer < maxJumpTime)
        {
            //Debug.Log($"Jumps Remaining: {jumpsRemaining} | Jumped Once: {jumpedOnce} | Final Jump Force: {finalJumpForce}");
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
        _input.HumanoidLand.Dash.performed += HandleDashInput;
        _input.HumanoidLand.Dash.canceled += HandleDashInput;
        _input.HumanoidLand.Jump.performed += HandleJump;
        _input.HumanoidLand.Jump.canceled += HandleJump;
        _input.HumanoidLand.Restart.performed += ReloadScene;

        //HumanoidWall
        _input.HumanoidWall.Forward.performed += HandleWallrunInput;
        _input.HumanoidWall.Forward.canceled += HandleWallrunInput;
        _input.HumanoidWall.Jump.performed += HandleJump;

        //GunHandler
        _input.Gun.Shoot.performed += playerGun.Shoot;
        _input.Gun.Shoot.canceled += playerGun.Shoot;
        _input.Gun.SwitchWeapon.performed += playerGun.SwitchWeapon;
        _input.Gun.Reload.performed += playerGun.Reload;
    }

    private void OnDisable()
    {
        //Unsubscribe methods from the input actions
        _input.Disable();

        //HumanoidLand
        _input.HumanoidLand.Walk.performed -= HandleWalkInput;
        _input.HumanoidLand.Walk.canceled -= HandleWalkInput;
        _input.HumanoidLand.Dash.performed -= HandleDashInput;
        _input.HumanoidLand.Dash.canceled -= HandleDashInput;
        _input.HumanoidLand.Jump.performed -= HandleJump;
        _input.HumanoidLand.Jump.canceled -= HandleJump;
        _input.HumanoidLand.Restart.performed -= ReloadScene;

        //HumanoidWall
        _input.HumanoidWall.Forward.performed -= HandleWallrunInput;
        _input.HumanoidWall.Forward.canceled -= HandleWallrunInput;
        _input.HumanoidWall.Jump.performed -= HandleJump;

        //GunHandler
        _input.Gun.Shoot.performed -= playerGun.Shoot;
        _input.Gun.Shoot.canceled -= playerGun.Shoot;
        _input.Gun.SwitchWeapon.performed -= playerGun.SwitchWeapon;
        _input.Gun.Reload.performed -= playerGun.Reload;
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
        if (state == MovementState.wallrunning)
        {
            //state = MovementState.wallrunning;
            _input.HumanoidLand.Disable();
            _input.HumanoidWall.Enable();
            ApplyFinalWallrunMovements();
        }
    }

    private void ReloadScene(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        MoveInput = new Vector2(currentInput.x * walkSpeed, currentInput.y * walkSpeed);

    }

    private void HandleDashInput(InputAction.CallbackContext context)
    {
        //KEEPING THIS IN CASE WE WANT TO ADD SPRINTING
        //playerIsSprinting = !playerIsSprinting;

        //if (currentInput != null)
        //{
        //    if (playerIsSprinting)
        //    {
        //        MoveInput = new Vector2(currentInput.x * sprintSpeed, currentInput.y * sprintSpeed);
        //    }
        //    else if (!playerIsSprinting)
        //    {
        //        MoveInput = new Vector2(currentInput.x * walkSpeed, currentInput.y * walkSpeed);
        //    }
        //}
        if (context.performed)
        {
            //InitiateDash();
            playerShouldDash = true;

            if (/*(characterController.velocity.z != 0 || characterController.velocity.x != 0) &&*/ PlayerCanDash)
            {
                StartCoroutine(Dash());
            }
        }
        else if (context.canceled)
        {
            playerShouldDash = false;
            playerDashing = false;
        }
    }

    //private void BeginDash()
    //{
    //    if (CanDoNextDash)
    //    {
    //        if (!playerDashing)
    //        {
    //            playerDashing = true;
    //            dashesRemaining--;
    //            dashStartTime = Time.time;
    //            dashCooldownStartTime = dashStartTime;

    //            moveDirection = (transform.TransformDirection(Vector3.right) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);
    //            moveDirection.y = 0;
    //        }
    //    }
    //    else
    //    {
    //        playerDashing = false;
    //    }
    //}

    //private void Dash()
    //{
    //    if (dashStartTime + dashTime > Time.time && PlayerCanDash && playerDashing)
    //    {
    //        characterController.Move(moveDirection * dashSpeed * Time.deltaTime);
    //    }
    //    else
    //    {
    //        lastDashEnd = Time.time;
    //        playerDashing = false;
    //    }
    //}

    //private void InitiateDash()
    //{
    //    dashesRemaining--;
    //    dashStartTime = Time.time;
    //    dashCooldownStartTime = dashStartTime;

    //    moveDirection = (transform.TransformDirection(Vector3.right) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);
    //    moveDirection.y = 0;
    //}

    //private void DoTheDash()
    //{

    //}

    //TODO: Doesn't need to be a coroutine
    private IEnumerator Dash()
    {
        dashesRemaining--;
        float startTime = Time.time;
        dashCooldownStartTime = startTime;

        moveDirection = (transform.TransformDirection(Vector3.right) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);
        moveDirection.y = 0;

        while (Time.time < startTime + dashTime) //&& playerShouldDash)
        {
            playerDashing = true;
            characterController.Move(moveDirection * dashSpeed * Time.deltaTime);

            lastDashEnd = Time.time;

            yield return null;
        }

        if (PlayerCanDash)
        {
            yield return dashBetweenWait;
            if (playerShouldDash)
            {
                StartCoroutine(Dash());
            }
        }
        else
        {
            playerDashing = false;
        }
    }

    private void DashCooldown()
    {
        if (dashCooldownStartTime + dashCooldownTime < Time.time && DashShouldCooldown)
        {
            dashesRemaining++;
            dashCooldownStartTime = Time.time;
            Debug.Log("Cooldown Complete!");
        }
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);

        if (restrictHorizontal)
        {
            //rotate camera around X and Y axis, and rotate player around x axis
            rotationY += Input.GetAxis("Mouse X") * lookSpeedX;
            rotationY = Mathf.Clamp(rotationY, -leftLookLimit, rightLookLimit);//clamp camera
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
        }
        else
        {
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
        }
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        //jumpsRemaining--;
        if (context.canceled)
        {
            //jumpsRemaining++;
            holdingJump = false;
            holdJumpTimer = 0;
        }
        if (context.performed && jumpsRemaining > 0)
        {
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

        //Debug.Log($"Character Grounded: {characterController.isGrounded}");
    }

    //KEEPING THIS INCASE WE WANT TO ADD CROUCHING
    //private void HandleCrouch()
    //{
    //    //only crouch if property conditions are met
    //    if (PlayerShouldCrouch)
    //    {
    //        StartCoroutine(CrouchStand());
    //    }
    //}

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded && state == MovementState.basic)
        {
            return;
        }

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPosCamera + Mathf.Sin(timer) * (walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    private void ApplyFinalBasicMovements()
    {
        HandleHeadbob();
        AdaptFOV();

        //make sure the player is on the ground if applying gravity (after pressing Jump)
        if (!characterController.isGrounded && !playerDashing)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        //The direction in which the player moves based on input
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.right) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);
        moveDirection.y = moveDirectionY;

        if (OnSteepSlope()) SteepSlopeMovement();

        //move the player based on the parameters gathered in the "Handle-" functions
        characterController.Move(moveDirection * Time.deltaTime);

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
    private void CheckForWall()
    {
        //Parameters in order: start point, direction, store hit info, distance, layermask
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);

        verticalInput = Input.GetAxisRaw("Vertical");

        //If the raycast has detected a wall and the player is not touching the ground
        if ((wallLeft || wallRight) && verticalInput > 0 && !characterController.isGrounded)
        {
            if (!holdingJump)
            {
                //Reset the jumps as if the player has touched the ground
                jumpedOnce = false;
                jumpsRemaining = jumpsAllowed;
            }

            //If the player is not currently in the wallRunning state
            if (state != MovementState.wallrunning)
            {
                //Make sure the player can't climb the wall
                moveDirection.y = 0;
                //Set the state to wallRunning
                state = MovementState.wallrunning;
            }
        }
        else
        {
            //If a wall is not detected and if the player is currently in the wallrunning state
            if (state == MovementState.wallrunning)
            {
                //Set the state back to the basic movement state
                state = MovementState.basic;
            }
        }
    }

    private void HandleWallrunInput(InputAction.CallbackContext context)
    {
        currentInput = (context.ReadValue<Vector2>());
        MoveInput = new Vector2(0, currentInput.y * wallRunSpeed); //Make sure the player can't move up the wall
    }

    private void ApplyFinalWallrunMovements()
    {
        HandleHeadbob();
        AdaptFOV();

        //Get of the normal of the surface ray hit on the right or left wall
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        //Get the cross product of the wallNormal and the up direction of the player transform
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        //apply gravity
        moveDirection.y -= wallRunGravity * Time.deltaTime;

        //The direction in which the player moves based on input
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(wallForward) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);
        moveDirection.y = -Mathf.Abs(moveDirectionY);

        //move the player based on the parameters gathered in the "Handle-" functions
        characterController.Move(moveDirection * Time.deltaTime);

        //if the player is on the ground, and they have less than max jumps, reset the remaining jumps to the maximum (for double jumping)
        if (characterController.isGrounded && jumpsRemaining < jumpsAllowed && !holdingJump)
        {
            jumpedOnce = false;
            jumpsRemaining = jumpsAllowed;
        }
    }

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        Debug.Log($"Player Health: { health }");
        CheckForDeath();
    }
    public void HealthRegen(float heal)
    {
        health += heal;
        if (health > MaxHealth)
        {
            health = MaxHealth;
        }
        Debug.Log($"Player healed. Current health is {health}");
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Debug.Log("Player died!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    //KEEPING THIS INCASE WE WANT TO ADD CROUCHING
    //Coroutine that handles crouching/standing
    //private IEnumerator CrouchStand()
    //{
    //    //make sure there is nothing above player's head that should prevent them from standing, if there is, do not allow them to stand
    //    if (playerIsCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
    //    {
    //        yield break;
    //    }

    //    //player is now in crouching animation
    //    playerInCrouchAnimation = true;

    //    float timeElapsed = 0; //amount of time elapsed during animation
    //    float targetHeight = playerIsCrouching ? standingHeight : crouchingHeight; //target height based on the state the player is in when they press crouch button
    //    float currentHeight = characterController.height; //the player's height when they press the crouch button
    //    Vector3 targetCenter = playerIsCrouching ? standingCenter : crouchingCenter; //target center based on the state the player is in when they press crouch button
    //    Vector3 currentCenter = characterController.center; //the player's center when they press the crouch button

    //    //while the animation is still going
    //    while(timeElapsed < timeToCrouch)
    //    {
    //        characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch); //change the current height to the target height
    //        characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrouch); //change the current center to the target center

    //        timeElapsed += Time.deltaTime; //increment the time elapsed based on the time it took between frames

    //        yield return null;
    //    }

    //    //Sanity check :P
    //    characterController.height = targetHeight;
    //    characterController.center = targetCenter;

    //    playerIsCrouching = !playerIsCrouching; //update whether or not the player is crouching

    //    playerInCrouchAnimation = false; //the crouching animation has ended
    //}
}
