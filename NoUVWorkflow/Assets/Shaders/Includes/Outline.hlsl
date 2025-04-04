// This code is an adaptation of the open-source work by Alexander Ameye
// From a tutorial originally posted here:
// https://alexanderameye.github.io/outlineshader
// Code also available on his Gist account
// https://gist.github.com/AlexanderAmeye

TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
float4 _CameraDepthTexture_TexelSize;

TEXTURE2D(_CameraDepthNormalsTexture);
SAMPLER(sampler_CameraDepthNormalsTexture);
 
float3 DecodeNormal(float4 enc)
{
    float kScale = 1.7777;
    float3 nn = enc.xyz*float3(2*kScale,2*kScale,0) + float3(-kScale,-kScale,1);
    float g = 2.0 / dot(nn.xyz,nn.xyz);
    float3 n;
    n.xy = g*nn.xy;
    n.z = g-1;
    return n;
}

float3 ViewDirectionFromScreenUV(float2 In) {
    // Code by Keijiro Takahashi @_kzr and Ben Golus @bgolus
    // Get the perspective projection
    float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
    // Convert the uvs into view space by "undoing" projection
    return -normalize(float3((In * 2 - 1) / p11_22, -1));
}

void OutlineObject_float(float2 UV, float OutlineThickness, float DepthSensitivity, 
float DepthThreshold, float DepthTightening, float DepthStrength, float NormalsSensitivity, 
float NormalsThreshold, float NormalsTightening, float NormalsStrength, float AcuteDepthStartDot, float AcuteDepthThresholdMult,
float FarDepthStart, float FarDepthThresholdMult, float FarNormalStartDepth, float FarNormalThresholdMult, out float Out)
{
    float td = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, UV).r;
    float fade = saturate(1.5*td);
    float thicc = OutlineThickness;
    float halfScaleFloor = floor(thicc * 0.5);
    float halfScaleCeil = ceil(thicc * 0.5);
    
    float2 uvSamples[4];
    float depthSamples[4];
    float3 normalSamples[4];

    uvSamples[0] = UV - float2(_CameraDepthTexture_TexelSize.x, _CameraDepthTexture_TexelSize.y) * halfScaleFloor;
    uvSamples[1] = UV + float2(_CameraDepthTexture_TexelSize.x, _CameraDepthTexture_TexelSize.y) * halfScaleCeil;
    uvSamples[2] = UV + float2(_CameraDepthTexture_TexelSize.x * halfScaleCeil, -_CameraDepthTexture_TexelSize.y * halfScaleFloor);
    uvSamples[3] = UV + float2(-_CameraDepthTexture_TexelSize.x * halfScaleFloor, _CameraDepthTexture_TexelSize.y * halfScaleCeil);

    [unroll] for(int i = 0; i < 4 ; i++)
    {
        depthSamples[i] = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvSamples[i]).r;
        normalSamples[i] = DecodeNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uvSamples[i]));
    }

    // Depth
    float depthFiniteDifference0 = depthSamples[1] - depthSamples[0];
    float depthFiniteDifference1 = depthSamples[3] - depthSamples[2];
    float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;
    //float depthThreshold = (1/DepthSensitivity) * depthSamples[0];
    //edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

    // Normals
    float3 normalFiniteDifference0 = normalSamples[1] - normalSamples[0];
    float3 normalFiniteDifference1 = normalSamples[3] - normalSamples[2];
    float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
    //edgeNormal = edgeNormal > (1/NormalsSensitivity) ? 1 : 0;

    float3 tn = DecodeNormal(SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, UV));
    float3 tvd = ViewDirectionFromScreenUV(UV);
    float depthThreshold = DepthThreshold * (smoothstep(AcuteDepthStartDot, 1, 1 - dot(tvd, tn)) * AcuteDepthThresholdMult + smoothstep(FarDepthStart, 1, td) * FarDepthThresholdMult + 1);
    float normalsThreshold = NormalsThreshold * (smoothstep(FarNormalStartDepth, 1, td) * FarNormalThresholdMult + 1);

    edgeDepth = pow(smoothstep(depthThreshold, 1, edgeDepth), DepthTightening) * DepthStrength;
    edgeNormal = pow(smoothstep(normalsThreshold, 1, edgeNormal), NormalsTightening) * NormalsStrength;

    float edge = max(edgeDepth, edgeNormal) * fade;
    Out = edge;
}