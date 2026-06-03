// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Unity
{
  /*주석 추가함*/
  // 부모 UI 영역의 해상도 규격에 맞춰 자신(영상/카메라 화면)의 RectTransform 스케일 및 offset 비율을 
  // 자동으로 맞춰주는 반응형 디스플레이 핏 유틸리티입니다.
  public class AutoFit : MonoBehaviour
  {
    [System.Serializable]
    public enum FitMode
    {
      Expand,    // 부모 영역에 빈 공간이 남지 않도록 자식을 비율 맞춰 꽉 채워 확대하는 모드 (화면 잘림 발생 가능)
      Shrink,    // 자식 화면 전체가 부모 내부에 온전히 담기도록 비율 맞춰 축소 및 맞춤하는 모드 (레터박스 발생 가능)
      FitWidth,  // 부모의 가로(Width) 너비에 무조건적으로 자식의 가로축을 맞추는 모드
      FitHeight, // 부모의 세로(Height) 높이에 무조건적으로 자식의 세로축을 맞추는 모드
    }

    [SerializeField] private FitMode _fitMode; // 인스펙터에서 지정할 자동 화면 맞춤 모드 플래그

    /*주석 추가함*/
    // UI 애니메이션이나 부모 크기 변화가 완전히 끝난 뒤 정밀 정렬하기 위해 LateUpdate 라이프사이클을 채택
    private void LateUpdate()
    {
      var rectTransform = GetComponent<RectTransform>();
      // 자신의 UI 사각형 영역 가로/세로 규격이 0일 경우 연산 에러 방지를 위해 가드 탈출
      if (rectTransform.rect.width == 0 || rectTransform.rect.height == 0)
      {
        return;
      }

      // 나를 감싸고 있는 직속 부모 UI 패널의 실제 픽셀 가로/세로 사각형 캔버스 영역을 가져
      var parentRect = gameObject.transform.parent.gameObject.GetComponent<RectTransform>().rect;
      // UI 자전 회전값까지 정밀 연산에 가산하여 왜곡 보정된 자식의 실제 바운딩 박스 가로, 세로 길이를 도출
      var (width, height) = GetBoundingBoxSize(rectTransform);

      // 1차 연산 단계: 부모 가로 폭 기준으로 자식 가로를 맞출 때의 배율(ratio)과 이에 따른 예상 세로 높이(h)를 구함
      var ratio = parentRect.width / width;
      var h = height * ratio;

      /*주석 추가함*/
      // 가로 맞춤 모드이거나, 
      // Expand/Shrink 조건문 상에서 가로 정렬이 적합하다고 판단된 경우 분기 진입
      if (_fitMode == FitMode.FitWidth || (_fitMode == FitMode.Expand && h >= parentRect.height) || (_fitMode == FitMode.Shrink && h <= parentRect.height))
      {
        // RectTransform의 상하좌우 패딩 오프셋 매트릭스 수치에 최종 스케일 배율을 곱해 UI 형태를 안전 변형
        rectTransform.offsetMin *= ratio;
        rectTransform.offsetMax *= ratio;
        return;
      }

      //  위의 가로축 기준 판정을 통과하지 못했다면 세로축(Height) 배율을 기준으로 역산하여 
      //  스케일 오프셋을 최종 갱신합니다.
      ratio = parentRect.height / height;

      rectTransform.offsetMin *= ratio;
      rectTransform.offsetMax *= ratio;
    }

    /*주석 추가함*/
    // UI 요소를 강제로 회전시켰을 때 사각형 꼭짓점이 찌그러지거나 왜곡되는 현상을 막기 위해 
    // 겉면을 감싸는 정확한 바운딩 박스 크기를 삼각함수 벡터 쿼터니언 연산으로 도출
    private (float, float) GetBoundingBoxSize(RectTransform rectTransform)
    {
      var rect = rectTransform.rect;
      var center = rect.center; // 사각형의 중심점 확보
      
      // 중심점 기준 좌측 상단 및 우측 상단의 상대적 로컬 거리를 픽셀 좌표로 가산
      var topLeftRel = new Vector2(rect.xMin - center.x, rect.yMin - center.y);
      var topRightRel = new Vector2(rect.xMax - center.x, rect.yMin - center.y);
      
      // 트랜스폼에 적용된 최종 UI 회전 수치(Quaternion)를 
      // 상대 벡터에 행렬 연산 연쇄 결합하여 실제 회전된 꼭짓점의 최종 2D 좌표를 추출
      var rotatedTopLeftRel = rectTransform.localRotation * topLeftRel;
      var rotatedTopRightRel = rectTransform.localRotation * topRightRel;
      
      // 회전으로 인해 튀어나간 X축 및 Y축 꼭짓점 원거리 값의 절대값 중 최대치(Max)를 스캔하여 경계 사각형의 절반 크기를 확정
      var wMax = Mathf.Max(Mathf.Abs(rotatedTopLeftRel.x), Mathf.Abs(rotatedTopRightRel.x));
      var hMax = Mathf.Max(Mathf.Abs(rotatedTopLeftRel.y), Mathf.Abs(rotatedTopRightRel.y));
      
      // 도출된 반지름 개념의 최대 영역 수치에 2배를 곱해 최종 전체 바운딩 박스의 [가로 폭, 세로 높이] 튜플 데이터로 반환
      return (2 * wMax, 2 * hMax);
    }
  }
}