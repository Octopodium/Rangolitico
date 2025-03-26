using System;
using UnityEngine;
public class SpriteChanger : MonoBehaviour{

    // Info :
    [SerializeField][Range(0, 3)]private int sprite;

    // Material :
    private MaterialPropertyBlock mpb;
    private int SpriteId = Shader.PropertyToID("_Sprite");

    // Componentes :
    private Renderer renderer;


    private void Awake(){
        renderer = GetComponentInChildren<Renderer>();
    }

    private void OnValidate(){
        if(!renderer) renderer = GetComponentInChildren<Renderer>();
        ChangeSprite(sprite);
    }

    /// <summary>
    /// Muda o sprite utilizado ao alterar o canal de UV sendo lido pelo material. Somente valores de 0 a 3 são aceitos.
    /// </summary>
    /// <param name="sprite"></param>
    public void ChangeSprite(int sprite){
        mpb = new MaterialPropertyBlock();

        Math.Clamp(sprite, 0, 3);
        this.sprite = sprite;

        mpb.SetInt(SpriteId, sprite);
        renderer.SetPropertyBlock(mpb);
    }
    
}
