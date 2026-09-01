// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using UnityEngine;
using UnityEngine.Video;

namespace Mediapipe.Unity.Sample
{
  [Serializable]
  public class AppSettings : ScriptableObject
  {
    [Serializable]
    public enum AssetLoaderType
    {
      StreamingAssets,
      AssetBundle,
      Local,
    }

    [SerializeField] private ImageSourceType _defaultImageSource;
    [SerializeField] private InferenceMode _preferableInferenceMode;
    [SerializeField] private AssetLoaderType _assetLoaderType;
    [SerializeField] private Logger.LogLevel _logLevel = Logger.LogLevel.Debug;

    [Header("Glog settings")]
    [SerializeField] private int _glogMinloglevel = Glog.Minloglevel;
    [SerializeField] private int _glogStderrthreshold = Glog.Stderrthreshold;
    [SerializeField] private int _glogV = Glog.V;

    [Header("WebCam Source")]
    [Tooltip("For the default resolution, the one whose width is closest to this value will be chosen")]

    [SerializeField] private int _preferredDefaultWebCamWidth = 640;
    [SerializeField] private ImageSource.ResolutionStruct[] _defaultAvailableWebCamResolutions = new ImageSource.ResolutionStruct[] {
      new ImageSource.ResolutionStruct(424, 240, 30),
      new ImageSource.ResolutionStruct(640, 360, 30),
      new ImageSource.ResolutionStruct(848, 480, 30),
      new ImageSource.ResolutionStruct(960, 540, 30),
      new ImageSource.ResolutionStruct(1280, 720, 30),
      new ImageSource.ResolutionStruct(1600, 896, 30),
      new ImageSource.ResolutionStruct(1920, 1080, 30),
    };

    [Header("Static Image Source")]
    [SerializeField] private Texture[] _availableStaticImageSources;
    [SerializeField] private ImageSource.ResolutionStruct[] _defaultAvailableStaticImageResolutions = new ImageSource.ResolutionStruct[] {
      new ImageSource.ResolutionStruct(512, 512, 0),
      new ImageSource.ResolutionStruct(640, 480, 0),
      new ImageSource.ResolutionStruct(1280, 720, 0),
    };

    [Header("Video Source")]
    [SerializeField] private VideoClip[] _availableVideoSources;

    public ImageSourceType defaultImageSource => _defaultImageSource;
    public InferenceMode preferableInferenceMode => _preferableInferenceMode;
    public AssetLoaderType assetLoaderType
    {
      get => _assetLoaderType;
      set => _assetLoaderType = value;
    }
    public Logger.LogLevel logLevel => _logLevel;

    public void ResetGlogFlags()
    {
      /*주석 추가함*/
      //MediaPipe 내부의 Google 로깅 라이브러리(Glog)의 
      //시스템 플래그 및 오류 출력 레벨을 초기화하고 적용하는 메서드
      //디버깅 및 에러 로그 기록의 세부 기준을 재설정하는 역할
      Glog.Logtostderr = true;
      Glog.Minloglevel = _glogMinloglevel;
      Glog.Stderrthreshold = _glogStderrthreshold;
      Glog.V = _glogV;
    }

   /*주석 추가함*/
   //유니티 에디터에 설정된 웹캠 선호 가로 해상도(기본 1280)와 지원 가능한 해상도 목록 데이터들을 기반으로,
   //게임에서 실제로 사용할 'WebCamSource(웹캠 입력 소스 객체)'를 생성하여 반환해 주는 메서드
    public WebCamSource BuildWebCamSource() => new WebCamSource(
      _preferredDefaultWebCamWidth,
      _defaultAvailableWebCamResolutions
    );

    /*주석 추가함*/
    //웹캠 대신 움직이지 않는 '정적 이미지'를 입력값으로 테스트하고 싶을 때 사용하는 메서드
    //등록된 이미지 텍스처와 규격 해상도 데이터를 기반으로 'StaticImageSource' 객체를 빌드하여 반환
    public StaticImageSource BuildStaticImageSource() => new StaticImageSource(
      _availableStaticImageSources,
      _defaultAvailableStaticImageResolutions
    );

  /*주석 추가함*/
  //실시간 카메라 대신 '녹화된 비디오 영상 파일'을 입력소스로 사용하여 테스트하고 싶을 때 사용하는 메서드
  //준비된 비디오 클립 배열 데이터를 기반으로 'VideoSource' 객체를 빌드하여 반환
    public VideoSource BuildVideoSource() => new VideoSource(_availableVideoSources);
  }
}
