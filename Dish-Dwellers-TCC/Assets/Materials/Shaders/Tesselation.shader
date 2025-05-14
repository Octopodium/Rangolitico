Shader "Unlit/Tesselation"
// This shader fills the mesh shape with a color predefined in the code.
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        Tags 
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma target 5.0

            #pragma vertex vert
            #pragma hull Hull
            #pragma domain Domain
            #pragma fragment frag


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            


            #define BARYCENTRIC_INTERPOLATE(fieldName) \
            patch[0].fieldName * barycentricCoordinates.x + \
            patch[1].fieldName * barycentricCoordinates.y + \
            patch[2].fieldName * barycentricCoordinates.z
            

            struct Attributes
            {
                float3 positionOS   : POSITION;                 
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            // Used to pass information from the vertex shader to the tesselation stage.
            struct TessellationControlPoint
            {
                float3 positionWS : INTERNALTESSPOS;
                float3 normalWS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Used to control edge and center subdivision in the PatchConstantFunction.
            struct TessellationFactors{
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;

                float3 bezierPoints[NUM_BEZIER_CONTROL_POINTS] : BEZIERPOS;
            };

            struct Interpolator{
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 positionCS : SV_Position;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
            };            

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            TessellationControlPoint vert(Attributes input)
            {
                TessellationControlPoint output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionWS = posInputs.positionWS;
                output.normalWS = normalInputs.normalWS;

                return output;
            }

            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("PatchConstantFunction")]
            [partitioning("interger")]
            TessellationControlPoint Hull(InputPatch<TessellationControlPoint, 3> patch, uint id: SV_OutputControlPointID)
            {
                return patch[id];
            }

            TessellationFactors PatchConstantFunction(InputPatch<TessellationControlPoint, 3> patch){
                UNITY_SETUP_INSTANCE_ID(patch[0]);

                TessellationFactors factors;
                factors.edge[0] = 1;
                factors.edge[1] = 1;
                factors.edge[2] = 1;
                factors.inside = 1;
                return factors;
            }

            [domain("tri")]
            Interpolator Domain(TessellationFactors factors, OutputPatch<TessellationControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation){
                Interpolator output;

                UNITY_SETUP_INSTANCE_ID(patch[0]);
                UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 positionWS = BARYCENTRIC_INTERPOLATE(positionWS);
                float3 normalWS = BARYCENTRIC_INTERPOLATE(normalWS);

                output.positionWS = positionWS;
                output.normalWS = normalWS;
                output.positionCS = TransformWorldToHClip(positionWS);

                return output;
            }

            // The fragment shader definition.            
            half4 frag() : SV_Target
            {
                // Defining the color variable and returning it.
                half4 customColor;
                customColor = half4(0.5, 0, 0, 1);
                return customColor;
            }
            ENDHLSL
        }
    }
}