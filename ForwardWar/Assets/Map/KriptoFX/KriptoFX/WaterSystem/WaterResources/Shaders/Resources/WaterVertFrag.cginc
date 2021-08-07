struct v2fDepth {
	float4  pos  : SV_POSITION;
	float3 worldPos : TEXCOORD1;
};



struct v2f {
	float4  pos  : SV_POSITION;

	float3 worldPos : TEXCOORD1;
	float3 viewDir : TEXCOORD2;

	float4 screenPos : TEXCOORD3;
	float4 grabPos : TEXCOORD4;

	half heightScattering : TEXCOORD5;
	float3 derivativePos : TEXCOORD6;

#if KW_INTERACTIVE_WAVES
	float2 ripplesUV : TEXCOORD7;
#endif
	float3 worldPosRefracted : TEXCOORD8;

	float4 shorelineUVAnim1 : TEXCOORD9;
	float4 shorelineUVAnim2 : TEXCOORD10;
	float4 shorelineWaveData1 : TEXCOORD11;
	float4 shorelineWaveData2 : TEXCOORD12;

};

float2 GetAnimatedUV(float2 uv, int _ColumnsX, int _RowsY, float FPS, float time)
{
	float2 size = float2(1.0f / _ColumnsX, 1.0f / _RowsY);
	uint totalFrames = _ColumnsX * _RowsY;
	uint index = time * 1.0f * FPS;
	uint indexX = index % _ColumnsX;
	uint indexY = floor((index % totalFrames) / _ColumnsX);

	float2 offset = float2(size.x * indexX, -size.y * indexY);
	float2 newUV = uv * size;
	newUV.y = newUV.y + size.y * (_RowsY - 1);

	return newUV + offset;
}

sampler2D KW_WavesBakedSim;
sampler2D KW_WavesBakedSimData;
sampler2D KW_WavesBakedSimNorm;


float OverridedTime;
float KW_WavesMapSize;

sampler2D KW_ScreenSpaceReflectionTex;
float4 KW_ScreenSpaceReflectionTex_TexelSize;
sampler2D screenCopyID2;
sampler2D KW_ScreenSpaceReflectionDist;
sampler2D KW_ScreenSpaceReflectionTex_Blured;
float3 KW_CameraForwardDir;
sampler2D KW_LastNormTex;
sampler2D KW_MipTexture;
float4 KW_MipTexture_TexelSize;
sampler2D _FluidsTex;
float4 _FluidsTex_TexelSize;

float4 Test;

float3 ComputeWaterOffset(float3 worldPos)
{
	//worldPos.xyz = mul(unity_ObjectToWorld, vertex);
	float2 uv = worldPos.xz / KW_FFTDomainSize;
	float3 offset = 0;
#if USE_FILTERING
	float3 disp = SampleBicubic(KW_DispTex, uv, KW_DispTex_TexelSize);
#else
	float3 disp = tex2Dlod(KW_DispTex, float4(uv, 0, 0)).xyz;
#endif

#ifdef KW_FLOW_MAP
	float2 flowMapUV = (worldPos.xz - KW_FlowMapOffset - KW_WaterPosition.xz) / KW_FlowMapSize + 0.5;
	disp = ComputeDisplaceUsingFlowMap(KW_DispTex, KW_FlowMapTex, disp, worldPos.xyz, uv, flowMapUV, KW_Time * KW_FlowMapSpeed);
#endif

#ifdef USE_MULTIPLE_SIMULATIONS
	disp += tex2Dlod(KW_DispTex_LOD1, float4(worldPos.xz / KW_FFTDomainSize_LOD1, 0, 0)).xyz;
	disp += tex2Dlod(KW_DispTex_LOD2, float4(worldPos.xz / KW_FFTDomainSize_LOD2, 0, 0)).xyz;
#endif
	offset += disp * float3(1.25, 1, 1.25);
	//offset.y += disp.y;
	//offset.xz += disp.xz * 1.25;
	return offset;
}

