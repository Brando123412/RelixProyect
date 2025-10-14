using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float fuerzaSalto = 7f;
    public float distanciaRaycastObstaculo = 0.4f;
    public LayerMask capaObstaculo;

    [Header("Vida / PowerUp")]
    public float duracionInmortalidad = 3f;

    [Header("Efectos visuales")]
    public ParticleSystem particulaInmortalidad; // Partícula que se activa durante la inmortalidad
    public SpriteRenderer spriteRenderer;        // Sprite del jugador
    public Color colorNormal = Color.white;
    public Color colorInmortal = Color.magenta;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool estaEnSuelo = false;
    private bool estaEnTunel = false;
    [SerializeField] bool esInmortal = false;
    private Collider2D obstaculoActual;

    [SerializeField] GameObject tarea2, tarea3;
    [SerializeField] GameObject tarea2_2, tarea3_3;
    [SerializeField] GameObject damage;

    [SerializeField] private Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (particulaInmortalidad != null)
            particulaInmortalidad.Stop();
    }

    void Update()
    {
        DetectarObstaculoDebajo();

        // --- Input para saltar ---
        bool saltoInput = Input.GetButtonDown("Jump");
        bool clickInput = Input.GetMouseButtonDown(0);
        bool touchInput = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;

        if ((saltoInput || clickInput || touchInput) && estaEnSuelo && !estaEnTunel)
        {
            Saltar();
        }
        ActualizarAnimaciones();
    }
    private void ActualizarAnimaciones()
    {
        if (animator == null) return;

        if (!estaEnSuelo)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                print("Hola");
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
            }
            else if (rb.linearVelocity.y < -0.1f)
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", true);
            }
            else
            {
                animator.SetBool("isJumping", false);
                animator.SetBool("isFalling", false);
            }
        }
        else
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
    }


    private void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
    }

    private void DetectarObstaculoDebajo()
    {
        Vector2 origen = new Vector2(transform.position.x, col.bounds.min.y);
        Vector2 direccion = Vector2.down;

        RaycastHit2D hit = Physics2D.Raycast(origen, direccion, distanciaRaycastObstaculo, capaObstaculo);

        if (hit.collider != null)
        {
            Collider2D obstaculo = hit.collider;
            if (obstaculo != obstaculoActual && !estaEnSuelo)
            {
                RestaurarObstaculoAnterior();
                obstaculoActual = obstaculo;
                obstaculo.isTrigger = false;
                estaEnSuelo = true;
            }
        }
        else
        {
            RestaurarObstaculoAnterior();
            estaEnSuelo = false;
        }

        Debug.DrawRay(origen, direccion * distanciaRaycastObstaculo, Color.red);
    }

    private void RestaurarObstaculoAnterior()
    {
        if (obstaculoActual != null)
        {
            obstaculoActual.isTrigger = true;
            obstaculoActual = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Suelo"))
            estaEnSuelo = true;

        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Suelo"))
            estaEnSuelo = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Tunel"))
            estaEnTunel = true;

        if (collision.CompareTag("PowerUp"))
        {
            Destroy(collision.gameObject);
            StartCoroutine(ActivarInmortalidad());
        }

        if (collision.CompareTag("Points"))
        {
            Destroy(collision.gameObject);
            GameController.Instance.AddScore(10);
        }

        /*if (collision.CompareTag("Obstaculo") && !esInmortal)
        {
            DamegeVisual();
            GameController.Instance.LoseLife();
        }*/

        if (collision.CompareTag("Tarea"))
        {
            rb.linearVelocity = Vector2.zero;
            ParticleSystem particulas = collision.GetComponentInChildren<ParticleSystem>();
            StartCoroutine(RealizarTarea(particulas));
        }
        if (collision.CompareTag("Tarea2"))
        {
            Time.timeScale = 0f;
            tarea2.SetActive(true);
        }
        if (collision.CompareTag("Tarea3"))
        {
            Time.timeScale = 0f;
            tarea3.SetActive(true);
        }
        if (collision.CompareTag("Tarea2.2"))
        {
            Time.timeScale = 0f;
            tarea2_2.SetActive(true);
        }
        if (collision.CompareTag("Tarea3.3"))
        {
            Time.timeScale = 0f;
            tarea3_3.SetActive(true);
        }
        if (collision.CompareTag("Final"))
        {
            GameController.Instance.GameOver();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Tunel"))
            estaEnTunel = false;
    }

    private void RecibirDano()
    {
        GameController.Instance.LoseLife();
    }
    private IEnumerator DamegeVisual()
    {
        print("Holaaaa");
        damage.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        damage.SetActive(false);
    }
    private IEnumerator ActivarInmortalidad()
    {
        esInmortal = true;
        Debug.Log("Inmortalidad activada!");

        // Guardar velocidad original
        float velocidadOriginal = InfiniteBackgroundAuto.Instance.scrollSpeed;

        // Aumentar velocidad durante la inmortalidad
        InfiniteBackgroundAuto.Instance.scrollSpeed = velocidadOriginal * 1.5f;

        // Activar partículas
        if (particulaInmortalidad != null)
            particulaInmortalidad.Play();

        // Iniciar parpadeo de color
        StartCoroutine(ParpadearColor());

        yield return new WaitForSeconds(duracionInmortalidad);

        // Volver a la velocidad original
        InfiniteBackgroundAuto.Instance.scrollSpeed = velocidadOriginal;

        esInmortal = false;

        if (particulaInmortalidad != null)
            particulaInmortalidad.Stop();

        if (spriteRenderer != null)
            spriteRenderer.color = colorNormal;

        Debug.Log("Inmortalidad terminada!");
    }


    private IEnumerator ParpadearColor()
    {
        float velocidadCambio = 1.5f; // velocidad del parpadeo
        float t = 0f;

        while (esInmortal)
        {
            t += Time.deltaTime * velocidadCambio;
            float factor = (Mathf.Sin(t * Mathf.PI) + 1f) / 2f; // valor entre 0 y 1
            spriteRenderer.color = Color.Lerp(colorNormal, colorInmortal, factor);
            yield return null;
        }
    }

    // === TAREAS ===
    private IEnumerator RealizarTarea(ParticleSystem value)
    {
        Time.timeScale = 0f;
        estaEnTunel = true;

        TaskUI.Instance.MostrarPanel(() =>
        {
            Debug.Log("Tarea completada!");
            estaEnTunel = false;
        });

        while (estaEnTunel)
            yield return null;

        GameController.Instance.AddScore(40);
        value.Stop();
        Time.timeScale = 1f;
    }
}
