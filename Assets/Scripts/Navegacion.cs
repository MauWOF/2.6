using UnityEngine;
using UnityEngine.SceneManagement;

public class Navegacion : MonoBehaviour
{
    public void Volver() => SceneManager.LoadScene("SampleScene");
    public void AlternarPantallaCompleta() => Screen.fullScreen = !Screen.fullScreen;
}
