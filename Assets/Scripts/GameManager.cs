using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Agregamos esto para poder cambiar de escena

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI textoFusibles;
    public GameObject panelVictoria;

    [Header("Menú de Pausa")]
    public GameObject panelPausa; // Arrastrar el PanelPausa acá

    public int fusiblesRecolectados = 0;
    private int fusiblesTotales = 5;

    // Variables de control
    private bool juegoPausado = false;
    private bool juegoTerminado = false; // Evita que pauses si ya ganaste

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        ActualizarUI();
    }

    // NUEVO: Escuchamos la tecla ESC todo el tiempo
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

    // --- LÓGICA DE PAUSA ---

    public void Pausar()
    {
        juegoPausado = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f; // Congela todo (jugador, enemigo, partículas)

        // Liberamos el mouse para poder hacer clic en los botones
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Reanudar()
    {
        juegoPausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f; // Descongela el tiempo

        // Volvemos a bloquear el mouse para el juego en primera persona
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void VolverAlMenu()
    {
        // ¡TRUCO CLAVE! Hay que descongelar el tiempo antes de cambiar de escena
        // Si no, el menú principal va a cargar con el tiempo congelado y nada va a funcionar.
        Time.timeScale = 1f;

        // Poné el nombre exacto de tu escena del menú acá:
        SceneManager.LoadScene("MenuPrincipal");
    }

    // --- LÓGICA DE FUSIBLES (Se mantiene igual) ---

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
            juegoTerminado = true; // Bloquea el botón ESC
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