float3 ComputeBeachWaveOffsetForOneLine(float2 wavesMapUV, float terrainDepth, float time, sampler2D tex_UV_angle_alpha, sampler2D tex_timeOffset_scale,
	inout float4 shorelineUVAnim, inout float4 shorelineWaveData)
{
	
	float fps = 20;
	float4 uv_angle_alpha = tex2Dlod(tex_UV_angle_alpha, float4(wavesMapUV, 0, 0));
	float2 timeOffset_Scale = tex2Dlod(tex_timeOffset_scale, float4(wavesMapUV, 0, 0));

	shorelineUVAnim.xy = uv_angle_alpha;

	float2 waveUV = uv_angle_alpha.xy; 
	float waveAngle = uv_angle_alpha.z;
	float waveAlpha = uv_angle_alpha.w;
	float timeOffset = timeOffset_Scale.x;
	
	float waveScale = timeOffset_Scale.y;

	if (waveAlpha.x > 0.1) 
	{
		
		time += timeOffset * KW_GlobalTimeOffsetMultiplier;
	
		float2 uv = GetAnimatedUV(waveUV, 14, 15, fps, time);
		float2 prevUV = GetAnimatedUV(waveUV, 14, 15, fps, time + 1.0 / fps);

		float3 pos = tex2Dlod(KW_ShorelineWaveDisplacement, float4(uv, 0, 0));
		float3 pos2 = tex2Dlod(KW_ShorelineWaveDisplacement, float4(prevUV, 0, 0));
		pos = lerp(pos, pos2, frac(time * fps));
		pos.y -= 2;
		pos.xy *= pos.z;

		float angle = (360 * waveAngle) * UNITY_PI / 180.0;
		float sina, cosa;
		sincos(angle, sina, cosa);
		float3 offsetWave = float3(cosa * pos.x, pos.y, -sina * pos.x);
		
		offsetWave = offsetWave * waveAlpha * waveScale * 0.3;
		offsetWave.y = max(offsetWave.y, pos.z * (terrainDepth - KW_WaterPosition.y + 0.125));


		shorelineUVAnim.xy = uv;
		shorelineUVAnim.zw = prevUV;

		shorelineWaveData.xy = float2(-sina, cosa);
		shorelineWaveData.z = frac(time * fps);
		shorelineWaveData.w = waveAlpha;

		return offsetWave;
	}
	return 0;
}

float3 ComputeBeachWaveNormal(float4 shorelineUVAnim, float4 shorelineWaveData)
{
	float4 waveNorm = tex2D(KW_ShorelineWaveNormal, shorelineUVAnim.xy).xyzw;
	float4 waveNorm2 = tex2D(KW_ShorelineWaveNormal, shorelineUVAnim.zw).xyzw;
	waveNorm = lerp(waveNorm, waveNorm2, shorelineWaveData.z);
	waveNorm.xyz = waveNorm.xyz * 2 - 1;
	waveNorm.xz *= -1;
	
	float2x2 m = float2x2(shorelineWaveData.y, -shorelineWaveData.x, shorelineWaveData.x, shorelineWaveData.y);
	waveNorm.xz = mul(m, waveNorm.xz);
	float wavesAlpha = shorelineWaveData.w > 0.999 ? 1 : 0;
	waveNorm.a *= wavesAlpha;
	return lerp(float3(0, 1, 0), waveNorm.xyz, waveNorm.a);
}

float ComputeWaterOrthoDepth(float3 worldPos)
{
	float2 depthUV = (worldPos.xz - KW_DepthPos.xz - KW_WaterPosition.xz*0) / KW_DepthOrthographicSize + 0.5;
	float terrainDepth = tex2Dlod(KW_OrthoDepth, float4(depthUV, 0, 0)).r * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y + KW_DepthPos.y;
	return terrainDepth;
}

