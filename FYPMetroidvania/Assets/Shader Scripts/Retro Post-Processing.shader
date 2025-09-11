Shader "Custom Post-Processing/Retro"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _pixelSize("Pixel Size", Range(0,20)) = 0
        _redCount("Red Colours", int) = 25
        _greenCount("Green Colours", int) = 25
        _blueCount("Blue Colours", int) = 25
        _bayerLevel("Dithering", Range(-1,2)) = -1 
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Uniforms
            uniform sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            uniform float _pixelSize;
            uniform int _redCount, _greenCount, _blueCount, _bayerLevel; 

            // Functions for RGB/OKLAB color convertion
            float3 rgb2xyz( float3 c ) {
    float3 tmp;
    tmp.x = ( c.r > 0.04045 ) ? pow( ( c.r + 0.055 ) / 1.055, 2.4 ) : c.r / 12.92;
    tmp.y = ( c.g > 0.04045 ) ? pow( ( c.g + 0.055 ) / 1.055, 2.4 ) : c.g / 12.92,
    tmp.z = ( c.b > 0.04045 ) ? pow( ( c.b + 0.055 ) / 1.055, 2.4 ) : c.b / 12.92;
    const float3x3 mat = float3x3(
		0.4124, 0.3576, 0.1805,
        0.2126, 0.7152, 0.0722,
        0.0193, 0.1192, 0.9505 
	);
    return 100.0 * mul(tmp, mat);
}
            float3 xyz2lab( float3 c ) {
    float3 n = c / float3(95.047, 100, 108.883);
    float3 v;
    v.x = ( n.x > 0.008856 ) ? pow( n.x, 1.0 / 3.0 ) : ( 7.787 * n.x ) + ( 16.0 / 116.0 );
    v.y = ( n.y > 0.008856 ) ? pow( n.y, 1.0 / 3.0 ) : ( 7.787 * n.y ) + ( 16.0 / 116.0 );
    v.z = ( n.z > 0.008856 ) ? pow( n.z, 1.0 / 3.0 ) : ( 7.787 * n.z ) + ( 16.0 / 116.0 );
    return float3(( 116.0 * v.y ) - 16.0, 500.0 * ( v.x - v.y ), 200.0 * ( v.y - v.z ));
}
            float3 rgb2lab( float3 c ) {
    float3 lab = xyz2lab( rgb2xyz( c ) );
    return float3( lab.x / 100.0, 0.5 + 0.5 * ( lab.y / 127.0 ), 0.5 + 0.5 * ( lab.z / 127.0 ));
}
            float3 lab2xyz( float3 c ) {
    float fy = ( c.x + 16.0 ) / 116.0;
    float fx = c.y / 500.0 + fy;
    float fz = fy - c.z / 200.0;
    return float3(
         95.047 * (( fx > 0.206897 ) ? fx * fx * fx : ( fx - 16.0 / 116.0 ) / 7.787),
        100.000 * (( fy > 0.206897 ) ? fy * fy * fy : ( fy - 16.0 / 116.0 ) / 7.787),
        108.883 * (( fz > 0.206897 ) ? fz * fz * fz : ( fz - 16.0 / 116.0 ) / 7.787)
    );
}
            float3 xyz2rgb( float3 c ) {
	const float3x3 mat = float3x3(
        3.2406, -1.5372, -0.4986,
        -0.9689, 1.8758, 0.0415,
        0.0557, -0.2040, 1.0570
	);
    float3 v = mul(c / 100.0, mat);
    float3 r;
    r.x = ( v.r > 0.0031308 ) ? (( 1.055 * pow( v.r, ( 1.0 / 2.4 ))) - 0.055 ) : 12.92 * v.r;
    r.y = ( v.g > 0.0031308 ) ? (( 1.055 * pow( v.g, ( 1.0 / 2.4 ))) - 0.055 ) : 12.92 * v.g;
    r.z = ( v.b > 0.0031308 ) ? (( 1.055 * pow( v.b, ( 1.0 / 2.4 ))) - 0.055 ) : 12.92 * v.b;
    return r;
}
            float3 lab2rgb( float3 c ) {
    return xyz2rgb( lab2xyz( float3(100.0 * c.x, 2.0 * 127.0 * (c.y - 0.5), 2.0 * 127.0 * (c.z - 0.5)) ) );
}

            // Dithering
            static const int bayer2[2 * 2] = {
                0, 2,
                3, 1
            };
            static const int bayer4[4 * 4] = {
                0, 8, 2, 10,
                12, 4, 14, 6,
                3, 11, 1, 9,
                15, 7, 13, 5
            };
            static const int bayer8[8 * 8] = {
                0, 32, 8, 40, 2, 34, 10, 42,
                48, 16, 56, 24, 50, 18, 58, 26,  
                12, 44,  4, 36, 14, 46,  6, 38, 
                60, 28, 52, 20, 62, 30, 54, 22,  
                3, 35, 11, 43,  1, 33,  9, 41,  
                51, 19, 59, 27, 49, 17, 57, 25, 
                15, 47,  7, 39, 13, 45,  5, 37, 
                63, 31, 55, 23, 61, 29, 53, 21
            };
            float GetBayer2(int x, int y) {
                return float(bayer2[(x % 2) + (y % 2) * 2]) * (1.0f / 4.0f) - 0.5f;
            }
            float GetBayer4(int x, int y) {
                return float(bayer4[(x % 4) + (y % 4) * 4]) * (1.0f / 16.0f) - 0.5f;
            }
            float GetBayer8(int x, int y) {
                return float(bayer8[(x % 8) + (y % 8) * 8]) * (1.0f / 64.0f) - 0.5f;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Pixelisation
                float2 UV = i.uv;
                UV.x *= _ScreenParams.x / _pixelSize;
                UV.y *= _ScreenParams.y / _pixelSize;
                UV.x = int(UV.x);
                UV.y = int(UV.y);
                UV.x /= _ScreenParams.x / _pixelSize;
                UV.y /= _ScreenParams.y / _pixelSize;

                float4 output = tex2D(_MainTex, UV); // Color sampler

                // Dithering
                if(_bayerLevel >= 0){
                    int x = i.uv.x * _MainTex_TexelSize.z;
                    int y = i.uv.y * _MainTex_TexelSize.w;
                    float bayerValues[3] = { 0, 0, 0 };
                    bayerValues[0] = GetBayer2(x, y);
                    bayerValues[1] = GetBayer4(x, y);
                    bayerValues[2] = GetBayer8(x, y);

                    output = output + 0.5f * bayerValues[_bayerLevel];
                }

                // Posterization
                output.r = floor((_redCount - 1.0) * output.r + 0.5) / (_redCount - 1.0f);
                output.g = floor((_greenCount - 1.0) * output.g + 0.5) / (_greenCount - 1.0f);
                output.b = floor((_blueCount - 1.0) * output.b + 0.5) / (_blueCount - 1.0f);

                // Convert RGB to OKLAB color space
                float3 rgb = float3(output.r,output.g,output.b);
                float3 lab = rgb2lab(rgb);
                //output = float4(lab.x,lab.y,lab.z,output.w);

                return output;
            }
            ENDHLSL
        }
    }
}
