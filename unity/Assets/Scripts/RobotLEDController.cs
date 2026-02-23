using UnityEngine;

public class RobotLEDController : MonoBehaviour
{
    private Material ledMaterial;
    private Color targetColor;

    void Awake()
    {
        // Wir greifen auf das Material zu. 
        // WICHTIG: Das Material muss "Emission" aktiviert haben!
        ledMaterial = GetComponent<Renderer>().material;
    }

    public void SetLEDColor(int r, int g, int b)
    {
        // Umrechnung von 0-255 (Python) auf 0.0-1.0 (Unity)
        float red = r / 255f;
        float green = g / 255f;
        float blue = b / 255f;

        targetColor = new Color(red, green, blue);

        // Wir setzen die Emission-Farbe des Materials
        // Das "_EmissionColor" ist der Standard-Name in Unity Shadern
        ledMaterial.SetColor("_EmissionColor", targetColor);
        
        // DynamicGI sorgt dafür, dass das Licht auch die Umgebung anstrahlt
        DynamicGI.SetEmissive(GetComponent<Renderer>(), targetColor);
        
        Debug.Log($"LED Simulation geändert auf: R:{r} G:{g} B:{b}");
    }
}