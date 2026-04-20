using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI textoFusibles;
    public GameObject panelVictoria;

    [Header("Menú de Pausa")]
    public GameObject panelPausa;

    public int fusiblesRecolectados = 0;
    private int fusiblesTotales = 5;

    private bool juegoPausado = false;
    private bool juegoTerminado = false; 

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        ActualizarUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !juegoTerminado)
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Pausar()
    {
        juegoPausado = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        juegoPausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }


    public void RecolectarFusible()
    {
        fusiblesRecolectados++;
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoFusibles != null)
        {
            textoFusibles.text = "Fusibles: " + fusiblesRecolectados + " / " + fusiblesTotales;
        }
    }

    public void IntentarAbrirPuerta()
    {
        if (fusiblesRecolectados >= fusiblesTotales)
        {
            juegoTerminado = true; 
            panelVictoria.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
        else
        {
            Debug.Log("Faltan fusibles. Tenés: " + fusiblesRecolectados);
        }
    }
}