float3 ComputeBeachWaveOffset(float3 worldPos, inout ShorelineData shorelineData, float timeOffset = 0)
{
	shorelineData.uv1 = 0;
	shorelineData.uv2 = 0;
	shorelineData.data1 = 0;
	shorelineData.data2 = 0;
	float3 offset = 0;

	float2 wavesMapUV = (worldPos.xz - KW_WaterPosition.xz) / KW_WavesMapSize + 0.5;

	//float2 depthUV = (worldPos.xz - KW_DepthPos.xz) / KW_DepthOrthographicSize + 0.5;
	//float terrainDepth = tex2Dlod(KW_OrthoDepth, float4(depthUV, 0, 0)).r * KW_DepthNearFarDistance.z - KW_DepthNearFarDistance.y + KW_DepthPos.y;
	float terrainDepth = ComputeWaterOrthoDepth(worldPos);
	//_Time.y = TEST;
	
	float timeLimit = (14.0 * 15.0) / 20.0; //(frameX * frameY) / fps
	//KW_Time = Test4.x;
	float time = frac((KW_GlobalTimeSpeedMultiplier * KW_Time) / timeLimit) * timeLimit;
	time += timeOffset;

	float3 offsetWave1 = ComputeBeachWaveOffsetForOneLine(wavesMapUV, terrainDepth, time, KW_BakedWaves1_UV_Angle_Alpha, KW_BakedWaves1_TimeOffset_Scale, shorelineData.uv1, shorelineData.data1);
	float3 offsetWave2 = ComputeBeachWaveOffsetForOneLine(wavesMapUV, terrainDepth, time, KW_BakedWaves2_UV_Angle_Alpha, KW_BakedWaves2_TimeOffset_Scale, shorelineData.uv2, shorelineData.data2);
	offset.xyz += offsetWave1;
	offset.xyz += offsetWave2;
	return offset;
}


float4 KW_PlanarReflection_TexelSize;


half3 SampleReflectionProbe(UNITY_ARGS_TEXCUBE(tex), half4 hdr, float3 reflDir, half mip)
{
	half4 rgbm = UNITY_SAMPLE_TEXCUBE_LOD(tex, reflDir, mip);
	return DecodeHDR(rgbm, hdr);
}

half4 ComputeWaterColor(v2f i, float facing)
{
	i.viewDir = normalize(i.viewDir);

	float2 uv = i.worldPos.xz / KW_FFTDomainSize;

	half3 normal = tex2D(KW_NormTex, uv);
	half3 normBicubicFiltered = SampleBicubic(KW_NormTex, uv, KW_NormTex_TexelSize);

	normal = Tex2D_AA_LQ(KW_NormTex, uv);
	
	float viewDist = length(i.worldPos.xyz - _WorldSpaceCameraPos);
	half bicubicLodDist = 10 + (1 - KW_FFT_Size_Normalized) * 40;
	normal = lerp(normBicubicFiltered, normal, saturate(viewDist / bicubicLodDist));
	
	//return float4(norm.xz*10, 0, 1);
	
#ifdef USE_MULTIPLE_SIMULATIONS
	half3 norm_lod1 = tex2D(KW_NormTex_LOD1, i.worldPos.xz / KW_FFTDomainSize_LOD1);
	norm_lod1 = Tex2D_AA_LQ(KW_NormTex_LOD1, i.worldPos.xz / KW_FFTDomainSize_LOD1);

	half3 norm_lod2 = tex2D(KW_NormTex_LOD2, i.worldPos.xz / KW_FFTDomainSize_LOD2);
	norm_lod2 = Tex2D_AA_LQ(KW_NormTex_LOD2, i.worldPos.xz / KW_FFTDomainSize_LOD2);

	normal = NormalsCombine(normal, norm_lod1, norm_lod2);
#endif
	
#if defined(KW_FLOW_MAP) || defined(KW_FLOW_MAP_EDIT_MODE) 
	float2 flowMapUV = (i.worldPos.xz - KW_FlowMapOffset - KW_WaterPosition.xz) / KW_FlowMapSize + 0.5;
	float2 flowmap = tex2D(KW_FlowMapTex, flowMapUV);
	
	normal = ComputeNormalUsingFlowMap(KW_NormTex, KW_FlowMapTex, normal, i.worldPos, uv, flowMapUV, KW_Time * KW_FlowMapSpeed);
#endif

//
//#ifdef KW_FLOW_MAP_EDIT_MODE
//	if (flowMapUV.x < 0 || flowMapUV.x > 1 || flowMapUV.y < 0 || flowMapUV.y > 1) return 0;
//	return half4(normal.xz + 0.5, 1, 1);
//#endif
	
#if USE_SHORELINE
	float3 shorelineWave1 = ComputeBeachWaveNormal(i.shorelineUVAnim1, i.shorelineWaveData1);
	float3 shorelineWave2 = ComputeBeachWaveNormal(i.shorelineUVAnim2, i.shorelineWaveData2);
	

	float terrainDepth = ComputeWaterOrthoDepth(i.worldPos);
	float shorelineNearDepthMask = saturate(terrainDepth - KW_WaterPosition.y + 0.85);
	normal = lerp(normal, float3(0, 1, 0), shorelineNearDepthMask);

	normal = NormalsCombine(normal, shorelineWave1, shorelineWave2);
	//return float4(shorelineNormal.xz, 0, 1);
#endif
	
	half sssMask = tex2Dlod(KW_WaterMaskScatterNormals_Blured, float4(i.screenPos.xy / i.screenPos.w, 0, 0)).y;
	//half2 waterNormals = tex2Dlod(KW_WaterMaskScatterNormals_Blured, float4(i.screenPos.xy / i.screenPos.w, 0, 0)).zw;
	//return float4(sssBlured, 0, 0, 1);
	
	float depthFix = dot(i.viewDir, float3(0, 1, 0));
	
	//depthFix = 1;
	///////////////////////////////////////////////////////////////////// REFRACTION ///////////////////////////////////////////////////////////////////
	

	//return float4(bluredDepth.xxx*1000, 1);
	float sceneZRefr = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTextureBeforeWaterZWrite_Blured, i.screenPos.xy / i.screenPos.w));
	float surfaceDepthRefr = (i.screenPos.w);
	float fadeRefr = clamp((sceneZRefr - surfaceDepthRefr) * depthFix, 0.5, min(KW_Transparent, 5));
	
	float3 refractedRay = ComputeWaterRefractRay(-i.viewDir, normal, fadeRefr * 1.5);
	

	float4 refractedClipPos = mul(UNITY_MATRIX_VP, float4(i.worldPosRefracted + refractedRay, 1.0));
	float4 refractionScreenPos = ComputeScreenPos(refractedClipPos);

	
