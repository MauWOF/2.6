using UnityEngine;
using UnityEngine.SceneManagement;

public class Interfaz : MonoBehaviour
{
    public void AbrirUnity() => Application.OpenURL("https://unity.com/");
    public void AbrirGitHub() => Application.OpenURL("https://github.com/");
    public void AbrirGoogle() => Application.OpenURL("https://www.google.com/");
    public void Opciones() => SceneManager.LoadScene("EscenaOpciones");

    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
