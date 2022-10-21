using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.Networking;

public class NetFirstPersonController : NetworkBehaviour
{
    NetworkVariable<Vector3> netPos = new NetworkVariable<Vector3>();
    NetworkObject netObj;

    //properties used to help check whether player can use certain mechanics. These are mostly to keep the code clean and organized
    //Kind of a rudimentary/crude state machine
    public bool PlayerCanMove { get; private set; } = true;
    public bool PlayerIsDashing { get; private set; }
    //private bool PlayerIsSprinting => playerCanSprint && Input.GetKey(sprintKey) && !playerIsCrouching;
    //private bool PlayerShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded && !playerIsCrouching;
    //private bool PlayerShouldCrouch => Input.GetKeyDown(crouchKey) && !playerInCrouchAnimation && characterController.isGrounded;

    //Checks used to see if player is able to use mechanics.
    [Header("Functional Options")]
    [Tooltip("Is the player in the middle of a special movement, i.e. ladder climbing?")]
    [SerializeField]
    public bool playerOnSpecialMovement = false;
    //[SerializeField]
    //private bool playerCanSprint = true;
    [SerializeField]
    private bool playerCanJump = true;
    //[SerializeField]
    //private bool playerCanCrouch = true;
    [SerializeField]
    private bool playerCanHeadbob = true;

    //parameters for different movement speeds
    [Header("Movement Parameters")]
    [SerializeField]
    private float walkSpeed = 3f;
    //[SerializeField]
    //private float sprintSpeed = 6f;
    //[SerializeField]
    //private float crouchSpeed = 1.5f;
    [SerializeField]
    private float fovDefault = 60f;
    [SerializeField]
    private float fovSprint = 70f;
    [SerializeField]
    private float fovIncrement = 5f;

    //Parameters for looking around with mouse
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)]
    private float lookSpeedX = 2f;
    [SerializeField, Range(1, 10)]
    private float lookSpeedY = 2f;
    [SerializeField, Range(1, 100)]
    private float upperLookLimit = 80f;
    [SerializeField, Range(1, 100)]
    private float lowerLookLimit = 80f;

    //Parameters for jump height and gravity
    [Header("Jumping Parameters")]
    [SerializeField]
    private float jumpForce = 8f;
    [SerializeField]
    private float gravity = 30f;

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
    private float dashSpeed = 0f;
    [SerializeField]
    private float dashTime;

    private Camera playerCamera;
    private CharacterController characterController;
    private Rigidbody playerRB;

    private Vector3 moveDirection;
    private Vector2 currentInput; //Whether player is moving vertically or horizontally along x and z planes
    private Vector2 dashInput;
    public Vector2 MoveInput { get; private set; }

    private float rotationX = 0f; //Camera rotation for clamping

    private bool playerIsSprinting;
    private bool playerDashing;

    private InputActions _input;

   

    void Awake()
    {
        netObj = GetComponent<NetworkObject>();
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        playerRB = GetComponent<Rigidbody>();
        defaultYPosCamera = playerCamera.transform.localPosition.y;

        //Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerCamera.fieldOfView = fovDefault;
    }

    public override void OnNetworkSpawn()
    {
        playerCamera.enabled = IsLocalPlayer ? true : false;
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsLocalPlayer)
            return;
        if (!playerOnSpecialMovement)
        {
            if (PlayerCanMove)
            {
                //HandleWalkInput();
                HandleMouseLook();

                //if (playerCanCrouch)
                //{
                //    HandleCrouch();
                //}

                //Apply all the movement parameters that are found earlier in the frame (above in Update())
                ApplyFinalMovements();
            }
        }
        else if (playerOnSpecialMovement)
        {
            HandleMouseLook();
        }

    }

    private void OnEnable()
    {
        
        //Subscribe methods to the input actions
        _input = new InputActions();
        _input.HumanoidLand.Enable();

        _input.HumanoidLand.Walk.performed += HandleWalkInput;
        _input.HumanoidLand.Walk.canceled += HandleWalkInput;

        _input.HumanoidLand.Dash.performed += HandleDashInput;

        _input.HumanoidLand.Jump.performed += HandleJump;

        _input.HumanoidLand.Restart.performed += ReloadScene;
    }

    private void OnDisable()
    {
        
        //Unsubscribe methods from the input actions
        _input.HumanoidLand.Walk.performed -= HandleWalkInput;
        _input.HumanoidLand.Walk.canceled -= HandleWalkInput;

        _input.HumanoidLand.Dash.performed -= HandleDashInput;

        _input.HumanoidLand.Jump.performed -= HandleJump;

        _input.HumanoidLand.Restart.performed -= ReloadScene;


        _input.HumanoidLand.Disable();
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

        if (characterController.velocity.z != 0 || characterController.velocity.x != 0)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        float startTime = Time.time;

        //The direction in which the player moves based on input
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.right) * MoveInput.x) + (transform.TransformDirection(Vector3.forward) * MoveInput.y);
        moveDirection.y = 0;//moveDirectionY;

        while (Time.time < startTime + dashTime)
        {
            playerDashing = true;
            characterController.Move(moveDirection * dashSpeed * Time.deltaTime);

            yield return null;
        }

        playerDashing = false;
    }

    private void HandleMouseLook()
    {
        //rotate camera around X and Y axis, and rotate player around x axis
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit); //clamp camera
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        //only jump if property conditions are met
        if (characterController.isGrounded)
        {
            moveDirection.y = jumpForce;
        }
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
        if (!characterController.isGrounded)
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

    private void ApplyFinalMovements()
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

        //move the player based on the parameters gathered in the "Handle-" functions
        characterController.Move(moveDirection * Time.deltaTime);
        //UpdateNetworkPosServerRPC(playerRB.position);
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

    //[ServerRpc]
    //public void UpdateNetworkPosServerRPC(Vector3 pos)
    //{
    //    netPos.Value = pos;
    //}
}