#if USE_VOLUMETRIC_LIGHT
	half4 volumeScattering = tex2D(KW_VolumetricLight, refractionScreenPos.xy / refractionScreenPos.w);
	//return volumeScattering;
#else
	half4 volumeScattering = half4(0.5, 0.5, 0.5, 1.0);
#endif

	
	half3 refraction = ComputeRefractionWithDispersion(_CameraColorTexture, i.worldPosRefracted, refractedRay);
	

	float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, refractionScreenPos));
	float surfaceDepth = (i.screenPos.w);
	float fade = (sceneZ - surfaceDepth);
	
	if (fade < 0)
	{
		//return float4(1, 0, 0, 1);
		sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
		fade = (sceneZ - surfaceDepth);

		refraction = tex2Dproj(_CameraColorTexture, i.grabPos);
		volumeScattering = tex2Dproj(KW_VolumetricLight, UNITY_PROJ_COORD(i.screenPos));
	
	}
	
	fade = (sceneZ - (i.screenPos.w)) * saturate(0.3 + depthFix);
	half surfaceTensionFade = saturate((LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos))) - (i.screenPos.w)) * 10);

	half3 reflection;
#if PLANAR_REFLECTION
	float2 refl_uv = ComputeScreenSpaceReflectionDir(normal, i.viewDir);
	refl_uv.y -= KW_PlanarReflectionClipPlaneOffset;
	reflection = tex2D(KW_PlanarReflection, refl_uv);
#else
	
	float3 reflVec = reflect(-i.viewDir, normal);
	reflection = UNITY_SAMPLE_TEXCUBE(KW_ReflectionCube, reflVec);
	
	#if SSPR_REFLECTION
		float2 refl_uv = ComputeScreenSpaceReflectionDir(normal, i.viewDir);
		refl_uv.y -= KW_SSR_ClipOffset;
		float4 ssr = tex2D(KW_ScreenSpaceReflectionTex, refl_uv).xyzw;
		reflection.xyz = lerp(reflection.xyz, ssr.xyz, ssr.a);
	#endif
