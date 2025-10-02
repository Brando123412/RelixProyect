using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración del Jugador")]
    [SerializeField] private float fuerzaSalto = 10f;
    [SerializeField] private int vidasIniciales = 2;

    [Header("Verificación de Suelo (Raycast)")]
    [SerializeField] private Transform puntoVerificacionSuelo;
    [SerializeField] private float distanciaRaycastSuelo = 0.3f;
    [SerializeField] private LayerMask capaSuelo;

    [Header("UI")]
    [SerializeField] private Text textoPuntuacion;
    [SerializeField] private Image[] iconosVidas;

    private Rigidbody2D rb;
    private bool estaEnSuelo;
    private int vidasActuales;
    private float puntuacion;
    private bool juegoTerminado;

    // Propiedades públicas
    public int Vidas => vidasActuales;
    public float Puntuacion => puntuacion;
    public bool IsGameOver => juegoTerminado;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Falta Rigidbody2D en el jugador.");
            enabled = false;
            return;
        }
        if (puntoVerificacionSuelo == null)
        {
            Debug.LogError("Debes asignar el 'puntoVerificacionSuelo' en el inspector.");
            enabled = false;
            return;
        }

        vidasActuales = vidasIniciales;
        puntuacion = 0;
        ActualizarUI();
    }

    void Update()
    {
        if (juegoTerminado) return;

        // --- Chequear suelo con Raycast ---
        RaycastHit2D hit = Physics2D.Raycast(
            puntoVerificacionSuelo.position,
            Vector2.down,
            distanciaRaycastSuelo,
            capaSuelo
        );
        estaEnSuelo = (hit.collider != null);

        // --- Salto ---
        if (Input.GetButtonDown("Jump") && estaEnSuelo)
        {
            Saltar();
        }

        // --- Puntuación ---
        puntuacion += Time.deltaTime * 10f;
        ActualizarPuntuacionUI();
    }

    void FixedUpdate()
    {
        if (juegoTerminado) return;

        // Sin movimiento horizontal propio
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (juegoTerminado) return;

        if (collision.gameObject.CompareTag("Obstaculo"))
        {
            RecibirDano();
        }
    }

    public void RecibirDano()
    {
        if (juegoTerminado) return;

        vidasActuales--;
        ActualizarUI();

        if (vidasActuales <= 0) GameOver();
        else Debug.Log("Vida perdida. Restantes: " + vidasActuales);
    }

    private void ActualizarPuntuacionUI()
    {
        if (textoPuntuacion != null)
            textoPuntuacion.text = "PUNTAJE: " + Mathf.FloorToInt(puntuacion);
    }

    private void ActualizarUI()
    {
        ActualizarPuntuacionUI();
        for (int i = 0; i < iconosVidas.Length; i++)
        {
            if (iconosVidas[i] != null)
                iconosVidas[i].enabled = (i < vidasActuales);
        }
    }

    private void GameOver()
    {
        juegoTerminado = true;
        Debug.Log("¡Juego Terminado! Puntaje final: " + Mathf.FloorToInt(puntuacion));

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.isKinematic = true;
    }

    void OnDrawGizmos()
    {
        if (puntoVerificacionSuelo == null) return;

        Gizmos.color = estaEnSuelo ? Color.green : Color.red;
        Gizmos.DrawLine(
            puntoVerificacionSuelo.position,
            puntoVerificacionSuelo.position + Vector3.down * distanciaRaycastSuelo
        );
    }
}
