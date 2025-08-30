using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNameDisplay : MonoBehaviour
{
    [Header("Configuración del Nombre")]
    public GameObject nombrePrefab; // Prefab con TextMeshPro
    public Vector3 offsetPosicion = new Vector3(0, 2f, 0);
    public bool mirarCamara = true;

    [Header("Colores")]
    public Color colorJugadorLocal = Color.green;
    public Color colorJugadorRemoto = Color.white;

    private PhotonView pv;
    private GameObject nombreUI;
    private TextMeshPro nombreTexto;
    private Camera camaraPrincipal;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        camaraPrincipal = Camera.main;

        CrearNombreUI();
    }

    void CrearNombreUI()
    {
        if (nombrePrefab == null)
        {
            CrearNombreBasico();
        }
        else
        {
            nombreUI = Instantiate(nombrePrefab, transform);
            nombreTexto = nombreUI.GetComponent<TextMeshPro>();
        }

        if (nombreTexto != null)
        {
            ConfigurarNombre();
        }
    }

    void CrearNombreBasico()
    {
        nombreUI = new GameObject("NombreJugador");
        nombreUI.transform.SetParent(transform);
        nombreUI.transform.localPosition = offsetPosicion;

        nombreTexto = nombreUI.AddComponent<TextMeshPro>();
        nombreTexto.text = "Nombre";
        nombreTexto.fontSize = 3;
        nombreTexto.alignment = TextAlignmentOptions.Center;
        nombreTexto.sortingOrder = 10;
    }

    void ConfigurarNombre()
    {
        if (pv != null && nombreTexto != null)
        {
            nombreTexto.text = pv.Owner.NickName;

            if (pv.IsMine)
            {
                nombreTexto.color = colorJugadorLocal;
                nombreTexto.text = nombreTexto.text + " (TÚ)";
            }
            else
            {
                nombreTexto.color = colorJugadorRemoto;
            }

            nombreUI.transform.localPosition = offsetPosicion;
        }
    }

    void LateUpdate()
    {
        if (mirarCamara && nombreUI != null && camaraPrincipal != null)
        {
            nombreUI.transform.LookAt(camaraPrincipal.transform);

            nombreUI.transform.Rotate(0, 180, 0);
        }
    }

    void OnDestroy()
    {
        if (nombreUI != null)
        {
            Destroy(nombreUI);
        }
    }

    public void ActualizarNombre(string nuevoNombre)
    {
        if (nombreTexto != null)
        {
            nombreTexto.text = nuevoNombre;

            if (pv.IsMine)
            {
                nombreTexto.text += " (TÚ)";
            }
        }
    }

    public void MostrarNombre(bool mostrar)
    {
        if (nombreUI != null)
        {
            nombreUI.SetActive(mostrar);
        }
    }
}