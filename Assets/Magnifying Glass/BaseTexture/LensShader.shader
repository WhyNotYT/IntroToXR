Shader "Custom/RealisticLens"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Distortion ("Distortion", Range(-1, 1)) = 0.5
        _Scale ("Scale", Range(0, 2)) = 1.0
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.1)) = 0.02
        _Rotation ("Rotation", Range(-3.14, 3.14)) = 0.0
        _Opacity ("Opacity", Range(0, 1)) = 0.5
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 2.0
        _Refraction ("Refraction Strength", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Distortion;
            float _Scale;
            float _ChromaticAberration;
            float _Rotation;
            float _Opacity;
            float _FresnelPower;
            float _Refraction;

            // Rotate UV coordinates by a given angle (in radians)
            float2 RotateUV(float2 uv, float angle)
            {
                float2 center = float2(0.5, 0.5);
                uv -= center;
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2 rotatedUV = float2(
                    uv.x * cosA - uv.y * sinA,
                    uv.x * sinA + uv.y * cosA
                );
                return rotatedUV + center;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }
            
            float2 distort(float2 uv, float strength)
            {
                float2 center = uv - 0.5;
                float r2 = dot(center, center);
                float f = 1 + r2 * strength;
                return center * f + 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                // Apply rotation to UVs
                float2 uv = RotateUV((i.uv - 0.5) / _Scale + 0.5, _Rotation);

                // Apply refraction based on normal direction
                float2 refractionOffset = (i.worldNormal.xy * _Refraction);
                uv += refractionOffset;

                // Sample each color channel with different distortion strengths
                float2 redUV = distort(uv, _Distortion - _ChromaticAberration);
                float2 greenUV = distort(uv, _Distortion);
                float2 blueUV = distort(uv, _Distortion + _ChromaticAberration);

                float red = tex2D(_MainTex, redUV).r;
                float green = tex2D(_MainTex, greenUV).g;
                float blue = tex2D(_MainTex, blueUV).b;
                fixed4 color = fixed4(red, green, blue, 1.0);

                // Fresnel Effect for Glass Transparency
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, i.worldNormal)), _FresnelPower);
                float alpha = saturate(_Opacity + fresnel);

                return fixed4(color.rgb, alpha);
            }
            ENDCG
        }
    }
}
