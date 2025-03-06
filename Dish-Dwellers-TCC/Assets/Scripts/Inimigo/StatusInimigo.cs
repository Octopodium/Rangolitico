using UnityEngine;

[CreateAssetMenu(fileName = "StatusInimigo", menuName = "Scriptable Objects/StatusInimigo")]
public class StatusInimigo : ScriptableObject
{
    [Header("Valores genéricos dos inimigos")]
    [Space(10)]
    public int vidas;
    public int dano;
    public float velocidade;

    [Header("Valores de zonas de percepção do player")]
    [Space(10)]
    public float campoDeVisao;
    public float zonaDeAtaque;
}
