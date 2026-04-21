using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TMPCustomEffect : MonoBehaviour
{
    public TMP_Text text;

    public void SetGradientText(string t)
    {
        string raw = t;

        // 파싱
        var parsed = GradientTextParser.Parse(raw, out var tags);

        // 텍스트 넣기
        text.text = parsed;

        // Mesh 업데이트
        text.ForceMeshUpdate();

        // 효과 적용
        ApplyEffects(tags);
    }

    void ApplyEffects(List<GradientTextParser.TagInfo> tags)
    {
        // 색상 효과 모두 적용
        foreach (var tag in tags)
        {
            if (tag.tag.Contains("gr_"))
            {
                ApplyColorEffect(tag);
            }
        }
        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // shake 효과 (색상 보존)
        foreach (var tag in tags)
        {
            if (tag.tag.Contains("shake") && !tag.tag.Contains("wave"))
            {
                StartCoroutine(ApplyShakeEffect(tag.start, tag.end));
            }
        }

        // wave 효과 (색상 보존)
        foreach (var tag in tags)
        {
            if (tag.tag.Contains("wave"))
            {
                StartCoroutine(ApplyWaveEffect(tag.start, tag.end));
            }
        }
    }

    void ApplyColorEffect(GradientTextParser.TagInfo tag)
    {
        var textInfo = text.textInfo;
        int charCount = textInfo.characterInfo.Length;
        
        // 범위 검증
        if (tag.start < 0 || tag.end >= charCount || tag.start > tag.end)
        {
            Debug.LogWarning($"Invalid tag range: start={tag.start}, end={tag.end}, charCount={charCount}");
            return;
        }

        // 색상 타입 추출
        string colorType = tag.tag.Replace("shake_", "").Replace("wave_", "").Replace("shake", "").Replace("wave", "");

        if (colorType.Contains("gr_SB"))// 스카이 블루
            ApplyHorizontalGradient(tag.start, tag.end, Color.blue, Color.cyan, 1, 5);
        else if (colorType.Contains("gr_B") && !colorType.Contains("gr_RBW"))// 블루
            ApplyHorizontalGradient(tag.start, tag.end, Color.blue, Color.blue, 1, 5);
        else if (colorType.Contains("gr_R") && !colorType.Contains("gr_RBW"))// 레드
            ApplyHorizontalGradient(tag.start, tag.end, Color.white, Color.red, 1, 5);
        else if (colorType.Contains("gr_O"))// 오렌지
            ApplyHorizontalGradient(tag.start, tag.end, Color.yellow, Color.red, 1, 5);
        else if (colorType.Contains("gr_Y"))// 옐로우
            ApplyHorizontalGradient(tag.start, tag.end, Color.white, Color.yellow, 1, 5);
        else if (colorType.Contains("gr_P"))// 퍼플
            ApplyHorizontalGradient(tag.start, tag.end, Color.magenta, Color.blue, 1, 5);
        else if (colorType.Contains("gr_RBW")) // 레인보우
            ApplyThreeColorGradient(tag.start, tag.end, Color.red, Color.yellow, Color.cyan);
        else if (colorType.Contains("gr_G")) // 그린
            ApplyHorizontalGradient(tag.start, tag.end, Color.green, Color.green, 1, 5);
        else if (colorType.Contains("gr_BK")) // 블랙
            ApplyHorizontalGradient(tag.start, tag.end, Color.black, Color.black, 1, 5);

    }

    IEnumerator ApplyShakeEffect(int start, int end)
    {
        var textInfo = text.textInfo;
        float cycleDuration = 0.3f;  // 한 사이클 길이
        float shakeAmount = 1f;

        // 원본 위치 저장
        var originalVertices = new Vector3[end - start + 1][];
        for (int i = start; i <= end; i++)
        {
            if (i < 0 || i >= textInfo.characterInfo.Length) continue;
            
            var charInfo = textInfo.characterInfo[i];
            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;
            
            if (meshIndex >= textInfo.meshInfo.Length) continue;
            
            originalVertices[i - start] = new Vector3[4];
            var verts = textInfo.meshInfo[meshIndex].vertices;
            System.Array.Copy(verts, vertexIndex, originalVertices[i - start], 0, 4);
        }

        // 무한 반복
        while (true)
        {
            float elapsed = 0f;
            
            while (elapsed < cycleDuration)
            {
                for (int i = start; i <= end; i++)
                {
                    if (i < 0 || i >= textInfo.characterInfo.Length) continue;

                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    int vertexIndex = charInfo.vertexIndex;
                    int meshIndex = charInfo.materialReferenceIndex;
                    
                    if (meshIndex >= textInfo.meshInfo.Length) continue;
                    
                    var verts = textInfo.meshInfo[meshIndex].vertices;

                    Vector3 randomShake = new Vector3(
                        Random.Range(-shakeAmount, shakeAmount),
                        Random.Range(-shakeAmount, shakeAmount),
                        0
                    );

                    for (int j = 0; j < 4; j++)
                    {
                        verts[vertexIndex + j] = originalVertices[i - start][j] + randomShake;
                    }
                }

                // Vertices만 업데이트 (Colors는 유지)
                text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 한 사이클 끝나면 원위치 복원
            for (int i = start; i <= end; i++)
            {
                if (i < 0 || i >= textInfo.characterInfo.Length) continue;
                
                var charInfo = textInfo.characterInfo[i];
                int vertexIndex = charInfo.vertexIndex;
                int meshIndex = charInfo.materialReferenceIndex;
                
                if (meshIndex >= textInfo.meshInfo.Length) continue;
                
                var verts = textInfo.meshInfo[meshIndex].vertices;
                System.Array.Copy(originalVertices[i - start], 0, verts, vertexIndex, 4);
            }
            
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }

    IEnumerator ApplyWaveEffect(int start, int end)
    {
        var textInfo = text.textInfo;
        float waveSpeed = 2f;  // 파도 움직임 속도
        float waveFrequency = 0.5f;  // 문자 간 위상 간격
        float waveAmplitude = 3f;  // 움직임 크기 (증가됨)
        
        // 원본 위치 저장
        var originalVertices = new Vector3[end - start + 1][];
        for (int i = start; i <= end; i++)
        {
            if (i < 0 || i >= textInfo.characterInfo.Length) continue;
            
            var charInfo = textInfo.characterInfo[i];
            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;
            
            if (meshIndex >= textInfo.meshInfo.Length) continue;
            
            originalVertices[i - start] = new Vector3[4];
            var verts = textInfo.meshInfo[meshIndex].vertices;
            System.Array.Copy(verts, vertexIndex, originalVertices[i - start], 0, 4);
        }

        // 무한 반복
        while (true)
        {
            for (int i = start; i <= end; i++)
            {
                if (i < 0 || i >= textInfo.characterInfo.Length) continue;

                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int vertexIndex = charInfo.vertexIndex;
                int meshIndex = charInfo.materialReferenceIndex;
                
                if (meshIndex >= textInfo.meshInfo.Length) continue;
                
                var verts = textInfo.meshInfo[meshIndex].vertices;

                // 사인파 이용한 위아래 움직임
                float waveOffset = Mathf.Sin(Time.time * waveSpeed + (i - start) * waveFrequency) * waveAmplitude;

                for (int j = 0; j < 4; j++)
                {
                    verts[vertexIndex + j] = originalVertices[i - start][j] + new Vector3(0, waveOffset, 0);
                }
            }

            // Vertices만 업데이트 (Colors는 유지)
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            yield return null;
        }
    }

    void ApplyVerticalGradient(int start, int end, Color topColor, Color bottomColor, int topRatio, int bottomRatio)
    {
        var textInfo = text.textInfo;
        
        if (start < 0 || end >= textInfo.characterInfo.Length) return;

        // 비율 계산: 위쪽이 차지할 비중 = topRatio / (topRatio + bottomRatio)
        float totalRatio = topRatio + bottomRatio;
        float topLerp = topRatio / totalRatio;  // 0 ~ 1 범위

        for (int i = start; i <= end; i++)
        {
            if (i < 0 || i >= textInfo.characterInfo.Length) continue;
            
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;

            if (meshIndex >= textInfo.meshInfo.Length) continue;

            var colors = textInfo.meshInfo[meshIndex].colors32;

            // topRatio : bottomRatio 비율에 따라 색상 배분
            // vertices[0,1] = 아래, vertices[2,3] = 위
            colors[vertexIndex + 0] = Color.Lerp(topColor, bottomColor, 0f);        // 왼쪽 아래 (bottomColor)
            colors[vertexIndex + 1] = Color.Lerp(topColor, bottomColor, 0f);        // 오른쪽 아래 (bottomColor)
            colors[vertexIndex + 2] = Color.Lerp(topColor, bottomColor, topLerp);   // 오른쪽 위
            colors[vertexIndex + 3] = Color.Lerp(topColor, bottomColor, topLerp);   // 왼쪽 위
        }
    }
    void ApplyHorizontalGradient(int start, int end, Color leftColor, Color rightColor, int leftRatio, int rightRatio)
    {
        var textInfo = text.textInfo;
        
        if (start < 0 || end >= textInfo.characterInfo.Length) return;

        // 비율 계산: 왼쪽이 차지할 비중 = leftRatio / (leftRatio + rightRatio)
        float totalRatio = leftRatio + rightRatio;
        float leftLerp = leftRatio / totalRatio;  // 0 ~ 1 범위

        for (int i = start; i <= end; i++)
        {
            if (i < 0 || i >= textInfo.characterInfo.Length) continue;
            
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;

            if (meshIndex >= textInfo.meshInfo.Length) continue;

            var colors = textInfo.meshInfo[meshIndex].colors32;

            // leftRatio : rightRatio 비율에 따라 색상 배분
            // vertices[0,3] = 왼쪽, vertices[1,2] = 오른쪽
            colors[vertexIndex + 0] = Color.Lerp(rightColor, leftColor, leftLerp);  // 왼쪽 아래
            colors[vertexIndex + 3] = Color.Lerp(rightColor, leftColor, leftLerp);  // 왼쪽 위
            colors[vertexIndex + 1] = Color.Lerp(rightColor, leftColor, 0f);        // 오른쪽 아래 (rightColor)
            colors[vertexIndex + 2] = Color.Lerp(rightColor, leftColor, 0f);        // 오른쪽 위 (rightColor)
        }
    }
    void ApplyThreeColorGradient(int start, int end, Color topLeftColor, Color topRightColor, Color bottomColor)
    {
        var textInfo = text.textInfo;
        
        if (start < 0 || end >= textInfo.characterInfo.Length) return;

        for (int i = start; i <= end; i++)
        {
            if (i < 0 || i >= textInfo.characterInfo.Length) continue;
            
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;

            if (meshIndex >= textInfo.meshInfo.Length) continue;

            var colors = textInfo.meshInfo[meshIndex].colors32;

            // 위쪽 좌측, 위쪽 우측, 아래쪽 중앙
            colors[vertexIndex + 0] = bottomColor; // 왼쪽 아래
            colors[vertexIndex + 1] = bottomColor; // 오른쪽 아래
            colors[vertexIndex + 2] = topRightColor; // 오른쪽 위
            colors[vertexIndex + 3] = topLeftColor; // 왼쪽 위
        }
    }
    void ApplyFourCornerGradient(int start, int end, Color topLeft, Color topRight, Color bottomRight, Color bottomLeft)
    {
        var textInfo = text.textInfo;
        
        if (start < 0 || end >= textInfo.characterInfo.Length) return;

        for (int i = start; i <= end; i++)
        {
            if (i < 0 || i >= textInfo.characterInfo.Length) continue;
            
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;

            if (meshIndex >= textInfo.meshInfo.Length) continue;

            var colors = textInfo.meshInfo[meshIndex].colors32;

            colors[vertexIndex + 0] = bottomLeft; // 왼쪽 아래
            colors[vertexIndex + 1] = bottomRight; // 오른쪽 아래
            colors[vertexIndex + 2] = topRight; // 오른쪽 위
            colors[vertexIndex + 3] = topLeft; // 왼쪽 위
        }
    }
}
