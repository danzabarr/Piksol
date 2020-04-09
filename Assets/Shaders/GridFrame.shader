Shader "Unlit/GridFrame"
{
	Properties
	{
		_Color("Fill", color) = (1, 1, 1, .5)
		_HighlightColor("Face Highlight", color) = (0, 1, 1, 1)

		[Space][Space][Space]
		_OutlineColor("Outline", color) = (1, 1, 1, 1)
		_OuterLine("Outline Weight", Range(0, 1)) = .1
		[Enum(UnityEngine.Rendering.CullMode)] _OutlineCull("Outline Cull", Int) = 2

		[Space][Space][Space]
		_LineColor("Grid", color) = (1, 1, 1, 1)
		_Thickness("Grid Weight", Range(0, 1)) = .1
		[Enum(UnityEngine.Rendering.CullMode)] _GridCull("Grid Cull", Int) = 2
		
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		LOD 100

		Pass
		{

			Blend SrcAlpha OneMinusSrcAlpha
			Cull[_GridCull]//Back
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 _Color;
			float4 _LineColor;
			float4 _HighlightColor;
			float4 _HighlightEUN;
			float4 _HighlightWDS;
			float4 _Subdivisions;
			float _Thickness;

			struct input
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 vertex : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			v2f vert(input v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//i.vertex -= _Subdivisions.w / 2 * ;

				float3 t = (1 - _Thickness) / _Subdivisions.xyz;

				float3 l = (i.vertex - _Thickness / 2 / _Subdivisions.xyz + 1 / _Subdivisions.xyz) % (1 / _Subdivisions.xyz);

				l = step(t, l);

				float4 col = _Color;
				float4 lineColor = _LineColor;

				if (i.normal.x > .9)
				{
					l.x = 0;
					col = lerp(col, _HighlightColor, abs(_SinTime.w / 3.14159) * _HighlightEUN.x);
				}
				if (i.normal.x < -.9)
				{
					l.x = 0;
					col = lerp(col, _HighlightColor, abs(_SinTime.w / 3.14159) * _HighlightWDS.x);
				}
				if (i.normal.y > .9)
				{
					l.y = 0;
					col = lerp(col, _HighlightColor, abs(_SinTime.w / 3.14159) * _HighlightEUN.y);
				}
				if (i.normal.y < -.9)
				{
					l.y = 0;
					col = lerp(col, _HighlightColor, abs(_SinTime.w / 3.14159) * _HighlightWDS.y);
				}
				if (i.normal.z > .9)
				{
					l.z = 0;
					col = lerp(col, _HighlightColor, abs(_SinTime.w / 3.14159) * _HighlightEUN.z);
				}
				if (i.normal.z < -.9)
				{
					l.z = 0;
					col = lerp(col, _HighlightColor, abs(_SinTime.w / 3.14159) * _HighlightWDS.z);
				}

				lineColor.a *= max(l.x, max(l.y, l.z));

				col.a = lineColor.a + col.a * (1 - lineColor.a);
				col.rgb = (lineColor.rgb * lineColor.a + col.rgb * col.a * (1 - lineColor.a)) / col.a;

				return col;
			}
			ENDCG
		}



		Pass
		{

			Blend SrcAlpha OneMinusSrcAlpha
			Cull [_OutlineCull]
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 _OutlineColor;
			float4 _Subdivisions;
			float _OuterLine;

			struct input
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 vertex : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			v2f vert(input v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 t = _OuterLine * 2 / _Subdivisions.xyz;
			
				float3 l = (i.vertex - t / 2 + 1) % 1;

				l = step(1 - t, l);

				float4 lineColor = _OutlineColor;

				if (i.normal.x > .9)
				{
					l.x = 0;
				}
				if (i.normal.x < -.9)
				{
					l.x = 0;
				}
				if (i.normal.y > .9)
				{
					l.y = 0;
				}
				if (i.normal.y < -.9)
				{
					l.y = 0;
				}
				if (i.normal.z > .9)
				{
					l.z = 0;
				}
				if (i.normal.z < -.9)
				{
					l.z = 0;
				}

				lineColor.a *= max(l.x, max(l.y, l.z));

				return lineColor;
			}
			ENDCG
		}
	}
}