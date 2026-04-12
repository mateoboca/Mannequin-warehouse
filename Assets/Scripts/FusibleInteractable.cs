using UnityEngine;

public class FusibleInteractable : MonoBehaviour
{
    public bool yaRecolectado = false;

    public void ActivarFusible()
    {
        // Si ya lo agarramos antes, no hace nada
        if (yaRecolectado) return;

        yaRecolectado = true;

        // Cambiamos el color del material a verde
        Renderer render = GetComponent<Renderer>();
        if (render != null)
        {
            render.material.color = Color.green;

            // Le prendemos la "Emisión" para que parezca que tiene una luz propia
            render.material.EnableKeyword("_EMISSION");
            render.material.SetColor("_EmissionColor", Color.green * 2f); // El *2f lo hace brillar más
        }
    }
}