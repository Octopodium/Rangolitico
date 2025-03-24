using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Raquete : MonoBehaviour, Ferramenta {
    List<Projectile> projeteisEmArea = new List<Projectile>();

    Vector3 direcaoProtecao = Vector3.zero;
    Player jogador;

    public void Inicializar(Player jogador) {
        this.jogador = jogador;
    }

    public void Acionar() {
        jogador.inputActionMap["Move"].performed += Raquetou;
        
        
        this.jogador.MostrarDirecional(true);
    }

    public void Soltar() {
        jogador.inputActionMap["Move"].performed -= Raquetou;
        
        this.jogador.MostrarDirecional(false);
    }

    public void Raquetou(InputAction.CallbackContext ctx) {
        Raquetou(jogador.inputActionMap["Move"].ReadValue<Vector2>());
    }

    void ClearNullList() {
        for (int i = projeteisEmArea.Count - 1; i >= 0; i--) {
            Projectile proj = projeteisEmArea[i];
            if (proj == null || proj.gameObject) projeteisEmArea.RemoveAt(i);
        }
    }

    public bool Raquetou(Vector2 dir) {
        ClearNullList();

        Vector2 playerDir = new Vector2(jogador.direcao.x, jogador.direcao.z);
        float angulo = Vector2.Angle(dir, playerDir);

        Debug.Log(projeteisEmArea.Count);

        if (angulo > 90 || projeteisEmArea.Count == 0) return false;

        // Pegar projetil mais proximo
        Projectile projetil = null;
        float proximidade = float.MaxValue;
        foreach (Projectile proj in projeteisEmArea) {

            float dist = Vector3.Distance(jogador.transform.position, proj.transform.position);
            if (dist < proximidade) {
                proximidade = dist;
                projetil = proj;
            }
        }

        if (proximidade == float.MaxValue) return false;

        projetil.MudarDirecao(dir);

        return true;
    }
    
    void FixedUpdate() {
        direcaoProtecao.x = jogador.direcao.x;
        direcaoProtecao.z = jogador.direcao.z;
/*
        if (direcaoProtecao.magnitude > 0)
            protecao.transform.forward = direcaoProtecao;*/
    }

    void OnTriggerEnter(Collider col) {
        Projectile projetil = col.gameObject.GetComponent<Projectile>();
        if (projetil != null) {
            projeteisEmArea.Add(projetil);
        }
    }

    void OnTriggerLeave(Collider col) {
        Projectile projetil = col.gameObject.GetComponent<Projectile>();
        if (projetil != null && projeteisEmArea.Contains(projetil)) {
            projeteisEmArea.Remove(projetil);
        }
    }
}
