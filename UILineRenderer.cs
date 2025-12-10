using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UILineRenderer : MonoBehaviour
{
    RectTransform targetA;
    List<RectTransform> targetBList;
    public float thickness = 5f;
    public GameObject linePrefab;

    private RectTransform rectTransform;
    private Image targetAImage;
    // 생성된 모든 선의 Image 컴포넌트를 저장하는 리스트 (사후 색상 처리를 위해 필요)
    private List<Image> generatedLineImages = new List<Image>();

    public void Init(RectTransform _targetA, List<RectTransform> _targetBList)
    {
        targetA = _targetA;
        targetBList = _targetBList;
        // 1. 초기화 및 필수 컴포넌트 검사 (기존 로직 유지)
        rectTransform = GetComponent<RectTransform>();
        generatedLineImages.Clear(); // 혹시 모를 재시작을 위해 초기화

        if (targetA == null)
        {
            Debug.LogError("UILineRenderer： **targetA가 할당되지 않았습니다**： 동작을 종료합니다.");
            return;
        }
        targetA.gameObject.SetActive(true);
        targetAImage = targetA.GetComponent<Image>();
        if (targetAImage == null)
        {
            Debug.LogError("UILineRenderer： **targetA**에 Image 컴포넌트가 없습니다： 색상 제어 기능을 사용할 수 없습니다.");
            gameObject.SetActive(false); return;
        }

        if (targetBList == null || targetBList.Count == 0)
        {
            Debug.LogWarning("UILineRenderer： targetB List에 할당된 타겟이 없습니다： targetA를 비활성화합니다.");
            targetA.gameObject.SetActive(false);
            gameObject.SetActive(false); return;
        }
        foreach (var item in targetBList)
        {
            item.gameObject.SetActive(true);
        }
        if (linePrefab == null || linePrefab.GetComponent<Image>() == null)
        {
            Debug.LogError("UILineRenderer： **linePrefab이 할당되지 않았거나 Image 컴포넌트가 없습니다**： 선을 그릴 수 없어 자신을 비활성화합니다.");
            gameObject.SetActive(false); return;
        }

        // 2. 선 그리기 로직 실행
        DrawAllLines();

        // 3.  모든 선을 그린 사후에 색상 제어기 호출
        if (generatedLineImages.Count > 0)
        {
            LineColorController.ApplyColors(targetAImage, generatedLineImages, targetBList);
        }

        Debug.Log($"UILineRenderer： 선 그리기를 완료하고 LineColorController를 호출했습니다.");
    }

    private void DrawAllLines()
    {
        // localPosA 변수를 한 번만 선언합니다.
        Vector2 localPosA;

        // 캔버스 로컬 좌표계로 변환하는 가장 일반적이고 안정적인 방법 중 하나를 선택합니다.
        // InverseTransformPoint를 사용하여 월드 위치를 UILineRenderer의 로컬 위치로 변환합니다.
        Vector3 localPosA3D = rectTransform.InverseTransformPoint(targetA.position);
        localPosA = new Vector2(localPosA3D.x, localPosA3D.y);

        for (int i = 0; i < targetBList.Count; i++)
        {
            RectTransform targetB = targetBList[i];
            if (targetB == null) continue;

            // 1. 선 오브젝트 생성 및 부모 설정 (기존 로직 유지)
            GameObject line = Instantiate(linePrefab, rectTransform);
            RectTransform lineRect = line.GetComponent<RectTransform>();
            Image lineImage = line.GetComponent<Image>();

            if (lineImage == null) continue;
            generatedLineImages.Add(lineImage);

            // 2. targetB의 월드 위치를 UILineRenderer의 로컬 좌표로 변환
            // localPosB 변수를 for 루프 내부에서 한 번만 선언합니다.
            Vector2 localPosB;
            Vector3 localPosB3D = rectTransform.InverseTransformPoint(targetB.position);
            localPosB = new Vector2(localPosB3D.x, localPosB3D.y);

            // 3. 선 그리기 로직 (로컬 좌표 사용)
            Vector2 difference = localPosB - localPosA; // 로컬 좌표 차이 계산
            float distance = difference.magnitude;
            float angle = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

            lineRect.sizeDelta = new Vector2(distance, thickness);

            // 선의 중앙 위치는 로컬 좌표 A와 로컬 좌표 B의 중간 지점입니다.
            lineRect.anchoredPosition = localPosA + difference * 0.5f;

            // 로컬 회전 설정
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);

            line.name = $"Line_{targetA.name}_to_{targetB.name}";
        }
    }
}