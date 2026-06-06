// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections;
using UnityEngine;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mediapipe.Unity
{
  /*주석 추가함*/
  // 비동기 코루틴 연산의 종료를 대기하고, 
  // 연산 결과 추출 및 타임아웃 예외 처리를 지원하는 커스텀 대기(CustomYieldInstruction) 클래스
  public class WaitForResult : CustomYieldInstruction
  {
    /*주석 추가함*/
    // 비동기 작업이 최종 완료된 후 반환된 원본 결과 객체 프로퍼티
    public object result { get; private set; }


    protected object tmpResult; // 연산 과정 중 실시간으로 인출되는 임시 결과물 보관 변수
    protected bool isDone = false; // 타겟 코루틴의 전반적인 모든 루프 연산이 성공적으로 완결되었는지 나타내는 플래그

    private readonly MonoBehaviour _runner; // 타임아웃 시 코루틴을 강제 해제하기 위한 주체 컴포넌트(러너) 레퍼런스
    private readonly IEnumerator _inner; // 실제 백엔드 단에서 대기 및 수행해야 할 타겟 비동기 로직
    private readonly Coroutine _coroutine; // 본 클래스가 래핑하여 제어하는 실제 유니티 코루틴 인스턴스

    public bool isError { get; private set; } = false; // 비동기 대기 작동 중 치명적인 예외(Error)가 발생했는지 판별하는 플래그
    public Exception error { get; private set; } // 발생한 예외의 원인과 스택 트레이스 정보를 담는 예외 객체 프로퍼티
    
    /*주석 추가함*/
    // 유니티 코루틴 엔진이 루프를 계속 대기(Hold)해야 하는지 판정하는 오버라이드 프로퍼티입니다. (완료되거나 에러 발생 시 대기 해제)
    public override bool keepWaiting => !isDone && !isError;

    /*주석 추가함*/
    // 생성자: 대기를 실행할 주체 컴포넌트와 타겟 로직, 그리고 최대 대기 허용 시간(밀리초)을 받아 강제 구동을함.
    public WaitForResult(MonoBehaviour runner, IEnumerator inner, long timeoutMillisec = long.MaxValue)
    {
      _runner = runner;
      _inner = inner;
      _coroutine = runner.StartCoroutine(Run(timeoutMillisec)); // 타임아웃 감시용 루프 코루틴 구동
    }

    /*주석 추가함*/
    // 타겟 코루틴의 내부 루프를 한 단계씩 수동 구동(`MoveNext`)하며 
    // 지연 시간 초과 및 예외 발생 여부를 실시간 크롤링
    private IEnumerator Run(long timeoutMillisec)
    {
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      while (true)
      {
        try
        {
          // 허용된 임계 타임아웃 시간을 초과한 경우 시스템 크래시를 방지하기 위해 연산을 강제 셧다운(Shutdown)
          if (stopwatch.ElapsedMilliseconds > timeoutMillisec)
          {
            _runner.StopCoroutine(_coroutine);
            throw new TimeoutException($"{stopwatch.ElapsedMilliseconds}ms has passed");
          }
          // 타겟 코루틴 로직의 다음 스텝 연산이 더 이상 존재하지 않는다면(종료 시) 무한 루프 탈출
          if (!_inner.MoveNext())
          {
            break;
          }
          tmpResult = _inner.Current; // 현재 프레임의 연산 출력물 저장
        }
        catch (Exception e) // 네이티브 라이브러리 세팅 등에서 유발된 예외를 캡처하여 유니티 엔진 다운을 방지
        {
          isError = true;
          error = e;
          yield break;
        }
        yield return tmpResult; // 유니티 메인 루프에 임시 결과 포워딩 및 대기 유지
      }
      Done(tmpResult); // 정상 완결 시 결과물 확정 등록 처리
    }

    /*주석 추가함*/
    // 비동기 시퀀스가 완결되었을 때 상태 플래그를 전환하고 최종 결과 데이터를 바인딩하는 가상 가드 메서드
    protected virtual void Done(object result) 
    {
      this.result = result;
      isDone = true;
    }
  }

  /*주석 추가함*/
  // 데이터의 형변환(Casting) 에러를 원천 차단하기 위해 
  // 강한 타입 검증을 지원하는 제네릭(Generic) WaitForResult 확장 클래스
  public class WaitForResult<T> : WaitForResult
  {
    /*주석 추가함*/
    // 명시적인 타겟 데이터 형식(`T`)으로 형변환된 비동기 완결 결과물 프로퍼티
    public new T result { get; private set; }


    public WaitForResult(MonoBehaviour runner, IEnumerator inner, long timeoutMillisec = long.MaxValue) : base(runner, inner, timeoutMillisec) { }

    /*주석 추가함*/
    // 부모 클래스의 완료 단계를 재정의하여 제네릭 데이터 타입으로 언박싱(Unboxing) 후 결과를 안전하게 저장
    protected override void Done(object result)
    {
      this.result = (T)result;
      isDone = true;
    }
  }
}
