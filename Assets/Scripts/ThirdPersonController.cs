using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ThirdPersonController;

public class ThirdPersonController : MonoBehaviour
{
    public static ThirdPersonController instance;

    public enum PlayerView { FirstPerson, ThirdPerson };
    public PlayerView playerView;

    public enum ControlsType { KeyBoardControl, SwipeControl };
    public ControlsType controlsType;

    public Transform cameraTransform, cameraParentTransform;
    public CharacterController characterController;
    public Transform circleImage;

    // public Transform firstPersonCameraTransform;

    public float cameraSensitivity;
    public float moveSpeed, jumpSpeed, gravity;
    public float moveInputDeadZone;
    // public Joystick joystick;

    [Range(0, 1f)]
    public float StartAnimTime = 0.3f;
    [Range(0, 1f)]
    public float StopAnimTime = 0.15f;

    public bool isControllingEnabled;

    [Header("Player View Data")]
    [Header("First Person Transforms")]
    [SerializeField] Vector3 cameraPosition_FP;

    [Header("Third Person Transforms")]
    [SerializeField] Vector3 cameraPosition_TP;

    [Header("Player Data")]
    public GameObject[] headParts;
    public GameObject cameraCollider;

    internal Animator anim;

    int leftFingerId, rightFingerId;
    float halfScreenWidth;

    Vector2 lookInput;
    float cameraPitch, cameraPitch_H;

    Vector2 moveTouchStartPosition;
    Vector2 moveInput;
    float animSpeed;

    bool isJump, isInAir, isRotation, isMovement;
    Vector3 jumpDirection, iniPos, currPos;
    // BNW.CharacterAudioHandler characterAudioHandler;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR || UNITY_WEBGL
        controlsType = ControlsType.KeyBoardControl;
#elif UNITY_ANDROID || UNITY_IOS
        controlsType = ControlsType.SwipeControl;
#endif

        SetPlayerView();
        leftFingerId = -1;
        rightFingerId = -1;
        halfScreenWidth = Screen.width / 2;
        moveInputDeadZone = Mathf.Pow(Screen.height / moveInputDeadZone, 2);

