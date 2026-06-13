// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  // 다양한 형식의 입력 미디어 소스(웹캠, 정적 이미지, 비디오 파일)를 
  // 단일 인터페이스 창구로 통합 제어하는 정적(Static) 프로바이더 클래스
  public static class ImageSourceProvider
  {
    private static WebCamSource _WebCamSource; // 백엔드에 캐싱된 실시간 하드웨어 웹캠 입력 객체
    private static StaticImageSource _StaticImageSource; // 백엔드에 캐싱된 디버깅용 정적 텍스처 이미지 객체
    private static VideoSource _VideoSource; // 백엔드에 캐싱된 녹화 비디오 클립 입력 객체

    /*주석 추가함*/
    // 외부 연산 그래프(GraphRunner 등)가 공용으로 참조하는 최종 활성화된 통합 미디어 소스 인스턴스 프로퍼티
    public static ImageSource ImageSource { get; private set; } 

    /*주석 추가함*/
    // 현재 활성화되어 스트리밍 중인 미디어 소스의 구체적인 데이터 타입을 역추적하여 반환하는 런타임 프로퍼티
    public static ImageSourceType CurrentSourceType
    {
      get
      {
        if (ImageSource is WebCamSource)
        {
          return ImageSourceType.WebCamera;
        }
        if (ImageSource is StaticImageSource)
        {
          return ImageSourceType.Image;
        }
        if (ImageSource is VideoSource)
        {
          return ImageSourceType.Video;
        }
        return ImageSourceType.Unknown;
      }
    }

    /*주석 추가함*/
    // 아까 분석한 Bootstrap 시스템 초기화 단계에서 
    // 생성된 3가지 하드웨어/미디어 소스 객체들을 전달받아 전역 등록하는 메서드
    internal static void Initialize(WebCamSource webCamSource, StaticImageSource staticImageSource, VideoSource videoSource)
    {
      _WebCamSource = webCamSource;
      _StaticImageSource = staticImageSource;
      _VideoSource = videoSource;
    }
    /*주석 추가함*/
    // 런타임 또는 유니티 에디터 상에서 입력 소스 타입(웹캠 <-> 비디오 등)을 
    // 실시간으로 교체(Switch) 매핑해 주는 메서드
    public static void Switch(ImageSourceType imageSourceType)
    {
      switch (imageSourceType)
      {
        case ImageSourceType.WebCamera:
          {
            ImageSource = _WebCamSource;
            break;
          }
        case ImageSourceType.Image:
          {
            ImageSource = _StaticImageSource;
            break;
          }
        case ImageSourceType.Video:
          {
            ImageSource = _VideoSource;
            break;
          }
        case ImageSourceType.Unknown:
        default:
          {
            throw new System.ArgumentException($"Unsupported source type: {imageSourceType}");
          }
      }
    }
  }
}
