// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using UnityEngine;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  // 모든 AI 미디어 파이프라인 러너들의 생명주기 및 초기화 시퀀스를 제어하는 최상위 추상(Abstract) 클래스
  public abstract class BaseRunner : MonoBehaviour
  {
    private static readonly string _BootstrapName = nameof(Bootstrap); // 싱글톤으로 관리될 부트스트랩 인스턴스의 고유 식별 명칭임.

    [SerializeField] private GameObject _bootstrapPrefab; // 씬 내에 부트스트랩이 누락되었을 경우 실시간 동적 생성을 위한 프리팹 자원임.

#pragma warning disable IDE1006
    // TODO: make it static
    protected virtual string TAG => GetType().Name; // 하위 구현 클래스 식별 및 디버깅 출력을 위한 추적 태그 프로퍼티
#pragma warning restore IDE1006

    protected Bootstrap bootstrap; // 시스템 전반의 초기화를 담당하는 핵심 부트스트랩 모듈 객체 레퍼런스 
    protected bool isPaused; // 현재 콘텐츠 파이프라인의 실시간 일시정지 상태 여부를 판별하는 플래그

    private readonly Stopwatch _stopwatch = new(); // 런타임 추론 프레임의 정밀 타임스탬프 생성을 위한 고해상도 하드웨어 타이머 시스템

    /*주석 추가함*/
    // 씬 진입 시 부트스트랩 모듈을 탐색하고, 
    // 네이티브 AI 백엔드 코어 인프라 세팅이 완료될 때까지 메인 연산을 안전하게 대기(Wait)
    protected virtual IEnumerator Start() 
    {
      bootstrap = FindBootstrap(); // 씬 내 부트스트랩 서칭 및 예외 생성 실행
      yield return new WaitUntil(() => bootstrap.isFinished); // 부트스트랩 초기화 시퀀스가 전면 완결될 때까지 대기 처리

      Play(); // 세팅 완료 후 메인 루프 가동
    }

    /// <summary>
    ///   Start the main program from the beginning. (시작화면으로 부터 메인프로그램 시작함.)
    /// </summary>
    public virtual void Play()
    {
      isPaused = false;
      _stopwatch.Restart();
    }

    /// <summary>
    ///   Pause the main program. (프로그램이 일시정지됨.)
    /// <summary>
    public virtual void Pause()
    {
      isPaused = true;
    }

    /// <summary>
    ///    Resume the main program. (프로그램이 다시 재생됨.)
    ///    If the main program has not begun, it'll do nothing. 
    ///    (만일 프로그램이 시작하지 않는다면, 아무일도 안일어남.)
    /// </summary>
    public virtual void Resume()
    {
      isPaused = false;
    }

    /// <summary>
    ///   Stops the main program.
    ///   (프로그램 멈춤.)
    /// </summary>
    public virtual void Stop()
    {
      isPaused = true;
      _stopwatch.Stop();
    }

    /*주석 추가함*/
    // 현재 스톱워치 인프라에서 연산된 밀리초(Millisec) 단위를 기반으로 
    // 유일한 데이터 프레임 식별용 타임스탬프를 리턴
    protected long GetCurrentTimestampMillisec() => _stopwatch.IsRunning ? _stopwatch.ElapsedTicks / TimeSpan.TicksPerMillisecond : -1;

    /*주석 추가함*/
    // 현재 유니티 가동 씬 내에서 'Bootstrap' 오브젝트를 탐색하고, 
    // 부재 시 하이어라키 상에 메모리 생존 조건(`DontDestroyOnLoad`)을 걸어 동적 생성(Instantiate)하는 
    // 핵심 아키텍처 가드 메서드
    protected Bootstrap FindBootstrap()
    {
      var bootstrapObj = GameObject.Find("Bootstrap");

      if (bootstrapObj == null)  // 첫 씬 구동이나 누락 시 예외 초기화 처리
      {
        Debug.Log("Initializing the Bootstrap GameObject");
        bootstrapObj = Instantiate(_bootstrapPrefab);
        bootstrapObj.name = _BootstrapName;
        DontDestroyOnLoad(bootstrapObj); // 씬이 전환되어도 초기화 데이터 및 세팅 소실 방지
      }

      return bootstrapObj.GetComponent<Bootstrap>(); // 부트스트랩 컴포넌트 인스턴스 반환 포워딩
    }
  }
}
