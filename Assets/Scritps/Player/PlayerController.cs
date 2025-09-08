using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask = 1;

    public Material matLocalPlayer;
    public Material matOtherPlayer;

    private PhotonView pv;
    private Rigidbody rb;
    private Renderer playerRenderer;
    private PlayerCamera playerCam;

    private bool isGrounded;
    private bool isRunning;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        playerCam = GetComponentInChildren<PlayerCamera>();

        PlayerSets();

        networkPosition = transform.position;
        networkRotation = transform.rotation;

        if (!pv.IsMine)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;
        }
    }

    void PlayerSets()
    {
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
        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            GroundCheck();
            HandleMovement();
            HandleJump();
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
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveVector = direction * currentSpeed;

            moveVector.y = rb.velocity.y;

            rb.velocity = moveVector;
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
        }
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
            Vector3 networkVelocity = (Vector3)stream.ReceiveNext();
            bool networkIsRunning = (bool)stream.ReceiveNext();

            if (rb != null)
            {
                rb.velocity = Vector3.Lerp(rb.velocity, networkVelocity, Time.deltaTime * 10f);
            }

            isRunning = networkIsRunning;
        }
    }

    public void RotatePlayer(float yRotation)
    {
        if (pv.IsMine)
        {
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    public float GetCurrentSpeed()
    {
        return isRunning ? runSpeed : walkSpeed;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}