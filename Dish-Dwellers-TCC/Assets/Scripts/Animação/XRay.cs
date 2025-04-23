using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class XRay : MonoBehaviour
{
    // Componentes:
    [HideInInspector] public Renderer render;
    private MaterialPropertyBlock mpb;

    private void Start(){

        if(render == null){
            render = GetComponentInChildren<Renderer>();
        }

        List<Material> materials = render.materials.ToList();

        mpb = new MaterialPropertyBlock();
        mpb.SetTexture("_MainTex", materials[0].mainTexture);
        render.SetPropertyBlock(mpb);
    }
}
