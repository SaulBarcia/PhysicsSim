using UnityEngine;

public class ForestFire : MonoBehaviour
{
    [Header("Dimensiones de la Red")]
    public int width = 200;
    public int height = 200;

    [Header("Parámetros de Simulación (Interactivos)")]
    [Range(0f, 1f)] public float treeDensity = 0.75f;        // Densidad inicial de árboles (cuántos habrá)
    [Range(0f, 1f)] public float baseBurnChance = 0.4f;      // Probabilidad base de propagación (de quemarse)
    [Range(0f, 1f)] public float lightningChance = 0.00005f; // Rayos espontáneos (igual de probable que la token pirata del Fortnite porque quemarán gran área de árboles)
    public Vector2 windDirection = new Vector2(1f, 0f);      // Dirección e intensidad del viento (podrá redirigir el fuego)
    public float updateInterval = 0.08f;                   // Velocidad de los turnos de la simulación (por segundo)

    private int[,] grid; 
    private int[,] nextGrid;
    private Texture2D fireTexture; 
    private MeshRenderer meshRenderer;
    private float timer;

    // Estados de las celdas:
    // 0 = Vacío / Tierra quemada (Negro)
    // 1 = Árbol sano (Verde)
    // 2 = Ardiendo (Rojo / Fuego)

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Se crea una textura dinámica (actualiza mientras simula) para colorear la matriz de celdas
        fireTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        fireTexture.filterMode = FilterMode.Point; // Mantiene el aspecto pixelado de autómata celular
        meshRenderer.material.mainTexture = fireTexture;

        grid = new int[width, height]; //Inicialización de la matriz
        nextGrid = new int[width, height];

        ResetSimulation(); //Reinicia el tiempo a 0 para probar distintos parámetros
    }

    void Update()
    {
        timer += Time.deltaTime; 
        if (timer >= updateInterval) //Controla los tuross 
        {
            timer = 0f;
            StepSimulation();
            UpdateTexture();
        }
    }

    // Mecanismos de la simulación: Propagación, consumo y viento
    void StepSimulation()
    {
        for (int x = 0; x < width; x++) //Recorrer matriz
        {
            for (int y = 0; y < height; y++)
            {
                int current = grid[x, y]; //Asignar current (estado actual)

                if (current == 2) //Si es 2 (Ardiendo)
                {
                    // El árbol se consume y pasa a tierra quemada/vacío
                    nextGrid[x, y] = 0;
                }
                else if (current == 1) //Si es 1 (Sano)
                {
                    bool catchFire = false; //Se mantiene

                    // Comprobar vecinos en las 8 direcciones
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                if (grid[nx, ny] == 2)
                                {
                                    // Efecto del viento: cambia la probabilidad de propagarse según la dirección (redirige el incendio)
                                    float windEffect = (dx * windDirection.x + dy * windDirection.y);
                                    float adjustedChance = baseBurnChance + (windEffect * 0.25f);
                                    adjustedChance = Mathf.Clamp01(adjustedChance);

                                    if (Random.value < adjustedChance)
                                    {
                                        catchFire = true;
                                    }
                                }
                            }
                        }
                    }

                    // Probabilidad de ignición espontánea por rayos
                    if (Random.value < lightningChance)
                    {
                        catchFire = true;
                    }

                    nextGrid[x, y] = catchFire ? 2 : 1; //Según si es verdadero/falso, cambia el estado de la celda en cuestión
                }
                else
                {
                    // Espacio vacío
                    nextGrid[x, y] = 0;
                }
            }
        }

        // Intercambiar buffers de la matriz (actualizarla entera)
        int[,] temp = grid;
        grid = nextGrid;
        nextGrid = temp;
    }

    // Traduce la matriz de enteros a colores en la textura
    void UpdateTexture()
    {
        Color32[] pixels = new Color32[width * height]; //Color32 maneja variables de todos los colores posibles (como es rango de 32 bits, va de 0 a 255)

        for (int y = 0; y < height; y++) //Recorrer matriz
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                int state = grid[x, y];

                if (state == 0) pixels[index] = new Color32(20, 20, 20, 255);       // Quemado/Vacío en un color ennegrecido
                else if (state == 1) pixels[index] = new Color32(34, 139, 34, 255);  // Árbol sano (Verde)
                else if (state == 2) pixels[index] = new Color32(255, 69, 0, 255);   // Ardiendo (Fuego) en un color más rojo
            }
        }

        fireTexture.SetPixels32(pixels); //Asigna el color a los píxeles de la textura (esa textura es la que cambia en la simulación)
        fireTexture.Apply(); //Se aplica la textura al objeto con este script
    }

    // Reinicio y generación aleatoria del bosque (vinculable a un botón UI)
    public void ResetSimulation()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = (Random.value < treeDensity) ? 1 : 0;
            }
        }

        // Foco inicial de fuego en el centro exacto
        grid[width / 2, height / 2] = 2;
        UpdateTexture();
    }
}