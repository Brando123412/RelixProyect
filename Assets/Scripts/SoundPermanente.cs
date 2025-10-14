using UnityEngine;

public class SoundPermanente : MonoBehaviour
{
    private static SoundPermanente instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Esto hace que el objeto no se destruya al cambiar escena
        }
        else
        {
            Destroy(gameObject); // Si ya hay una instancia, destruye la nueva para no duplicar sonidos
        }
    }
}