        anim = GetComponent<Animator>();
        iniPos = transform.localPosition;
        currPos = iniPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (controlsType == ControlsType.KeyBoardControl)
        {
            if (isControllingEnabled)
            {
                MoveUsingKeys();
                LookUsingKeys();

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isJump = true;
                }
                JumpControls();
            }
            else if (isRotation)
            {
                LookUsingKeys();
            }
            else if (isMovement)
            {
                MoveUsingKeys();

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isJump = true;
                }
                JumpControls();
            }
            else
            {
                animSpeed = 0;
                anim.SetFloat("Blend", animSpeed, StopAnimTime, Time.deltaTime);
                circleImage.gameObject.SetActive(false);
                leftFingerId = -1;
                rightFingerId = -1;
                jumpDirection.x = 0;
                jumpDirection.y = 0;
            }
        }
        else if (controlsType == ControlsType.SwipeControl)
        {
            if (isControllingEnabled)
            {
                GetTouchInput();

                if (rightFingerId != -1)
                {
                    LookAround();
                }

                if (leftFingerId != -1)
                {
                    Move();
                }
                else
                {
                    animSpeed = 0;
                    anim.SetFloat("Blend", animSpeed, StopAnimTime, Time.deltaTime);
                    circleImage.gameObject.SetActive(false);
                    jumpDirection.x = 0;
                    jumpDirection.y = 0;
                }
                JumpControls();
                isRotation = false;
                isMovement = false;
            }
            else if (isRotation)
            {
                GetTouchInput();

                if (rightFingerId != -1)
                {
                    LookAround();
                }

                animSpeed = 0;
                anim.SetFloat("Blend", animSpeed, StopAnimTime, Time.deltaTime);
                circleImage.gameObject.SetActive(false);
                jumpDirection.x = 0;
                jumpDirection.y = 0;
            }
            else if (isMovement)
            {
                GetTouchInput();

                if (leftFingerId != -1)
                {
                    Move();
                }
                else
                {
                    animSpeed = 0;
                    anim.SetFloat("Blend", animSpeed, StopAnimTime, Time.deltaTime);
                    circleImage.gameObject.SetActive(false);
                    jumpDirection.x = 0;
                    jumpDirection.y = 0;
                }
                JumpControls();
            }
            else
            {
                animSpeed = 0;
                anim.SetFloat("Blend", animSpeed, StopAnimTime, Time.deltaTime);
                circleImage.gameObject.SetActive(false);
                leftFingerId = -1;
                rightFingerId = -1;
                jumpDirection.x = 0;
                jumpDirection.y = 0;
            }
        }
    }

    void GetTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch _touch = Input.GetTouch(i);

            switch (_touch.phase)
            {
                case TouchPhase.Began:
                    // Check if the touch is on the left side of the screen for movement
                    if (_touch.position.x < halfScreenWidth && leftFingerId == -1)
                    {
                        leftFingerId = _touch.fingerId;
                        moveTouchStartPosition = _touch.position;
                        circleImage.gameObject.SetActive(true);
                        circleImage.position = new Vector3(_touch.position.x, _touch.position.y, transform.position.z);
                    }
                    // Check if the touch is on the right side of the screen for rotation
                    else if (_touch.position.x > halfScreenWidth && rightFingerId == -1)
                    {
                        rightFingerId = _touch.fingerId;
                    }
                    break;

                case TouchPhase.Moved:
                    // Handle movement control on the left side
                    if (_touch.fingerId == leftFingerId)
                    {
                        moveInput = _touch.position - moveTouchStartPosition;

                        // Update the movement indicator on the screen
                        circleImage.gameObject.SetActive(true);
                        circleImage.position = new Vector3(_touch.position.x, _touch.position.y, transform.position.z);
                        Move(); // Call move to apply movement control
                    }
                    // Handle rotation control on the right side
                    else if (_touch.fingerId == rightFingerId)
                    {
                        lookInput = _touch.deltaPosition * cameraSensitivity * Time.deltaTime;
                        LookAround(); // Call look to apply rotation control
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    circleImage.gameObject.SetActive(false);
                    // Reset movement if the left finger is lifted
                    if (_touch.fingerId == leftFingerId)
                    {
                        leftFingerId = -1;
                        moveInput = Vector2.zero;
                        animSpeed = 0;
                    }
                    // Reset rotation if the right finger is lifted
                    else if (_touch.fingerId == rightFingerId)
                    {
                        rightFingerId = -1;
                        lookInput = Vector2.zero;
                    }
                    break;

                case TouchPhase.Stationary:
                    if (_touch.fingerId == rightFingerId)
                    {
                        lookInput = Vector2.zero; // Keep lookInput zero if stationary on rotation side
                    }
                    break;
            }
        }
    }


    void GetMouseMovement()
    {
        if (Input.GetMouseButton(1))
        {
            lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * cameraSensitivity;
        }
        else
        {
            lookInput = Vector2.zero;
        }
    }

    public void StopRotation()
    {
        isControllingEnabled = false;
        leftFingerId = -1;
        rightFingerId = -1;
        lookInput = Vector2.zero;
        moveInput = Vector2.zero;
        moveTouchStartPosition = Vector2.zero;
        Move();
    }

    void LookAround()
    {
        cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

        if (isFirstPerson())
        {
            if (cameraParentTransform.localEulerAngles.y >= -70 && cameraParentTransform.localEulerAngles.y <= 80)
            {
                cameraParentTransform.Rotate(transform.up, lookInput.x);
            }
            else
            {
                cameraPitch_H = Mathf.Clamp(cameraParentTransform.localEulerAngles.y, -70, 80);
                transform.eulerAngles = new Vector3(0, cameraParentTransform.eulerAngles.y - cameraPitch_H, 0);
                cameraParentTransform.localEulerAngles = new Vector3(0, cameraPitch_H, 0);
            }
        }
        else
        {
            cameraParentTransform.Rotate(transform.up, lookInput.x);
        }
    }

    void Move()
    {
        if (moveInput.sqrMagnitude <= moveInputDeadZone)
        {
            return;
        }

        if (cameraParentTransform.localEulerAngles != Vector3.zero)
        {
            transform.eulerAngles = cameraParentTransform.eulerAngles;
            cameraParentTransform.localEulerAngles = Vector3.zero;
        }

        Vector2 moveDir = moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        characterController.Move(transform.right * moveDir.x + transform.forward * moveDir.y);

        Vector2 normalizeMoveInput = moveInput;
        normalizeMoveInput.Normalize();
        animSpeed = normalizeMoveInput.sqrMagnitude;
        anim.SetFloat("Blend", animSpeed, StartAnimTime, Time.fixedDeltaTime);

        jumpDirection.x = moveDir.x;
        jumpDirection.y = moveDir.y;
    }

    public void StandPlayer()
    {
        anim.SetTrigger("stand");
    }
    public void StopMovement()
    {
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        animSpeed = 0;
    }

    #region Jump
    public void Click_Jump()
    {
        isJump = true;
    }

    void JumpControls()
    {
        if (characterController.isGrounded)
        {
            isInAir = false;
            currPos = transform.localPosition;
        }

        if (isJump && !isInAir)
        {
            isInAir = true;
            isJump = false;
            if (transform.localPosition.y <= currPos.y + 0.1f)
            {
                jumpDirection.z = jumpSpeed;
                anim.SetTrigger("jump");
            }
        }
        jumpDirection.z -= gravity * Time.deltaTime;
        characterController.Move(transform.right * jumpDirection.x + transform.forward * jumpDirection.y + transform.up * jumpDirection.z);
    }
    #endregion

    #region PlayerView 
    public void ChangePlayerView(bool isFirstPerson)
    {
        if (playerView == PlayerView.FirstPerson)
        {
            playerView = PlayerView.ThirdPerson;
        }
        else
        {
            playerView = PlayerView.FirstPerson;
        }

        SetPlayerView();
    }

    void SetPlayerView()
    {
        if (isFirstPerson())
        {
            // First-person view setup
            cameraTransform.localPosition = cameraPosition_FP;
            Debug.Log("cameraTransform first person: " + cameraTransform.localPosition);
            SetPlayerHead(false);

           
            //ThirdPersonController controller = GetComponent<ThirdPersonController>();
            //if (controller != null)
            //{
            //    controller.enabled = false; // Disable third-person controller
            //}
        }
        else
        {
            // Third-person view setup
           
            //ThirdPersonController controller = GetComponent<ThirdPersonController>();
            //if (controller != null)
            //{
            //    controller.enabled = true; // Enable third-person controller
            //}

            cameraTransform.localPosition = cameraPosition_TP;
            Debug.Log("cameraTransform third person: " + cameraTransform.localPosition);
            SetPlayerHead(true);
        }
    }

    void SetPlayerHead(bool status)
    {
        foreach (var item in headParts)
        {
            item.SetActive(status);
        }
        cameraCollider.SetActive(status);
    }

    internal bool isFirstPerson()
    {
        return (playerView == PlayerView.FirstPerson);
    }
    #endregion

    #region Pc move controls
    void MoveUsingKeys()
    {
        Vector2 movementAxis = Vector2.zero;

        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            movementAxis = Vector2.zero;
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                movementAxis = new Vector2(movementAxis.x, 1);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                movementAxis = new Vector2(movementAxis.x, -1);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                movementAxis = new Vector2(-1, movementAxis.y);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                movementAxis = new Vector2(1, movementAxis.y);
            }
        }

        if (movementAxis.y != 0 || movementAxis.x != 0)
        {
            if (cameraParentTransform.localEulerAngles != Vector3.zero)
            {
                transform.eulerAngles = cameraParentTransform.eulerAngles;
                cameraParentTransform.localEulerAngles = Vector3.zero;
            }

            Vector2 moveDir = movementAxis.normalized * moveSpeed * 2 * Time.deltaTime;
            characterController.Move(transform.right * moveDir.x + transform.forward * moveDir.y);

            Vector2 normalizeMoveInput = movementAxis;
            normalizeMoveInput.Normalize();
            animSpeed = normalizeMoveInput.sqrMagnitude * 2;
            anim.SetFloat("Blend", animSpeed, StartAnimTime, Time.deltaTime);

            jumpDirection.x = moveDir.x;
            jumpDirection.y = moveDir.y;
        }
        else
        {
            animSpeed = 0;
            anim.SetFloat("Blend", animSpeed, StopAnimTime, Time.deltaTime);
            circleImage.gameObject.SetActive(false);
            leftFingerId = -1;
            rightFingerId = -1;
            jumpDirection.x = 0;
            jumpDirection.y = 0;
        }
    }

    void LookUsingKeys()
    {
        GetMouseMovement();

        Vector2 rotationAxis = Vector2.zero;

        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            rotationAxis = Vector2.zero;
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                rotationAxis = new Vector2(rotationAxis.x, 1);
            }
            if (Input.GetKey(KeyCode.S))
            {
                rotationAxis = new Vector2(rotationAxis.x, -1);
            }
            if (Input.GetKey(KeyCode.A))
            {
                rotationAxis = new Vector2(-1, rotationAxis.y);
            }
            if (Input.GetKey(KeyCode.D))
            {
                rotationAxis = new Vector2(1, rotationAxis.y);
            }
        }

        if (rotationAxis.x != 0 || rotationAxis.y != 0)
        {
            lookInput = new Vector2(lookInput.x + rotationAxis.x, lookInput.y + rotationAxis.y) * cameraSensitivity / 4;
        }

        cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        cameraParentTransform.Rotate(transform.up, lookInput.x);
    }
    #endregion
}