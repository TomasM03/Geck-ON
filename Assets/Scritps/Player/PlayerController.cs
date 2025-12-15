using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float slideForce = 10f;
    public float slideDuration = 0.8f;

    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask = 1;

    public float standingHeight = 2f;
    public float crouchHeight = 1f;

    public Animator animator;
    public float animationSmoothTime = 0.1f;

    private PhotonView pv;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    private bool isGrounded;
    private bool isRunning;
    private bool isCrouching;
    private bool isSliding;
    private float slideTimer;

    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float currentSpeed;
    private float speedVelocity; 
    
    private float airTimer;
    public float minAirTime = 0.3f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsSlidingHash = Animator.StringToHash("IsSliding");
    private static readonly int CrouchWalkHash = Animator.StringToHash("CrouchWalk");

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (pv.IsMine)
        {
            gameObject.name = "Player_" + PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            gameObject.name = "Player_" + pv.Owner.NickName;

            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;
        }

        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Update()
    {
        if (pv.IsMine)
        {
            GroundCheck();
            HandleCrouch();
            HandleMovement();
            HandleJump();
            UpdateAnimator();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
        }
    }

    void GroundCheck()
    {
        if (airTimer > 0)
        {
            airTimer -= Time.deltaTime;
            isGrounded = false;
            return;
        }

        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        else
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
    }

    void HandleCrouch()
    {
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isRunning && isGrounded)
            {
                StartSlide();
                return;
            }
        }

        if (Input.GetKey(KeyCode.LeftControl) && !isSliding)
        {
            if (!isCrouching)
            {
                StartCrouch();
            }
        }
        else if (!isSliding)
        {
            if (isCrouching)
            {
                EndCrouch();
            }
        }
    }

    void StartCrouch()
    {
        isCrouching = true;
        if (capsule != null)
        {
            capsule.height = crouchHeight;
            capsule.center = new Vector3(0, crouchHeight / 2f, 0);
        }
    }

    void EndCrouch()
    {
        isCrouching = false;
        if (capsule != null)
        {
            capsule.height = standingHeight;
            capsule.center = new Vector3(0, standingHeight / 2f, 0);
        }
    }

    void StartSlide()
    {
        isSliding = true;
        isCrouching = true;
        slideTimer = slideDuration;

        if (capsule != null)
        {
            capsule.height = crouchHeight;
            capsule.center = new Vector3(0, crouchHeight / 2f, 0);
        }

        Vector3 slideDirection = transform.forward;
        rb.AddForce(slideDirection * slideForce, ForceMode.VelocityChange);
    }

    void EndSlide()
    {
        isSliding = false;

        if (!Input.GetKey(KeyCode.LeftControl))
        {
            EndCrouch();
        }
    }

    void HandleMovement()
    {
        if (isSliding) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        float speed;
        if (isCrouching)
            speed = crouchSpeed;
        else if (isRunning)
            speed = runSpeed;
        else
            speed = walkSpeed;

        Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 move = direction * speed;
            move.y = rb.velocity.y;
            rb.velocity = move;
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching && !isSliding)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            isGrounded = false;
            airTimer = minAirTime;

            if (animator != null)
                animator.Play("Jump");
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float targetSpeed = horizontalVel.magnitude;

        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, animationSmoothTime);

        animator.SetFloat(SpeedHash, currentSpeed);
        animator.SetBool(IsGroundedHash, isGrounded);
        animator.SetBool(IsRunningHash, isRunning);
        animator.SetBool(IsCrouchingHash, isCrouching);
        animator.SetBool(IsSlidingHash, isSliding);

        bool isCrouchWalking = isCrouching && !isSliding && currentSpeed > 0.1f;
        animator.SetBool(CrouchWalkHash, isCrouchWalking);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
            stream.SendNext(isRunning);
            stream.SendNext(isCrouching);
            stream.SendNext(isSliding);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            Vector3 vel = (Vector3)stream.ReceiveNext();
            isRunning = (bool)stream.ReceiveNext();
            isCrouching = (bool)stream.ReceiveNext();
            isSliding = (bool)stream.ReceiveNext();

            if (rb != null)
                rb.velocity = Vector3.Lerp(rb.velocity, vel, Time.deltaTime * 10f);

            if (animator != null)
            {
                animator.SetBool(IsRunningHash, isRunning);
                animator.SetBool(IsCrouchingHash, isCrouching);
                animator.SetBool(IsSlidingHash, isSliding);
            }
        }
    }

    public bool IsGrounded() { return isGrounded; }
    public bool IsRunning() { return isRunning; }
    public bool IsCrouching() { return isCrouching; }
    public bool IsSliding() { return isSliding; }
}