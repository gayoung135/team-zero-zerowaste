// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using UnityEngine.UI;

namespace Mediapipe.Unity
{
  /*주석 추가함*/
  // 유니티 UI 시스템(RawImage)을 기반으로 
  // 입력 이미지 소스를 화면에 올바른 해상도와 방향으로 출력하는 디스플레이 제어 클래스
  public class Screen : MonoBehaviour
  {
    [SerializeField] private RawImage _screen; // 텍스처를 출력할 유니티 UI 컴포넌트

    private ImageSource _imageSource; // 현재 연동 중인 비주얼 미디어 공급원 레퍼런스

    /*주석 추가함*/
    // UI 스크린 컴포넌트의 출력 텍스처 데이터 프로퍼티
    public Texture texture
    {
      get => _screen.texture;
      set => _screen.texture = value;
    }

    /*주석 추가함*/
    // 텍스처의 렌더링 영역 및 반전 상태를 정의하는 UV 래핑 좌표계 프로퍼티
    public UnityEngine.Rect uvRect
    {
      set => _screen.uvRect = value;
    }

    /*주석 추가함*/
    // 미디어 소스 사양(해상도, 회전 각도, 반전 여부)에 맞춰 UI 스크린의 기본 트랜스폼 구조를 빌드하는 초기화 메서드
    public void Initialize(ImageSource imageSource)
    {
      _imageSource = imageSource;

      Resize(_imageSource.textureWidth, _imageSource.textureHeight); // 미디어 소스 해상도에 맞춰 UI 크기 동적 조절
      Rotate(_imageSource.rotation.Reverse()); // 소스 고유 회전각의 역방향으로 UI 각도 보정
      ResetUvRect(RunningMode.Async); // 비동기 모드 기준의 UV 오프셋 반전 세팅
      texture = imageSource.GetCurrentTexture(); // 소스의 현재 프레임 텍스처 바인딩
    }

    /*주석 추가함*/
    // 입력 영상의 픽셀 해상도 규격에 맞춰 UI 출력 캔버스의 가로/세로 영역(`sizeDelta`)을 리사이징
    public void Resize(int width, int height)
    {
      _screen.rectTransform.sizeDelta = new Vector2(width, height);
    }

    /*주석 추가함*/
    // 디바이스 회전 상태에 따라 UI 캔버스의 오일러 회전각(`localEulerAngles`)을 정밀 매칭
    public void Rotate(RotationAngle rotationAngle)
    {
      _screen.rectTransform.localEulerAngles = rotationAngle.GetEulerAngles();
    }

    /*주석 추가함*/
    // 동기화 파이프라인에서 전달받은 원본 텍스처 프레임 데이터를 복사하여 화면에 다이렉트로 업데이트하는 메서드
    public void ReadSync(Experimental.TextureFrame textureFrame)
    {
      if (!(texture is Texture2D)) // 일반 텍스처인 경우 복사 연산이 가능한 Texture2D 포맷으로 동적 재생성
      {
        texture = new Texture2D(_imageSource.textureWidth, _imageSource.textureHeight, TextureFormat.RGBA32, false);
        ResetUvRect(RunningMode.Sync); // 동기화 모드에 최적화된 UV 오프셋으로 재연산
      }
      textureFrame.CopyTexture(texture); // 픽셀 버퍼 고속 복사 실행
    }

    /*주석 추가함*/
    // 파이프라인 구동 방식(동기/비동기) 및 카메라 렌더링 상태(전면/후면 카메라 여부)를 판별하여 
    // 화면 왜곡을 방지하기 위한 UV 매핑 좌표계를 재정렬
    private void ResetUvRect(RunningMode runningMode)
    {
      var rect = new UnityEngine.Rect(0, 0, 1, 1); // 기본 UV 좌표 공간 선언

      /*주석 추가함*/
      // 비동기 연산 시 플립 처리가 필요 없는 CPU 바인딩 특성을 고려하여, 
      // 하드웨어 수직 반전 상태일 때 픽셀을 상하 반전
      if (_imageSource.isVerticallyFlipped && runningMode == RunningMode.Async)
      {
        // In Async mode, we don't need to flip the screen vertically since the image will be copied on CPU.
        rect = FlipVertically(rect);
      }
      // 모바일 전면 카메라 모드 등 거울 대칭 연산이 필요한 경우, 
      // 회전 매트릭스 각도를 고려하여 하드웨어 좌우/상하 반전 오프셋을 역산 적용
      if (_imageSource.isFrontFacing)
      {
        // Flip the image (not the screen) horizontally.
        // It should be taken into account that the image will be rotated later.
        var rotation = _imageSource.rotation;

        if (rotation == RotationAngle.Rotation0 || rotation == RotationAngle.Rotation180)
        {
          rect = FlipHorizontally(rect);
        }
        else // 90도 또는 270도 회전 상태일 경우 축 변환에 의해 상하를 뒤집어야 좌우 반전 효과가 연출
        {
          rect = FlipVertically(rect);
        }
      }

      uvRect = rect; // 최종 계산된 보정 렉트 행렬을 UI 화면에 반영
    }

    /*주석 추가함*/
    // 텍스처 픽셀 공간을 가로 축 기준으로 미러링(좌우 반전) 매핑하는 좌표 변환 메서드
    private UnityEngine.Rect FlipHorizontally(UnityEngine.Rect rect)
    {
      return new UnityEngine.Rect(1 - rect.x, rect.y, -rect.width, rect.height);
    }

    /*주석 추가함*/
    // 텍스처 픽셀 공간을 세로 축 기준으로 미러링(상하 반전) 매핑하는 좌표 변환 메서드
    private UnityEngine.Rect FlipVertically(UnityEngine.Rect rect)
    {
      return new UnityEngine.Rect(rect.x, 1 - rect.y, rect.width, -rect.height);
    }
  }
}
