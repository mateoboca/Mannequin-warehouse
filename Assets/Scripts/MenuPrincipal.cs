using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuPrincipal : MonoBehaviour
{
    public void Jugar()
    {

        SceneManager.LoadScene("Juego");
    }

    public void Salir()
    {

        Debug.Log("¡Cerrando el juego!");
        Application.Quit();
    }
}