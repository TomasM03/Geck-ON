using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    [Header("Movement")]
    public float speed = 5f;
    public float rotationSpeed = 100f;

    [Header("Visual Configuration")]
    public Material localPlayerMaterial;
    public Material remotePlayerMaterial;

    private PhotonView pv;
    private Rigidbody rb;
    private Renderer playerRenderer;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();

        // Configure material based on whether it's our player or not
        ConfigurePlayer();

        // If it's not our player, disable some components for optimization
        if (!pv.IsMine)
        {
            // Disable camera if it has one
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;

            // Disable AudioListener if it has one
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null)
                listener.enabled = false;
        }
    }

    void ConfigurePlayer()
    {
        if (pv.IsMine)
        {
            // It's our player
            if (localPlayerMaterial != null)
                playerRenderer.material = localPlayerMaterial;

            // Change name to identify it
            gameObject.name = "MY_PLAYER_" + PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            // It's a remote player
            if (remotePlayerMaterial != null)
                playerRenderer.material = remotePlayerMaterial;

            // Change name
            gameObject.name = "REMOTE_PLAYER_" + pv.Owner.NickName;
        }
    }

    void Update()
    {
        // Only process input if it's our player
        if (!pv.IsMine)
            return;

        ProcessMovement();
    }

    void ProcessMovement()
    {
        // Get keyboard input
        float horizontal = Input.GetAxis("Horizontal"); // A/D or left/right arrows
        float vertical = Input.GetAxis("Vertical");     // W/S or up/down arrows

        // Free movement in all directions (like a top-down game)
        Vector3 movement = new Vector3(horizontal, 0, vertical) * speed * Time.deltaTime;
        transform.position += movement;

        // Optional: Rotate towards movement direction
        if (movement.magnitude > 0.1f)
        {
            Quaternion newRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void OnGUI()
    {
        // Show controls only for our player
        if (pv.IsMine)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 180, 100));
            GUILayout.Label("Controls:");
            GUILayout.Label("WASD/Arrows - Move");
            GUILayout.Label("Auto rotation");
            GUILayout.EndArea();
        }
    }

    #region IPunObservable - To sync position and rotation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We send our position and rotation
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // We receive position and rotation from other players
            Vector3 position = (Vector3)stream.ReceiveNext();
            Quaternion rotation = (Quaternion)stream.ReceiveNext();

            // Smooth interpolation for fluid movement
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 10);
        }
    }

    #endregion
}