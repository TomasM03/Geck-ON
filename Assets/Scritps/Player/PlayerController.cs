using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;

    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask = 1;

    public Material matLocalPlayer;
    public Material matOtherPlayer;

    public Animator animator;
    public float animationSmoothTime = 0.1f;

    private PhotonView pv;
    private Rigidbody rb;
    private Renderer playerRenderer;

    private bool isGrounded;
    private bool isRunning;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    private float currentSpeed;
    private float speedVelocity;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (pv.IsMine)
        {
            if (matLocalPlayer != null && playerRenderer != null)
                playerRenderer.material = matLocalPlayer;
            gameObject.name = "Player_" + PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            if (matOtherPlayer != null && playerRenderer != null)
                playerRenderer.material = matOtherPlayer;
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
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        else
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;

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
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            isGrounded = false;

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
    }

    public void TriggerDeathAnimation()
    {
        if (animator != null)
            animator.SetBool(IsDeadHash, true);
    }

    public void ResetDeathAnimation()
    {
        if (animator != null)
            animator.SetBool(IsDeadHash, false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rb.velocity);
            stream.SendNext(isRunning);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            Vector3 vel = (Vector3)stream.ReceiveNext();
            isRunning = (bool)stream.ReceiveNext();

            if (rb != null)
                rb.velocity = Vector3.Lerp(rb.velocity, vel, Time.deltaTime * 10f);
        }
    }

    public bool IsGrounded() { return isGrounded; }
    public bool IsRunning() { return isRunning; }
    public float GetCurrentSpeed() { return isRunning ? runSpeed : walkSpeed; }
}