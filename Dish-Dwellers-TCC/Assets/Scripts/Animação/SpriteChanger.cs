using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;
public class SpriteChanger : MonoBehaviour{

    // Info :
    [SerializeField][Range(0, 3)]private int sprite = 0;

    // Material :
    private MaterialPropertyBlock mpb;
    private LocalKeyword[] keywords;
    const string REFERENCE = "_SPRITE";
    readonly string[] SULFIX = {
        "_ONE",
        "_TWO",
        "_THREE",
        "_FOUR"
    };
    

    // Componentes :
    private Renderer render;


    private void Awake(){
        render = GetComponentInChildren<Renderer>();
        keywords = render.material.enabledKeywords;
    }

    private void FixedUpdate(){
        ChangeSprite(sprite);
    }

    /// <summary>
    /// Muda o sprite utilizado ao alterar o canal de UV sendo lido pelo material. Somente valores de 0 a 3 são aceitos.
    /// </summary>
    /// <param name="sprite"></param>
    public void ChangeSprite(int sprite) {
        Math.Clamp(sprite, 0, 3);

        SelectKeyword(sprite);

        this.sprite = sprite;
    }

    private void SelectKeyword(int index){
        foreach(var keyword in SULFIX){
            render.material.DisableKeyword(REFERENCE + keyword);
        }
        render.material.EnableKeyword(REFERENCE + SULFIX[index]);
    }

    
}
