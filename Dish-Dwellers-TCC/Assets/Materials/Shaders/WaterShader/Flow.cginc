#if !defined (FLOW_INCLUDED)
#define FLOW_INCLUDED

float3 FlowUV (float2 uv, float2 flowVector, float time, float offset){
    float3 weightedUV = 0;
    float progress = frac(time + offset);
    
    weightedUV.xy = uv - flowVector * progress;
    weightedUV.z = 1 - abs(1 - 2 * progress);  

    return weightedUV;
}

#endif