using UnityEngine;
using Photon.Pun;

public class PlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    public bool invertY = false;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;

    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    public float rotationSmoothing = 10f;
    public float positionSmoothing = 10f;

    public Transform playerBody;
    public Camera mainCam;

    private PhotonView pv;
    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    private bool canLook = true;

    private Vector3 cameraVelocity;

    void Start()
    {
        pv = GetComponentInParent<PhotonView>();

        if (pv != null && pv.IsMine)
        {
            SetupThirdPersonCamera();
        }
        else
        {
            if (mainCam != null)
                mainCam.enabled = false;
        }
    }

    void SetupThirdPersonCamera()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mainCam != null)
        {
            mainCam.enabled = true;

            AudioListener audioListener = mainCam.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;
            }
            else
            {
                mainCam.gameObject.AddComponent<AudioListener>();
            }
        }

        DisableOtherAudioListeners();

        if (playerBody == null)
        {
            playerBody = transform.parent;
        }

        if (playerBody != null)
        {
            horizontalRotation = playerBody.eulerAngles.y;
        }
        verticalRotation = 20f;

        canLook = true;
    }

    void Update()
    {
        if (pv == null || !pv.IsMine || !canLook)
            return;

        HandleMouseLook();
        HandleInput();
    }

    void LateUpdate()
    {
        if (pv == null || !pv.IsMine || playerBody == null)
            return;

        UpdateCameraPosition();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (invertY)
            mouseY = -mouseY;

        horizontalRotation += mouseX;
        verticalRotation -= mouseY;

        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        if (playerBody != null)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, horizontalRotation, 0f);
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
        }
    }

    void UpdateCameraPosition()
    {
        if (mainCam == null || playerBody == null)
            return;

        Vector3 direction = Quaternion.Euler(verticalRotation, horizontalRotation, 0f) * Vector3.back;
        Vector3 targetPosition = playerBody.position + Vector3.up * cameraHeight + direction * cameraDistance;

        Vector3 playerCenter = playerBody.position + Vector3.up * cameraHeight;
        Vector3 rayDirection = targetPosition - playerCenter;
        float maxDistance = rayDirection.magnitude;

        if (Physics.Raycast(playerCenter, rayDirection.normalized, out RaycastHit hit, maxDistance))
        {
            targetPosition = hit.point - rayDirection.normalized * 0.3f;
        }

        mainCam.transform.position = Vector3.SmoothDamp(
            mainCam.transform.position,
            targetPosition,
            ref cameraVelocity,
            1f / positionSmoothing
        );

        Vector3 lookTarget = playerBody.position + Vector3.up * (cameraHeight * 0.8f);
        mainCam.transform.LookAt(lookTarget);
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            LockCursor();
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            cameraDistance = Mathf.Clamp(cameraDistance - scroll * 2f, 2f, 10f);
        }
    }

    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canLook = true;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        canLook = false;
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }

    public void SetInvertY(bool invert)
    {
        invertY = invert;
    }

    public void SetCameraDistance(float distance)
    {
        cameraDistance = Mathf.Clamp(distance, 1f, 15f);
    }

    void DisableOtherAudioListeners()
    {
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();

        foreach (AudioListener listener in allListeners)
        {
            if (listener.gameObject != mainCam.gameObject)
            {
                listener.enabled = false;
            }
        }
    }

    public void DisableCamera()
    {
        canLook = false;
        if (mainCam != null)
            mainCam.enabled = false;
    }

    public void EnableCamera()
    {
        if (pv != null && pv.IsMine)
        {
            canLook = true;
            if (mainCam != null)
                mainCam.enabled = true;
            LockCursor();
        }
    }
}