#endif
	
	float viewDistNormalized = saturate(viewDist / (KW_WaterFarDistance * 2));
	half3 underwaterColor = ComputeUnderwaterColor(refraction, volumeScattering.rgb,  fade, KW_Transparent, KW_WaterColor, KW_Turbidity, KW_TurbidityColor);
	
	float2 fluidsUV = (i.worldPos.xz - KW_DepthPos.xz - KW_WaterPosition.xz * 0) / KW_DepthOrthographicSize + 0.5;
	float4 fluids = tex2D(_FluidsTex, fluidsUV);
	
	float2 fluidTexel = _FluidsTex_TexelSize.xy * (int)Test.w;
	float3 tl = tex2D(_FluidsTex, fluidsUV - float2(fluidTexel.x, 0));
	float3 tr = tex2D(_FluidsTex, fluidsUV + float2(fluidTexel.x, 0));
	float3 tu = tex2D(_FluidsTex, fluidsUV + float2(0, fluidTexel.y));
	float3 td = tex2D(_FluidsTex, fluidsUV - float2(0, fluidTexel.y));
	
	float3 dx = (tr.xyz - tl.xyz);
	float3 dy = (tu.xyz - td.xyz);
	float2 densDif = float2(dx.z, dy.z);

	float dens = dot(float3(densDif, dx.x + dy.y), fluids.xyz); //density
	float2 laplacian = tu.xy + td.xy + tr.xy + tl.xy - 4.0 * fluids.xy;
	float divergence = saturate(max(length(dx), length(dy)) - 0.1);

	//return float4(divergence.xxx , 1);


	//float diff = max(0, fluids.x - fluids.y) + max(0, fluids.y - fluids.x);
	//float foamMask = saturate((abs(fluids.yyy) - Test.w));
	float3 foam = tex2D(_FoamTex, i.worldPos.xz / 40  + fluids.xy * 0.15 + float2(-_Time.x * 1, 0));
	underwaterColor = lerp(underwaterColor, lerp(underwaterColor, 1, foam), 1 * divergence);
	

#if FIX_UNDERWATER_SKY_REFLECTION
	#if PLANAR_REFLECTION
		float3 reflVec = reflect(-i.viewDir, normal);
	#endif
	half reflectionBounceMask = 1 - saturate(dot(reflVec, float3(0, 1, 0)));
	reflectionBounceMask = Pow5(reflectionBounceMask);
	reflection = lerp(reflection, max(underwaterColor, reflection), reflectionBounceMask);
#endif
	half linearFresnel = ComputeLinearFresnel(normal, i.viewDir);
	half waterFresnel = ComputeWaterFresnel(linearFresnel);
	
	half3 finalColor = lerp(underwaterColor, reflection, waterFresnel);

#if REFLECT_SUN
	finalColor += ComputeSunlight(normal, i.viewDir, linearFresnel, KW_DirLightForward, KW_DirLightColor, volumeScattering.a, viewDistNormalized, KW_Transparent);
#endif
	finalColor += ComputeSSS(sssMask, underwaterColor, volumeScattering.a, KW_Transparent);
	


	

	return float4(finalColor, surfaceTensionFade);
}

v2fDepth vertDepth(float4 vertex : POSITION)
{
	v2fDepth o;
	o.worldPos = mul(unity_ObjectToWorld, vertex);
	float3 waterOffset = ComputeWaterOffset(o.worldPos);

#if USE_SHORELINE
	ShorelineData shorelineData;
	float3 beachOffset = ComputeBeachWaveOffset(o.worldPos, shorelineData);
	float terrainDepth = ComputeWaterOrthoDepth(o.worldPos);
	vertex.xyz += lerp(waterOffset, 0, saturate(terrainDepth - KW_WaterPosition.y + 0.85));
	vertex.xyz += beachOffset;
#else
	vertex.xyz += waterOffset;
#endif

	o.pos = UnityObjectToClipPos(vertex);
	
	return o;
}


