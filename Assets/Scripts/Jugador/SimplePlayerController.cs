using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("UI Interacción")]
    public GameObject interactPrompt;

    private float xRotation = 0f;
    private CharacterController cc;
    private Vector3 velocity;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        if (cc.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        cc.Move(move * speed * Time.deltaTime);

        RaycastHit hit;
        bool lookingAtInteractable = Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, 3f)
                                     && (hit.collider.CompareTag("Fusible") || hit.collider.CompareTag("Puerta"));

        if (interactPrompt != null)
            interactPrompt.SetActive(lookingAtInteractable);

        if (Input.GetKeyDown(KeyCode.E) && lookingAtInteractable)
        {
            if (hit.collider.CompareTag("Fusible"))
            {
                FusibleInteractable fusible = hit.collider.GetComponent<FusibleInteractable>();
                if (fusible != null && !fusible.yaRecolectado)
                {
                    fusible.ActivarFusible();
                    GameManager.Instancia.RecolectarFusible();
                }
            }
            else if (hit.collider.CompareTag("Puerta"))
            {
                GameManager.Instancia.IntentarAbrirPuerta();
            }
        }
    }
}