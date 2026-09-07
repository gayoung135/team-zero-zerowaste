// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;

namespace Mediapipe.Unity.Sample
{
  /*주석 추가함*/
  //AI 손 인식에 필요한 핵심 리소스(학습 모델 및 에셋 파일)를 비동기로 관리하고
  //불러오는 작업을 대행해 주는 정적(Static) 헬퍼 클래스
  public static class AssetLoader
  {
    /*주석 추가함*/
    // 실제 에셋 로딩을 담당할 리소스 매니저(객체)를 주입받아 등록하는 메서드
    // 이 메서드를 통해 매니저가 지정되어야만 실질적인 에셋 준비 로직이 작동함.
    private static IResourceManager _ResourceManager;

    /*주석 추가함*/
    //실제 에셋 로딩을 담당할 리소스 매니저(객체)를 주입받아 등록하는 메서드
    //이 메서드를 통해 매니저가 지정되어야만 실질적인 에셋 준비 로직이 작동
    public static void Provide(IResourceManager manager) => _ResourceManager = manager;

    /*주석 추가함*/
    //지정된 이름과 고유 키를 기반으로 에셋 파일을 불러오도록
    //리소스 매니저에게 요청하는 핵심 비동기 메서드
    public static IEnumerator PrepareAssetAsync(string name, string uniqueKey, bool overwrite = false)
    {
      if (_ResourceManager == null)
      {
        throw new System.InvalidOperationException("ResourceManager is not provided");
      }
      return _ResourceManager.PrepareAssetAsync(name, uniqueKey, overwrite);
    }

    /*주석 추가함*/
    //에셋 파일 이름과 고유 식별 키를 동일하게 사용하여
    //간편하게 에셋을 로드할 수 있도록 도와주는 오버로딩(Overloading) 메서드
    public static IEnumerator PrepareAssetAsync(string name, bool overwrite = false) => PrepareAssetAsync(name, name, overwrite);
  }
}
