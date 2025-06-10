#if !defined (FLOW_INCLUDED)
#define FLOW_INCLUDED

float3 FlowUV (float2 uv, float2 flowVector, float2 jump, float tilling, float time, float offset){
    float3 weightedUV = 0;
    float progress = frac(time + offset);
    
    weightedUV.xy = uv - flowVector * progress;
    //weightedUV *= tilling;
    weightedUV += offset;
    weightedUV.xy += (time - progress) * jump;
    weightedUV.z = 1 - abs(1 - 2 * progress);  

    return weightedUV;
}

#endif