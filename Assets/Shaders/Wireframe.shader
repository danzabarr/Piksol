Shader "Custom/Geometry/Wireframe"
{
	Properties
	{
		[PowerSlider(3.0)]
		_WireframeVal("Thickness", Range(0., 0.5)) = 0.05
		_FrontColor("Front", color) = (1, 1, 1, 1)
		_BackColor("Back", color) = (1, 1, 1, 1)
		_FillColor("Fill", Color) = (0, 0, 0, 0)
		[Toggle] _RemoveDiag("Remove Diagonals", Float) = 0
		_Alpha("Alpha", Range(0, 1)) = 1
	}
	SubShader
	{
			

	Pass
	{

		Tags { "Queue" = "Geometry" "RenderType" = "Transparent" }
		//Cull Front
			
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }


		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma geometry geom

		// Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
		#pragma shader_feature __ _REMOVEDIAG_ON

		#include "UnityCG.cginc"

		struct v2g {
			float4 worldPos : SV_POSITION;
		};

		struct g2f {
			float4 pos : SV_POSITION;
			float3 bary : TEXCOORD0;
		};

		v2g vert(appdata_base v) {
			v2g o;
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
			return o;
		}

		[maxvertexcount(3)]
		void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
			float3 param = float3(0., 0., 0.);

			#if _REMOVEDIAG_ON
			float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
			float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
			float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

			if (EdgeA > EdgeB && EdgeA > EdgeC)
				param.y = 1.;
			else if (EdgeB > EdgeC && EdgeB > EdgeA)
				param.x = 1.;
			else
				param.z = 1.;
			#endif

			g2f o;
			o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
			o.bary = float3(1., 0., 0.) + param;
			triStream.Append(o);
			o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
			o.bary = float3(0., 0., 1.) + param;
			triStream.Append(o);
			o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
			o.bary = float3(0., 1., 0.) + param;
			triStream.Append(o);
		}

		float _WireframeVal;
		fixed4 _BackColor;
		float _Alpha;

		fixed4 frag(g2f i) : SV_Target {
			if (!any(bool3(i.bary.x < _WireframeVal, i.bary.y < _WireframeVal, i.bary.z < _WireframeVal)))
				discard;

			fixed4 col = _BackColor;
			col.a *= _Alpha;
			return col;
		}

		ENDCG
	}

	Pass
	{
		Tags { "Queue" = "Geometry" "RenderType" = "Transparent" }

		//Cull Back

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma geometry geom

			// Change "shader_feature" with "pragma_compile" if you want set this keyword from c# code
			#pragma shader_feature __ _REMOVEDIAG_ON

			#include "UnityCG.cginc"

			struct v2g {
				float4 worldPos : SV_POSITION;
			};

			struct g2f {
				float4 pos : SV_POSITION;
				float3 bary : TEXCOORD0;
			};

			v2g vert(appdata_base v) {
				v2g o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream) {
				float3 param = float3(0., 0., 0.);

				#if _REMOVEDIAG_ON
				float EdgeA = length(IN[0].worldPos - IN[1].worldPos);
				float EdgeB = length(IN[1].worldPos - IN[2].worldPos);
				float EdgeC = length(IN[2].worldPos - IN[0].worldPos);

				if (EdgeA > EdgeB && EdgeA > EdgeC)
					param.y = 1.;
				else if (EdgeB > EdgeC && EdgeB > EdgeA)
					param.x = 1.;
				else
					param.z = 1.;
				#endif

				g2f o;
				o.pos = mul(UNITY_MATRIX_VP, IN[0].worldPos);
				o.bary = float3(1., 0., 0.) + param;
				triStream.Append(o);
				o.pos = mul(UNITY_MATRIX_VP, IN[1].worldPos);
				o.bary = float3(0., 0., 1.) + param;
				triStream.Append(o);
				o.pos = mul(UNITY_MATRIX_VP, IN[2].worldPos);
				o.bary = float3(0., 1., 0.) + param;
				triStream.Append(o);
			}

			float _WireframeVal;
			fixed4 _FrontColor;
			fixed4 _FillColor;
			float _Alpha;

			fixed4 frag(g2f i) : SV_Target{

				fixed4 col = _FrontColor;
				if (!any(bool3(i.bary.x <= _WireframeVal, i.bary.y <= _WireframeVal, i.bary.z <= _WireframeVal)))
					col = _FillColor;

				col.a *= _Alpha;
				return col;
			}

			ENDCG
		}
	}
}