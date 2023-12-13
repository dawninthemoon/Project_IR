
Shader "Custom/SpriteShadowScreenShader"
{
	Properties
	{
		_CharacterTexture("Character Texture", 2D) = "white" {}
		_MainTex("Background Texture", 2D) = "white" {}
		_ShadowMapTexture("ShadowMap Texture", 2D) = "white" {}

		//[HideInInspector]
		_InterfaceTexture("Interface Texture", 2D) = "white" {}
		//[HideInInspector]
		_PerspectiveTexture("Perspective Texture", 2D) = "white" {}
		_PerspectiveDepthTexture("Perspective Depth Texture", 2D) = "white" {}

		_SunAngle("Sun Angle Degree", Range(0.0, 360.0)) = 0.0
		_ShadowDistance("Shadow Distance", Range(0.1, 3.0)) = 0.1
		_ShadowDistanceRatio("Shadow Distance Ratio", Range(0.0, 10.0)) = 0.0

		_ScreenSize("Screen Size", Vector) = (1024, 1024, 0, 0)
		_ShadowDistanceOffset("Shadow Distance Offset", Range(0.0, 100.0)) = 0.0

		_ShadowColor("ShadowColor", Color) = (0,0,0,1)

		[Space][Space][Space]
		_Brightness("Brightness", Range(0.0, 5.0)) = 1.0
		_Saturation("Saturation", Range(0.0, 1.0)) = 1.0
		_ColorTint("Color Tint", Color) = (1,1,1,1)
		_BackgroundColorTint("BackgroundColor", Color) = (1,1,1,1)

		[Space][Space][Space]
		_ShadowBlurExponential("Shadow Blur Exp", Range(0.0, 1.0)) = 0.0
		_BlurSize("Blur Size", Range(0.0, 32.0)) = 0.0
		_MultiSampleDistance("Multi Sample Distance", Range(0.0, 5.0)) = 0.0
		_MultiSampleColorTintRight("Multi Sample Color Tint Right", Color) = (1,1,1,1)
		_MultiSampleColorTintLeft("Multi Sample Color Tint Left", Color) = (1,1,1,1)

		[Space][Space][Space]
		_FogRate("Fog Rate", Range(0.0, 1.0)) = 0.0
		_FogStrength("Fog Strength", Range(0.0, 1.0)) = 1.0
		_FogColor("Fog Color", Color) = (1,1,1,1)


		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;
				fixed4 _ShadowColor;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _CharacterTexture;
				sampler2D _MainTex;
				sampler2D _ShadowMapTexture;
				sampler2D _PerspectiveTexture;
				sampler2D _InterfaceTexture;
				sampler2D _AlphaTex;
				sampler2D _PerspectiveDepthTexture;


				float _SunAngle;
				float _ShadowDistance;
				float _ShadowDistanceRatio;
				float _AlphaSplitEnabled;
				float _ShadowDistanceOffset;
				float4 _ScreenSize;

				float _Brightness;
				float _Saturation;
				fixed4 _ColorTint;
				fixed4 _BackgroundColorTint;

				float _BlurSize;
				float _ShadowBlurExponential;
				float _MultiSampleDistance;
				fixed4 _MultiSampleColorTintRight;
				fixed4 _MultiSampleColorTintLeft;


				float _FogRate;
				float _FogStrength;
				fixed4 _FogColor;

				half4 _MainTex_TexelSize;

				static const float backgroundBrightnessFactor = 7.5f;

				fixed4 SampleSpriteTexture(sampler2D sampleTexture, float2 uv)
				{
					fixed4 color = tex2D(sampleTexture, uv);

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				fixed4 gatherShadows(float2 texcoord)
				{
					return SampleSpriteTexture(_ShadowMapTexture, texcoord);
				}

				static const float2 resolution = float2(1024, 1024);

				/*
				1. 그림자는 오프셋 조절이 있다.
				2. 어떤 맵의 샘플링 값에 따라 오프셋을 조정한다.
				3. 값이 없는 구간에는 그림자가 안나와야한다.
				*/
				fixed4 drawCharacter(float2 texcoord)
				{
					fixed4 characterSample = SampleSpriteTexture(_CharacterTexture, texcoord);

					return characterSample;
				}

				fixed4 drawPerspective(float2 texcoord)
				{
					fixed4 perspectiveSample = SampleSpriteTexture(_PerspectiveTexture, texcoord);

					return perspectiveSample;
				}

				fixed4 drawInterface(float2 texcoord)
				{
					fixed4 interfaceSample = SampleSpriteTexture(_InterfaceTexture, texcoord);

					return interfaceSample;
				}

				fixed4 sampleBackground(float2 texcoord)
				{
					fixed4 backgroundSample = SampleSpriteTexture(_MainTex, texcoord) * _BackgroundColorTint;

					return backgroundSample;
				}


				fixed4 drawCharacterShadow(float4 backgroundSample, float2 texcoord)
				{
					float shadowSample = SampleSpriteTexture(_PerspectiveDepthTexture, texcoord);

					float sunAngle = _SunAngle;

					float near = 0.3f;
					float far = 1.0f;

					float clipDistance = far - near;
					float shadowDistance = (shadowSample * clipDistance);
					float additionalShadowDistance = _ShadowDistance * ((clipDistance / shadowDistance) * _ShadowDistanceRatio);
					float2 toUV = (1.0 / _ScreenSize.xy);

					float2 shadowDirection = float2(cos(sunAngle), sin(sunAngle));

					float2 shadowSampleTarget = toUV * (shadowDirection * (_ShadowDistance + additionalShadowDistance * _ShadowDistance));
					float2 shadowOffset = toUV * shadowDirection * _ShadowDistanceOffset;

					fixed4 shadowReSample = SampleSpriteTexture(_CharacterTexture, texcoord + shadowOffset + shadowSampleTarget);
						
					return shadowReSample * _ShadowColor;
				}

				fixed4 bluredShadowSample(float2 texcoord)
				{
					float near = 0.3f;
					float far = 100.0f;
					float Pi = 6.28318530718; // Pi*2
					float shadowSample = SampleSpriteTexture(_PerspectiveDepthTexture, texcoord);
					float clipDistance = far - near;
					float shadowDistance = (shadowSample * clipDistance);
					// GAUSSIAN BLUR SETTINGS {{{
					float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
					float Quality = 16.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
					float Size = _ShadowBlurExponential * (shadowDistance - clipDistance); // BLUR SIZE (Radius)
					// GAUSSIAN BLUR SETTINGS }}}

					float2 Radius = Size / resolution;

					fixed4 shadow = drawCharacterShadow(sampleBackground(texcoord), texcoord);

					float4 Color = shadow;
					// Blur calculations
					for (float d = 0.0; d < Pi; d += Pi / Directions)
					{
						for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
						{
							Color += drawCharacterShadow(sampleBackground(texcoord + float2(cos(d), sin(d)) * Radius * i), texcoord + float2(cos(d), sin(d)) * Radius * i);
						}
					}

					Color /= (Quality * Directions);

					return Color;
				}


				fixed4 bluredBackgroundSample(float2 texcoord)
				{
					float Pi = 6.28318530718; // Pi*2

					// GAUSSIAN BLUR SETTINGS {{{
					float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
					float Quality = 4.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
					float Size = _BlurSize; // BLUR SIZE (Radius)
					// GAUSSIAN BLUR SETTINGS }}}

					float2 Radius = Size / resolution;
					float4 Color = sampleBackground(texcoord);
					// Blur calculations
					for (float d = 0.0; d < Pi; d += Pi / Directions)
					{
						for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
						{
							Color += sampleBackground(texcoord + float2(cos(d), sin(d)) * Radius * i);
						}
					}
					Color /= Quality * Directions - backgroundBrightnessFactor;
					return Color;
					}

					fixed4 allTogether(float2 texcoord)
					{
						fixed4 backgroundSample = bluredBackgroundSample(texcoord);
						fixed4 characterSample = drawCharacter(texcoord);
						fixed4 characterShadow = bluredShadowSample(texcoord);
						fixed4 perspectiveSample = drawPerspective(texcoord);
						fixed4 interfaceSample = drawInterface(texcoord);

						fixed4 shadowdedBackground = lerp(perspectiveSample, characterShadow, characterShadow.a);
						fixed4 mixed = lerp(backgroundSample, shadowdedBackground, perspectiveSample.a);
						fixed4 mixed2 = lerp(mixed, characterSample, characterSample.a);
						fixed4 mixed3 = lerp(mixed2, interfaceSample, interfaceSample.a);
						return mixed3;

						return backgroundSample;
					}

					fixed4 onlyShadow(v2f IN)
					{
						fixed4 characterColor = SampleSpriteTexture(_CharacterTexture, IN.texcoord);
						fixed4 backgroundSample = SampleSpriteTexture(_MainTex, IN.texcoord);
						fixed4 shadowMap = SampleSpriteTexture(_ShadowMapTexture, IN.texcoord);

						float sunAngle = _SunAngle * 0.0174532925 + 3.141592;
						float additionalShadowDistance = _ShadowDistance * ((1.0 - shadowMap.r) * _ShadowDistanceRatio);
						float2 toUV = (1.0 / _ScreenSize.xy);

						float2 shadowDirection = float2(cos(sunAngle),sin(sunAngle));

						float2 shadowSampleTarget = toUV * (shadowDirection * (_ShadowDistance + additionalShadowDistance));
						float2 shadowOffset = toUV * shadowDirection * _ShadowDistanceOffset;

						float characterShadowSample = SampleSpriteTexture(_CharacterTexture, IN.texcoord + shadowOffset + shadowSampleTarget).a;

						return _ShadowColor * characterShadowSample;
					}

					fixed4 frag(v2f IN) : SV_Target
					{
						fixed4 resultColor = allTogether(IN.texcoord);

					//brigtness
					resultColor.rgb *= _Brightness;

					//grayscale
					float gray = dot(resultColor.rgb, fixed3(0.21f, 0.72f, 0.07f));
					resultColor.rgb = lerp(resultColor.rgb, fixed3(gray,gray,gray), 1.0 - _Saturation);

					//color tint
					resultColor.rgb *= _ColorTint;

					//fog
					float fogRate = smoothstep(0.0, 1.0, (IN.texcoord.y - _FogStrength) * (1.0 / ((1.0 - _FogStrength) * _FogRate)));
					resultColor.rgb *= (fogRate + ((1.0 - fogRate) * (1.0 - _FogColor.a)));
					resultColor.rgb += _FogColor.rgb * (1.0 - fogRate) * _FogColor.a;

					return resultColor;
				}
			ENDCG
			}
		}
}