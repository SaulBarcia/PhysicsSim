using UnityEngine;
using UnityEngine.UI;

public class SimulationUI : MonoBehaviour
{
    public ForestFire simulation;

    public Slider densitySlider;
    public Slider windSlider;
    public Button resetButton;

    void Start()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);
    }

    void Update()
    {
        // Actualizar parámetros en tiempo real desde los sliders de la UI
        if (densitySlider != null)
            simulation.treeDensity = densitySlider.value;

        if (windSlider != null)
        {
            // El slider modifica la fuerza del viento horizontalmente
            simulation.windDirection = new Vector2(windSlider.value, 0f);
        }
    }

    void OnResetClicked()
    {
        simulation.ResetSimulation();
    }
}