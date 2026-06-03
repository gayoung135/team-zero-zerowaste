// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using UnityEngine;

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  // MediaPipe 최신 버전의 비전 컴포넌트 태스크 API(BaseVisionTaskApi)군을 
  // 제네릭으로 전달받아 연동하는 고수준 추상 마스터 클래스
  public abstract class VisionTaskApiRunner<TTask> : BaseRunner where TTask : Tasks.Vision.Core.BaseVisionTaskApi
  {
    [SerializeField] protected Screen screen; // 연산된 비전 인식 데이터를 화면에 표출해 주는 출력 디스플레이 스크린 객체

    private Coroutine _coroutine; // 최신 비전 태스크의 실시간 무한 분석 루프를 담당하는 코루틴 레퍼런스
    protected TTask taskApi; // 상속 클래스에서 정의된 실질적인 구체화 비전 인공지능 API 인스턴스

    public RunningMode runningMode; // 동기(Sync) 또는 비동기(Async) 파이프라인 구동 방식을 지정하는 플래그

    /*주석 추가함*/
    // 기존 재생 코루틴이 존재할 경우 리셋 처리 후, 부모의 스톱워치 기동 및 신규 비전 연산 `Run` 루틴을 작동
    public override void Play()
    {
      if (_coroutine != null)
      {
        Stop();
      }
      base.Play();
      _coroutine = StartCoroutine(Run());
    }

    /*주석 추가함*/
    // 콘텐츠 라이프사이클을 일시정지하고, 
    // 백엔드 카메라 소스(ImageSource)의 이미지 프레임 캡처 스트리밍을 대기 상태로 전환
    public override void Pause()
    {
      base.Pause();
      ImageSourceProvider.ImageSource.Pause();
    }

    /*주석 추가함*/
    // 콘텐츠 정지 상태를 소거하고, 대기 중이던 비디오/웹캠 영상 캡처 루틴을 비동기 코루틴으로 다시 활성화
    public override void Resume()
    {
      base.Resume();
      var _ = StartCoroutine(ImageSourceProvider.ImageSource.Resume());
    }

    /*주석 추가함*/
    // 메인 비전 코루틴을 중단하고 카메라 스트리밍을 종료하며, 
    // 네이티브 태스크 인스턴스 소켓을 명시적으로 폐쇄(`Close`) 및 널(Null) 초기화
    public override void Stop()
    {
      base.Stop();
      StopCoroutine(_coroutine);
      ImageSourceProvider.ImageSource.Stop();
      taskApi?.Close();
      taskApi = null;
    }

    /*주석 추가함*/
    // 자식 비전 솔루션(예: 신형 오브젝트 검출 스크립트 등)에서 프레임을 읽어와 
    // 실시간 추론을 반복 수행할 메인 코루틴 추상 메서드
    protected abstract IEnumerator Run();

    /*주석 추가함*/
    // 화면에 오버레이되는 AI 시각화 레이어(Annotation) 컴포넌트에 
    // 현재 매핑된 카메라 이미지 소스의 원본 해상도 규격과 좌우 반전 파라미터를 동기화 바인딩하는 정적 메서드
    protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController, ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation
    {
      annotationController.isMirrored = expectedToBeMirrored;
      annotationController.imageSize = new Vector2Int(imageSource.textureWidth, imageSource.textureHeight);
    }
  }
}
