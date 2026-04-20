using UnityEngine;

public class FusibleInteractable : MonoBehaviour
{
    public bool yaRecolectado = false;

    public void ActivarFusible()
    {
        if (yaRecolectado) return;

        yaRecolectado = true;

        Renderer render = GetComponent<Renderer>();
        if (render != null)
        {
            render.material.color = Color.green;
            render.material.EnableKeyword("_EMISSION");
            render.material.SetColor("_EmissionColor", Color.green * 2f); 
        }
    }
}