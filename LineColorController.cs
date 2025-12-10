using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public static class LineColorController
{
    // Hex 코드를 Color 구조체로 변환한 상수 정의
    private static readonly Color LOCKED_COLOR = new Color(120f / 255f, 120f / 255f, 120f / 255f, 1f); // 787878
    private static readonly Color UNLOCKED_COLOR = new Color(255f / 255f, 101f / 255f, 115f / 255f, 1f); // FF6573

    /// <summary>
    /// UILineRenderer가 선을 모두 그린 후 호출되어, targetA, 모든 선, 그리고 targetB의 색상 변경만 담당합니다.
    /// </summary>
    /// <param name="targetAImage">targetA의 Image 컴포넌트</param>
    /// <param name="lineImages">생성된 모든 선의 Image 컴포넌트 리스트</param>
    /// <param name="targetBs">targetA와 연결된 targetB RectTransform 리스트</param>
    public static void ApplyColors(Image targetAImage, List<Image> lineImages, List<RectTransform> targetBs)
    {
        if (targetAImage == null) return;

        bool isAnyTargetUnlocked = false;

        // 1. 모든 targetB를 순회하며 선/targetB의 색상 설정 및 targetA 상태 플래그 업데이트
        for (int i = 0; i < targetBs.Count; i++)
        {
            RectTransform targetB = targetBs[i];
            Image lineImage = lineImages[i];

            if (targetB == null || lineImage == null) continue;

            // LineTarget 상태 확인
            LineTarget lineTarget = targetB.GetComponent<LineTarget>();
            bool isLocked = lineTarget == null || lineTarget.isLocked;

            Color currentColor = isLocked ? LOCKED_COLOR : UNLOCKED_COLOR;

            // 선 색상 설정
            lineImage.color = currentColor;

            // targetB 색상 설정
            Image targetBImage = targetB.GetComponent<Image>();
            if (targetBImage != null)
            {
                targetBImage.color = currentColor;
            }

            // targetA 상태 플래그 업데이트
            if (!isLocked)
            {
                isAnyTargetUnlocked = true;
            }
        }

        // 2. targetA 색상 설정 (모든 반복문 완료 후)
        targetAImage.color = isAnyTargetUnlocked ? UNLOCKED_COLOR : LOCKED_COLOR;
    }
}