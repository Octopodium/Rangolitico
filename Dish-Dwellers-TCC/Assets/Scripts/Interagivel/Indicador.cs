using UnityEngine;
using UnityEngine.Animations;

public class Indicador : MonoBehaviour {
    public ParentConstraint parentConstraint;
    int sourceId = -1;
    public Interagivel interagivel { get; private set; }
    public bool ativo => gameObject.activeSelf;

    void Awake() {
        /*parentConstraint = GetComponent<ParentConstraint>();
        if (parentConstraint == null) parentConstraint = gameObject.AddComponent<ParentConstraint>();*/
        parentConstraint.rotationAxis = Axis.None; // Desabilita rotação ao ser pego
    }

    public void Mostrar(Interagivel interagivel) {
        if (interagivel == null) return;
        if (this.interagivel == interagivel) return;
        if (this.interagivel != null) RemoverUltimo();

        this.interagivel = interagivel;

        sourceId = parentConstraint.AddSource(new ConstraintSource() {
            sourceTransform = interagivel.transform,
            weight = 1f
        });

        parentConstraint.SetTranslationOffset(sourceId, interagivel.offsetIndicador);
        parentConstraint.constraintActive = true;

        gameObject.SetActive(true);
    }

    public void Esconder(Interagivel interagivel) {
        if (this.interagivel == interagivel) Esconder();
    }

    public void Esconder() {
        RemoverUltimo();

        gameObject.SetActive(false);
        interagivel = null;
    }

    void RemoverUltimo() {
        if (sourceId != -1) {
            parentConstraint.RemoveSource(sourceId);
            parentConstraint.constraintActive = false;
            sourceId = -1;
        }
    }
}
