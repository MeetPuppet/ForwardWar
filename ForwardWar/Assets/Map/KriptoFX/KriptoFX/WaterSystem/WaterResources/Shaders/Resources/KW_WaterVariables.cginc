struct ShorelineData
{
	float4 uv1;
	float4 uv2;
	float4 data1;
	float4 data2;
};

float _MyTest;

static const float PI = 3.14159265359;
static const float PI2 = 6.28318530718;

half4						KW_WaterColor;
half4						KW_TurbidityColor;
half4						_MainColor;
half4						_SeaColor;
half4						_BottomColor;
half4						_BottomColorDeep;
half4						_SSSColor;
half4						_DiffColor;
half4						_BubblesColor;
half4						_IndirectDiffColor;
half4						_IndirectSpecColor;
half						_Metalic;
half						_Roughness;

sampler2D KW_PointLightAttenuation;
sampler2D _BRDFTex;
sampler2D _MainTex;
sampler2D					_FoamTex;
sampler2D					_BubblesTex;
sampler2D					KW_FlowMapTex;
sampler2D					KW_DispTex;
sampler2D					KW_DispTex_LOD1;
sampler2D					KW_DispTex_LOD2;
sampler2D					KW_NormTex;
sampler2D					KW_NormTex_LOD1;
sampler2D					KW_NormTex_LOD2;
sampler2D					KW_NormTex_Detail;

sampler2D KW_InteractiveWavesTex;
float4 KW_InteractiveWavesTex_TexelSize;
half KW_InteractiveWavesAreaSize;

sampler2D					KW_RipplesTexture;
sampler2D					KW_RipplesTexturePrev;
sampler2D					KW_RipplesNormalTexture;
sampler2D					KW_RipplesNormalTexturePrev;
sampler2D					_ReflectionTex;
sampler2D					_CameraColorTexture;
sampler2D KW_WaterOpaqueTexture;
//sampler2D					_ShadowMapTexture;
sampler2D					KW_VolumetricLight;
sampler2D KW_WaterMaskDepth;

float4						KW_DispTex_TexelSize;
float4						KW_DispTexDetail_TexelSize;
float4 KW_NormTex_TexelSize;
float4 KW_NormTex_LOD1_TexelSize;
float4 KW_NormTex_LOD2_TexelSize;
float4 KW_NormTex_Detail_TexelSize;
float4 KW_DispTex_LOD1_TexelSize;
float4 KW_DispTex_LOD2_TexelSize;
float4						_CameraColorTexture_TexelSize;


float						_Distortion;
float _test;
float4 _test2;
float _test3;

half _Turbidity;
half _WaterTimeScale;
half						KW_FFTDomainSize;
half						KW_FFTDomainSize_LOD1;
half						KW_FFTDomainSize_LOD2;
half						KW_FFTDomainSize_Detail;

half						KW_NormalLod;
half						KW_NormalLod_LOD1;
half						KW_NormalLod_LOD2;

half						KW_NormMipCount;
half						KW_NormMipCount_LOD1;
half						KW_NormMipCount_LOD2;
half						KW_NormMipCount_Detail;

float2						KW_RipplesUVOffset;
half						KW_RipplesScale;
half						KW_FlowMapSize;
float2						KW_FlowMapOffset;
half KW_FlowMapSpeed;
half						KW_WindSpeed;
half						KW_WindSpeed_LOD1;
half						KW_WindSpeed_LOD2;
half						KW_DistortScale;
half						KW_Time;
half _TesselationFactor;
half KW_WaterFarDistance;
half KW_NormalScattering_Lod;

sampler2D KW_DitherTexture;
sampler2D KW_DistanceFieldDepthIntersection;
sampler2D	KW_DistanceField;
sampler2D KW_TimeRemap;
sampler2D KW_ShoreWaveTex;
sampler2D					_TestTex;
sampler2D _NoiseTex;
sampler2D KW_UpDepth;
float4 KW_DistanceFieldPos;
float4 KW_UpDepthPos;
float3				KW_WaterPosition;
sampler2D KW_BeachWavesTex;
float4 KW_BeachWavesPos;

uniform float4 _GAmplitude;
uniform float4 _GFrequency;
uniform float4 _GSteepness;
uniform float4 _GSpeed;
uniform float4 _GDirectionAB;
uniform float4 _GDirectionCD;

UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
sampler2D _CameraDepthTextureBeforeWaterZWrite;
sampler2D _CameraDepthTextureBeforeWaterZWrite_Blured;
float4 _CameraDepthTextureBeforeWaterZWrite_TexelSize;
float KW_Transparent;
float KW_Turbidity;
half _DistanceBetweenBeachWaves;
half _MinimalDepthForBeachWaves;

float3 KW_DirLightForward;
float3 KW_DirLightColor;

int KW_PointLightCount;
float4 KW_PointLightPositions[100];
float4 KW_PointLightColors[100];

float4x4 KW_ViewToWorld;
float4x4 KW_ProjToView;

float3 KW_InteractPos;
float3 KW_InteractCameraOffset_Last;
sampler2D KW_InteractiveWavesNormalTex;
sampler2D KW_InteractiveWavesNormalTexPrev;
sampler2D KW_ShorelineTex;
sampler2D KW_ShorelineNormalTex;
float4 KW_ShorelineTex_TexelSize;

sampler2D KW_ShorelineTexMap;
float KW_ShorelineSize;
float3 KW_ShorelineOffset;

float3 KW_DistanceFieldDepthPos;
float KW_DistanceFieldDepthArea;
float KW_DistanceFieldDepthFar;

sampler2D _TestTexture;
sampler2D _TestDispTexture;
sampler2D _TestNormalTexture;
float4 FoamAnimUV;


sampler2D KW_OrthoDepth;
float KW_DepthOrthoSize;
float3 KW_DepthNearFarDistance;
float3 KW_DepthPos;

sampler2D KW_BakedWaves1_UV_Angle_Alpha;
sampler2D KW_BakedWaves2_UV_Angle_Alpha;

sampler2D KW_BakedWaves1_TimeOffset_Scale;
sampler2D KW_BakedWaves2_TimeOffset_Scale;

samplerCUBE   KW_LightsCube;
UNITY_DECLARE_TEXCUBE (KW_ReflectionCube);
float KW_ReflectionCubeMip;
sampler2D KW_ReflectionTex;
sampler2D KW_WaterMaskScatterNormals;
float4 KW_WaterMaskScatterNormals_TexelSize;
sampler2D KW_WaterDepth;
sampler2D KW_WaterMaskScatterNormals_Blured;
sampler2D KW_ShorelineWaveNormal;
sampler2D KW_ShorelineWaveDisplacement;

float TEST;
float KW_GlobalTimeOffsetMultiplier;
float KW_GlobalTimeSpeedMultiplier;
float KW_DepthOrthographicSize;
float4 Test4;
float KW_FFT_Size_Normalized;
sampler2D KW_PlanarReflection;
sampler2D KW_PlanarReflectionDepth;
float KW_PlanarReflectionClipPlaneOffset;
float KW_SSR_ClipOffset;