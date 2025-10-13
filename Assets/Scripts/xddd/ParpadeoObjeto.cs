using TMPro;
using UnityEngine;

public class ParpadeoObjeto : MonoBehaviour
{
    public float velocidad = 5f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;
    private TMP_Text texto;
    private bool bajando = true;
    private Color colorActual;

    void Start()
    {
        texto = GetComponent<TMP_Text>();
        colorActual = texto.color;
    }

    void Update()
    {
        float alpha = colorActual.a;
        alpha += (bajando ? -1 : 1) * velocidad * Time.deltaTime;

        if (alpha <= minAlpha)
        {
            alpha = minAlpha;
            bajando = false;
        }
        else if (alpha >= maxAlpha)
        {
            alpha = maxAlpha;
            bajando = true;
        }

        colorActual.a = alpha;
        texto.color = colorActual;
    }
}
