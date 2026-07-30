# LCE Micro-swimmer Blood-flow Simulator

Unity `6000.3.21f1`에서 생성·검증한, LCE(liquid crystal elastomer) 꼬리 마이크로스위머의 저레이놀즈수 혈류 시뮬레이터입니다.

## 실행

1. Unity Hub에서 이 폴더를 엽니다.
2. `Assets/Scenes/Main.unity`를 열고 Play를 누릅니다.
3. 왼쪽 패널에서 꼬리/머리 형상과 구동 조건을 바꿉니다.
4. 마우스 오른쪽 드래그로 회전, 휠로 확대/축소, `H`로 패널을 숨깁니다.

빈 Scene에서도 런타임 부트스트랩이 전체 실험 환경을 자동 생성합니다.

## 포함된 모델

- 혈류: 원통 혈관 안의 층류 Poiseuille 속도장 `u(r)=u_max(1-r²/R²)`
- 미세수영체: `Re << 1`에서 관성을 제거하고 매 프레임 힘 평형을 푸는 overdamped Stokes 모델
- 추진: LCE 진행파의 진폭 제곱과 각주파수에 비례하는 resistive-force 기반 축방향 추진 근사
- 꼬리: 물고기 지느러미형 횡파, 회전 나선형, 비틀리는 리본형
- 머리: 구형, 장축형, 디스크형의 방향별 유체저항 계수
- 시각화: 단면 위치에 따라 서로 다른 속도로 흐르는 혈구 및 청색 방향 트레이서
- 실시간 표시: Reynolds 수, 유동 영역, 수영체 속도와 실제 길이

기본 SI 파라미터는 길이 120 µm, 혈액 밀도 1060 kg/m³, 점도 3.5 mPa·s, 중심 유속 2 mm/s입니다. 화면상의 이동 속도는 관찰이 가능하도록 별도의 world-space 배율을 씁니다. Reynolds 수 계산은 SI 값만 사용합니다.

## 확장 위치

- 새 꼬리 형상/변형: `Assets/Scripts/LCETailVisual.cs`의 `Deform`
- 추진 계수: `Assets/Scripts/MicroSwimmerModel.cs`의 `PropulsionCoefficient`
- 머리 저항 텐서: 같은 파일의 `HeadDragMultiplier`
- 혈류 프로파일: `Assets/Scripts/BloodFlowField.cs`의 `VelocityAt`

현재 구현은 형상 탐색과 교육용 실시간 비교를 위한 reduced-order model입니다. 정량적인 생체 내 예측에는 혈관 벽, 비뉴턴 혈액, 적혈구 상호작용과 유체-구조 연성(FSI)을 검증된 FEM/BEM/CFD 솔버로 보정해야 합니다.
# LCE-NANO
# LCE-NANO
