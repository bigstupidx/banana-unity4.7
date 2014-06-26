Shader "HIDDEN/Unlit/Text 1" 
{
	Properties
	{
		_MainTex ("Alpha (A)", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Offset -1, -1
			Fog { Mode Off }
			//ColorMask RGB
			AlphaTest Greater .01
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _ClipRange0 = float4(0.0, 0.0, 1.0, 1.0);
			float2 _ClipArgs0 = float2(1000.0, 1000.0);

			struct appdata_t
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 worldPos : TEXCOORD1;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				
				float2 temp = v.vertex.xy * _ClipRange0.zw;
				o.worldPos = temp + _ClipRange0.xy;
				return o;
			}

			half4 frag (v2f IN) : COLOR
			{
				// Softness factor
				float worldPosY = abs(IN.worldPos.y);
				float worldPosX = abs(IN.worldPos.x);
				float2 factorX = (1.0 - worldPosX) * _ClipArgs0.x;
				float2 factorY = (1.0 - worldPosY) * _ClipArgs0.y;
				float factor = min(factorX, factorY);
			
				// Sample the texture
				half4 col = IN.color;
				col.a *= tex2D(_MainTex, IN.texcoord).a;
				col.a *= clamp( factor, 0.0, 1.0);

				return col;
			}
			ENDCG
		}
	}
}
