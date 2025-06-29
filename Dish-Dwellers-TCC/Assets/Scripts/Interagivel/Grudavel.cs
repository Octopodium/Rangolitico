using System;
using UnityEngine;
using UnityEngine.Animations;

public enum LimitacaoDoGrude { GrudaTudo, GrudaX, GrudaY, GrudaZ, GrudaXY, GrudaXZ, GrudaYZ, NaoGruda }

public class Grudavel : MonoBehaviour {
    ParentConstraint parentConstraint; // Para manter o jogador preso a um objeto quando necessário
    Transform grudavelTransform; // Transform do objeto grudável, usado para manter a posição relativa

    void Start() {
        parentConstraint = gameObject.GetComponent<ParentConstraint>();
        if (parentConstraint == null) parentConstraint = gameObject.AddComponent<ParentConstraint>();

        parentConstraint.rotationAxis = Axis.None;
    }
    
    public void Grudar(Transform target, LimitacaoDoGrude limitacao = LimitacaoDoGrude.GrudaTudo, bool manterPosicao = true) {
        if (target == null) return;

        Vector3 offset = manterPosicao ? transform.position - target.position : Vector3.zero;

        if (parentConstraint.sourceCount > 0) parentConstraint.SetSource(0, new ConstraintSource { sourceTransform = target, weight = 1f });
        else parentConstraint.AddSource(new ConstraintSource { sourceTransform = target, weight = 1f });

        grudavelTransform = target;

        parentConstraint.SetTranslationOffset(0, offset);

        parentConstraint.rotationAxis = Axis.None;
        parentConstraint.translationAxis = GetAxis(limitacao);

        parentConstraint.constraintActive = true;
        
    }

    public void Desgrudar() {
        if (parentConstraint == null || parentConstraint.sourceCount == 0) return;
        
        grudavelTransform = null; // Limpa a referência ao grudável
        parentConstraint.constraintActive = false;
        parentConstraint.RemoveSource(0);
    }

    public void Desgrudar(Transform target) {
        if (target == null || target != grudavelTransform) return;
        Desgrudar();
    }

    public Axis GetAxis(LimitacaoDoGrude limitacao) {
        switch (limitacao) {
            case LimitacaoDoGrude.GrudaTudo:
                return Axis.X | Axis.Y | Axis.Z; // Proibe movimento em todos os eixos
            case LimitacaoDoGrude.GrudaX:
                return Axis.X; // Proibe movimento apenas no eixo X
            case LimitacaoDoGrude.GrudaY:
                return Axis.Y; // Proibe movimento apenas no eixo Y
            case LimitacaoDoGrude.GrudaZ:
                return Axis.Z; // Proibe movimento apenas no eixo Z
            case LimitacaoDoGrude.GrudaXY:
                return Axis.X | Axis.Y; // Proibe movimento nos eixos X e Y
            case LimitacaoDoGrude.GrudaXZ:
                return Axis.X | Axis.Z; // Proibe movimento nos eixos X e Z
            case LimitacaoDoGrude.GrudaYZ:
                return Axis.Y | Axis.Z; // Proibe movimento nos eixos Y e Z
            default:
                return Axis.None;
        }
    }
}