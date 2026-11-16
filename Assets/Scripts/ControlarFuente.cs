using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ControlarFuente : MonoBehaviour
{
    public TMP_Text texto;

    void Start()
    {
        GameObject.Find("BotonAumentar").GetComponent<Button>().onClick.AddListener(AumentarFuente);
        GameObject.Find("BotonDisminuir").GetComponent<Button>().onClick.AddListener(DisminuirFuente);
    }

    public void AumentarFuente()
    {
        texto.fontSize += 10;
    }

    public void DisminuirFuente()
    {
        texto.fontSize -= 10;
    }
}