Shader "Custom/SimpleSpriteBlur"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _BlurRadiusPx ("Blur Radius (pixels)", Range(0, 50)) = 4
    }

    SubShader
    {
        Tags{ "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalRenderPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 2.0
            #pragma multi_compile_fog
            #pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // SpriteRenderer-friendly declarations
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            float _BlurRadiusPx;
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;      // SpriteRenderer tint
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos   = TransformObjectToHClip(v.vertex.xyz);
                o.uv    = v.uv;
                o.color = v.color;
                return o;
            }

            half4 Sample(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            }

            half4 frag (v2f i) : SV_Target
            {
                // Convert pixel radius to UV offset
                float2 o = _MainTex_TexelSize.xy * _BlurRadiusPx * 2.0;


                // 9-tap blur: center + 4 cardinal + 4 diagonals (normalized weights)
                half4 c  = Sample(i.uv) * 0.20;
                c += Sample(i.uv + float2( o.x, 0)) * 0.15;
                c += Sample(i.uv + float2(-o.x, 0)) * 0.15;
                c += Sample(i.uv + float2(0,  o.y)) * 0.15;
                c += Sample(i.uv + float2(0, -o.y)) * 0.15;
                c += Sample(i.uv + float2( o.x,  o.y)) * 0.05;
                c += Sample(i.uv + float2( o.x, -o.y)) * 0.05;
                c += Sample(i.uv + float2(-o.x,  o.y)) * 0.05;
                c += Sample(i.uv + float2(-o.x, -o.y)) * 0.05;

                // Preserve SpriteRenderer tint & alpha
                c.rgb *= i.color.rgb;
                c.a   *= i.color.a;
                return c;
            }
            ENDHLSL

            

        }


    }
}