half4 fragDepth(v2fDepth i, float facing : VFACE) : SV_Target
{
		//FragmentOutput o;

		float2 uv = i.worldPos.xz / KW_FFTDomainSize;
		
		half3 norm = tex2D(KW_NormTex, uv);
		half3 normScater = tex2Dlod(KW_NormTex, float4(uv, 0, KW_NormalScattering_Lod));
		
		#ifdef USE_MULTIPLE_SIMULATIONS
			half3 normScater_lod1 = tex2Dlod(KW_NormTex_LOD1, float4(i.worldPos.xz / KW_FFTDomainSize_LOD1, 0, 2));
			half3 normScater_lod2 = tex2Dlod(KW_NormTex_LOD2, float4(i.worldPos.xz / KW_FFTDomainSize_LOD2, 0, 1));
			normScater = normalize(half3(normScater.xz + normScater_lod1.xz + normScater_lod2.xz, normScater.y * normScater_lod1.y * normScater_lod2.y)).xzy;

			half3 norm_lod1 = tex2D(KW_NormTex_LOD1, i.worldPos.xz / KW_FFTDomainSize_LOD1);
			half3 norm_lod2 = tex2D(KW_NormTex_LOD2,  i.worldPos.xz / KW_FFTDomainSize_LOD2);
			norm = normalize(half3(norm.xz + norm_lod1.xz + norm_lod2.xz, norm.y * norm_lod1.y * norm_lod2.y)).xzy;
		#endif


		float3 viewDir = (_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
		float distance = length(viewDir);
		viewDir = normalize(viewDir);
		int idx;
		half sss = 0;
		half windLimit = clamp(KW_WindSpeed - 0.25, -0.25, 1);
		windLimit -= clamp((KW_WindSpeed - 4) * 0.25, 0, 0.8);
			
			float3 lightDir = KW_DirLightForward.xyz;
			
			half zeroScattering = saturate(dot(viewDir, -(lightDir + float3(0, 1, 0))));

			float3 H = (lightDir + norm * float3(-1, 1, -1));
			float scattering = (dot(viewDir, -H));
			sss += windLimit * (scattering - zeroScattering  * 0.95);

		

		/*float mipUV = i.screenPos.xy / KW_FFTDomainSize;
		float2 dx = ddx(mipUV * KW_NormTex_TexelSize.zw);
		float2 dy = ddy(mipUV * KW_NormTex_TexelSize.zw);
		float d = max(dot(dx, dx), dot(dy, dy));
		const float rangeClamp = pow(2.0, (KW_NormMipCount - 1) * 2.0);
		d = clamp(d, 1.0, rangeClamp);
		float mipLevel = 0.5 * log2(d);
		mipLevel = floor(mipLevel);*/

		//o.dest0 =  half4(saturate(0.75 - facing * 0.25), i.pos.z, saturate(sss), 0);  //-1 back face, 1 fron face
		//o.dest1 = half4(norm, 1);
	
		return  half4(saturate(0.75 - facing * 0.25) , saturate(sss - 0.1), norm.xz * 0.5 + 0.5);
}

v2f ComputeVertexInterpolators(v2f o, float3 worldPos, float4 vertex : POSITION)
{
	o.pos = UnityObjectToClipPos(vertex);
	o.viewDir = _WorldSpaceCameraPos.xyz - worldPos.xyz;
	o.worldPosRefracted = mul(unity_ObjectToWorld, vertex).xyz;
	o.screenPos = ComputeScreenPos(o.pos);
	o.grabPos = ComputeGrabScreenPos(o.pos);

	return o;
}

v2f ComputeShorelineVertexInterpolators(v2f o, ShorelineData shorelineData)
{
	o.shorelineUVAnim1 = shorelineData.uv1;
	o.shorelineUVAnim2 = shorelineData.uv2;
	o.shorelineWaveData1 = shorelineData.data1;
	o.shorelineWaveData2 = shorelineData.data2;

	return o;
}

v2f vert(float4 vertex : POSITION)
{
	v2f o;
	o.worldPos = mul(unity_ObjectToWorld, vertex);

	float3 waterOffset = ComputeWaterOffset(o.worldPos);
#if USE_SHORELINE
	ShorelineData shorelineData;
	float3 beachOffset = ComputeBeachWaveOffset(o.worldPos, shorelineData);
	float terrainDepth = ComputeWaterOrthoDepth(o.worldPos);
	vertex.xyz += lerp(waterOffset, 0, saturate(terrainDepth - KW_WaterPosition.y + 0.85));
	vertex.xyz += beachOffset;
	
	o = ComputeShorelineVertexInterpolators(o, shorelineData);
#else
	vertex.xyz += waterOffset;
#endif
	o = ComputeVertexInterpolators(o, o.worldPos.xyz, vertex);
	return o;
}


half4 frag(v2f i, float facing : VFACE) : SV_Target
{
	//	float2 depthUV = (i.worldPos.xz) / KW_DepthOrthoSize * 0.5 + 0.5;
	//float4 bakedWavesPosition = tex2Dlod(KW_BakedWavesPosition, float4(depthUV, 0, 0));
	//if (bakedWavesPosition.z > 0.5) discard;
	return ComputeWaterColor(i, facing);
}


