using UnityEngine;

public sealed class SeleccionAnimacion : MonoBehaviour
{
    static readonly int Seleccionado = Animator.StringToHash("Seleccionado");
    [SerializeField] Animator[] animadores;

    public void Seleccionar(Animator elegido)
    {
        foreach (Animator animador in animadores)
            animador.SetBool(Seleccionado, animador == elegido);
    }
}
