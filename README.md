# 에셋 번들 패치 시스템

## 프로젝트 정보
  - 유니티 버전 : 20.3.14f1(LTS)
  - 사용한 웹서버 : 구글드라이버

## 에셋번들 파일 만드는 법
추후 블로그 포스팅 예정

## 에셋 번들 패치 로직 설명
  1. 로컬에 버전 파일이 있는지 검사
  2. 서버에 버전 파일을 다운받아 필요 값 대입 & 큐에 삽입  
  3. 패치 코루틴 실행
  4. 큐에 넣었던 인덱스를 빼서 다운로드 또는 캐시 로드 진행
  5. 다운로드 또는 캐시 로드 완료 시 에셋 번들 데이터를 빼와서 셋팅

  > **로컬에 파일이 있을시**<br>
  로컬과 서버 버전을 비교 한 후 다르면 서버 버전 파일 저장

## 주요 함수
UnityWebRequestAssetBundle.GetAssetBundle(...)<br>
DownloadHandlerAssetBundle.GetContent(...)

## 스크립트 설명
AssetBundleManager.cs
에셋 번들 파일을 다운로드 또는 캐싱 로드

DataContainer.cs
다운로드 또는 로드를 완료한 에셋 번들의 데이터를 저장

ContentsManager.cs
DataContainer에 저장되어 있는 데이터를 불러와 실제로 보여주도록 함
## 시연 영상
[![에셋 번들 시연]( https://img.youtube.com/vi/ByQAii2x5mI/0.jpg)](https://youtu.be/ByQAii2x5mI)
