// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using UnityEngine;

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  // 특정 GraphRunner 계열을 상속받아 
  // AI 비주얼 솔루션의 실행 룹과 제어 프레임워크를 연동하는 제네릭(Generic) 추상 마스터 클래스
  public abstract class LegacySolutionRunner<TGraphRunner> : BaseRunner where TGraphRunner : GraphRunner
  {
    [SerializeField] protected Screen screen; // 연산된 AI 결과물(관절 포인트 등)을 화면에 렌더링하는 출력 스크린 객체
    [SerializeField] protected TGraphRunner graphRunner; // 상속 클래스에 지정된 특정 AI 연산 그래프 인스턴스

    private Coroutine _coroutine; // 메인 AI 연산 및 추론 루프를 관리하는 코루틴 레퍼런스

    public RunningMode runningMode; // 동기/비동기 그래프 작동 모드 플래그

    /*주석 추가함*/
    // GraphRunner의 제한 시간 프로퍼티에 접근하여 밀리초 단위 세팅을 포워딩하는 래퍼 프로퍼티
    public long timeoutMillisec 
    {
      get => graphRunner.timeoutMillisec;
      set => graphRunner.timeoutMillisec = value;
    }

    /*주석 추가함*/
    // 기존 구동 중인 코루틴이 있다면 안전하게 차단 후, 
    // 부모 클래스의 가동 로직 및 메인 Run() 코루틴 루프를 신규 실행
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
    // 콘텐츠 상태를 일시정지로 전환함
    public override void Pause()
    {
      base.Pause();
      ImageSourceProvider.ImageSource.Pause();
    }

    /*주석 추가함*/
    // 콘텐츠를 다시 가동 상태로 전환함
    public override void Resume()
    {
      base.Resume();
      var _ = StartCoroutine(ImageSourceProvider.ImageSource.Resume());
    }

    /*주석 추가함*/
    // 실행 중인 메인 AI 루프 코루틴을 강제 해제하고, 
    // 카메라 입력 장치 및 백엔드 AI 연산 그래프 채널을 폐쇄 및 중지(Stop)
    public override void Stop()
    {
      base.Stop();
      StopCoroutine(_coroutine);
      ImageSourceProvider.ImageSource.Stop();
      graphRunner.Stop();
    }

    /*주석 추가함*/
    // 자식 솔루션(예: 손 인식 실제 구동부)에서 입력 프레임을 전달받아 
    // 무한 추론 연산을 수행할 핵심 루프 추상 메서드
    protected abstract IEnumerator Run();

    /*주석 추가함*/
    // 화면에 그려질 AI 시각화 레이어(Annotation)의 좌우 반전 상태 및 카메라 회전각 각도를 
    // 하드웨어 사양(전면/후면 카메라, 텍스처 플립 여부)에 맞춰 실시간 동기화 보정하는 정적 메서드
    protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController, ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation
    {
      annotationController.isMirrored = expectedToBeMirrored ^ imageSource.isHorizontallyFlipped ^ imageSource.isFrontFacing;
      annotationController.rotationAngle = imageSource.rotation.Reverse();
    }
  }
}
