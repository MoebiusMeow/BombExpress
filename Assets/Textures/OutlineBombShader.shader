Shader "Hidden/OutlineEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _outlineColor ("OutlineColor", Color) = (1, 1, 1, 1)
        _innerColor ("InnerColor", Color) = (1, 0, 0, 1)
        _width ("Width", range(0, 100)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _outlineColor;
            float4 _innerColor;
            float _width;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 colL = tex2D(_MainTex, i.uv + float2(-1, 0) * _MainTex_TexelSize.xy * _width);
                fixed4 colR = tex2D(_MainTex, i.uv + float2(+1, 0) * _MainTex_TexelSize.xy * _width);
                fixed4 colT = tex2D(_MainTex, i.uv + float2(0, -1) * _MainTex_TexelSize.xy * _width);
                fixed4 colB = tex2D(_MainTex, i.uv + float2(0, +1) * _MainTex_TexelSize.xy * _width);
                if (col.a < 1)
                {
					if (colL.a >= 1 || colR.a >= 1 || colT.a >= 1 || colB.a >= 1) return _outlineColor;
                    else discard;
				}
                if (col.r <= 0.5 && (colL.a < 1 || colR.a < 1 || colT.a < 1 || colB.a < 1)) return _innerColor;
                return col;
            }
            ENDCG
        }
    }
}
