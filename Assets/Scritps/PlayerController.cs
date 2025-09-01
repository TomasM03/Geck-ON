using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    public float speed = 5f;
    public float rotationSpeed = 100f;

    public Material matLocalPlayer;
    public Material matOtherPlayer;

    private PhotonView pv;
    private Rigidbody rb;
    private Renderer playerRenderer;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();

        PlayerSets();

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
            if (matLocalPlayer != null)
                playerRenderer.material = matLocalPlayer;

            gameObject.name = "Player_" + PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            if (matOtherPlayer != null)
                playerRenderer.material = matOtherPlayer;

            gameObject.name = "Player_" + pv.Owner.NickName;
        }
    }

    void Update()
    {
        if (!pv.IsMine)
            return;

        Movement();
    }

    void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical"); 

        Vector3 movimiento = new Vector3(horizontal, 0, vertical) * speed * Time.deltaTime;
        transform.position += movimiento;

        if (movimiento.magnitude > 0.1f)
        {
            Quaternion nuevaRotacion = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, nuevaRotacion, Time.deltaTime * rotationSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            Vector3 posicion = (Vector3)stream.ReceiveNext();
            Quaternion rotacion = (Quaternion)stream.ReceiveNext();

            transform.position = Vector3.Lerp(transform.position, posicion, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacion, Time.deltaTime * 10);
        }
    }

}