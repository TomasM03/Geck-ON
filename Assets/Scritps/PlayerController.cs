using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float velocidadRotacion = 100f;

    [Header("Configuración Visual")]
    public Material materialJugadorLocal;
    public Material materialJugadorRemoto;

    private PhotonView pv;
    private Rigidbody rb;
    private Renderer playerRenderer;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();

        // Configurar el material según si es nuestro jugador o no
        ConfigurarJugador();

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
        // If it's not our player, disable some components for optimization
=======
        // Si no es nuestro jugador, desactivar algunos componentes para optimizar
>>>>>>> parent of 2d2f8a7 (Nickname/TMP)
=======
        // Si no es nuestro jugador, desactivar algunos componentes para optimizar
>>>>>>> parent of 2d2f8a7 (Nickname/TMP)
=======
        // Si no es nuestro jugador, desactivar algunos componentes para optimizar
>>>>>>> parent of 2d2f8a7 (Nickname/TMP)
        if (!pv.IsMine)
        {
            // Desactivar la cámara si tiene una
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;

            // Desactivar el AudioListener si tiene uno
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null)
                listener.enabled = false;
        }
    }

    void ConfigurarJugador()
    {
        if (pv.IsMine)
        {
            // Es nuestro jugador
            if (materialJugadorLocal != null)
                playerRenderer.material = materialJugadorLocal;

            // Cambiar el nombre para identificarlo
            gameObject.name = "MI_JUGADOR_" + PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            // Es un jugador remoto
            if (materialJugadorRemoto != null)
                playerRenderer.material = materialJugadorRemoto;

            // Cambiar el nombre
            gameObject.name = "JUGADOR_REMOTO_" + pv.Owner.NickName;
        }
    }

    void Update()
    {
        // Solo procesar input si es nuestro jugador
        if (!pv.IsMine)
            return;

        ProcesarMovimiento();
    }

    void ProcesarMovimiento()
    {
        // Obtener input del teclado
        float horizontal = Input.GetAxis("Horizontal"); // A/D o flechas izquierda/derecha
        float vertical = Input.GetAxis("Vertical");     // W/S o flechas arriba/abajo

        // Movimiento libre en todas las direcciones (como un juego de vista superior)
        Vector3 movimiento = new Vector3(horizontal, 0, vertical) * velocidad * Time.deltaTime;
        transform.position += movimiento;

        // Opcional: Rotar hacia la dirección de movimiento
        if (movimiento.magnitude > 0.1f)
        {
            Quaternion nuevaRotacion = Quaternion.LookRotation(movimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, nuevaRotacion, Time.deltaTime * velocidadRotacion);
        }
    }

    void OnGUI()
    {
        // Mostrar controles solo para nuestro jugador
        if (pv.IsMine)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 180, 100));
            GUILayout.Label("Controles:");
            GUILayout.Label("W/S - Adelante/Atrás");
            GUILayout.Label("A/D - Rotar");
            GUILayout.EndArea();
        }
    }

    #region IPunObservable - Para sincronizar posición y rotación

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Enviamos nuestra posición y rotación
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Recibimos la posición y rotación de otros jugadores
            Vector3 posicion = (Vector3)stream.ReceiveNext();
            Quaternion rotacion = (Quaternion)stream.ReceiveNext();

            // Interpolación suave para movimiento fluido
            transform.position = Vector3.Lerp(transform.position, posicion, Time.deltaTime * 10);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacion, Time.deltaTime * 10);
        }
    }

    #endregion
}