# Literature-constrained LCE DDS Microswimmer Simulator

## Proposed geometry and its limits

The maximum-thrust preset is a **40 x 12 um** axially aligned swimmer in a **60 um**
tumor microvessel, with a 0.5 um diameter needle extending 6 um sideways. Per the
design objective, axial length is **not constrained by an in-vessel turning
envelope**. The size is a simulation optimization, not a published device. The needle is deliberately smaller
than the conservative 1 um target opening. Hashizume *et al.* directly observed
inter-endothelial tumor-vessel openings of **0.3--4.7 um**; the UI preserves that
measurement as the admissible opening range rather than implying every tumor has
the same pore size [Am. J. Pathol. 156 (2000) 1363-1380](https://doi.org/10.1016/S0002-9440(10)65006-7).

The reference vessel is 60 um, selectable only by changing the source-backed
scenario data. Tumor microvessels are heterogeneous and tortuous: the geometry is
a design scenario based on direct mammary-tumor microvascular morphometry, not a
universal anatomical number [Less *et al.*, Cancer Research 51 (1991) 265-273](https://pubmed.ncbi.nlm.nih.gov/1988098/).

Blood uses density **1060 kg/m3** and dynamic viscosity **3.5 mPa.s**. The latter is
within the measured high-shear whole-blood plateau in the rheology experiments of
Brust *et al.* [Phys. Rev. Lett. 110 (2013) 078305](https://doi.org/10.1103/PhysRevLett.110.078305).
The default 1 mm/s centreline speed is a literature-scale microvascular scenario,
not a patient-specific measurement. Every displayed Reynolds number uses these SI
values.

Within the UI's finite actuation domain (0.2--12 Hz frequency, 0.05--1.15
amplitude and 0.5--3.5 waves), a cycle scan selects **12 Hz, 1.15 amplitude and
1.6 waves**. The current RFT model predicts a cycle mean of **9.15 um/s** and a
mean thrust magnitude of **5.37 pN** for that preset. This is a bounded numerical
optimum, not a universal maximum: without material, power and deformation limits,
"maximize speed freely" is mathematically unbounded. Press **MAX THRUST (ignore
turning)** to restore the preset after changing controls. Turning clearance is not
calculated or used by the delivery interlock.

### Station keeping is only credible next to the wall

The program does **not** pretend that this swimmer can cancel centreline blood
flow. It solves the Poiseuille profile for the distance from the no-slip wall at
which the predicted swimming speed equals local flow:

`y = R [1 - sqrt(1 - U_swim/U_max)]`.

At typical model output this layer is sub-micrometre. Thus upstream rotation is a
near-wall injection manoeuvre, not free-stream hovering. If the target cannot be
reached inside the displayed hold layer, the proposed injection is infeasible.
Because the body centre remains one radius from the wall, the simulator also
computes the minimum upstream speed there. The autonomous controller **interlocks
needle deployment** whenever predicted propulsion is below that value; it does
not visually fake successful station keeping.
Magnetic upstream orientation in flow has been demonstrated for artificial
bacterial flagella, but this simulation does not invent an unreported LCE steering
torque [Peyer *et al.*, IEEE ICRA (2012) 96-101](https://doi.org/10.1109/ICRA.2012.6225095).

The dome and 18 passive fringes are a hypothesis. Flexible appendages can break
time reversibility through elastic phase lag at low Reynolds number, as measured
in a canonical flexible-oar swimmer [Qian *et al.*, Phys. Rev. E 77 (2008) 036308](https://doi.org/10.1103/PhysRevE.77.036308).
The present fringe count, dome shape, silicone material, needle deployment and
drug dose have **not** been validated in vivo. The UI therefore calls the sequence
Navigate/Turn/StationKeep/Inject rather than presenting it as a clinical controller.

Unity `6000.3.21f1`에서 생성·검증한, LCE(liquid crystal elastomer) 꼬리 마이크로스위머의 저레이놀즈수 혈류 시뮬레이터입니다.

## 실행

1. Unity Hub에서 이 폴더를 엽니다.
2. `Assets/Scenes/Main.unity`를 열고 Play를 누릅니다.
3. 왼쪽 패널에서 구동 조건, 혈류, 전달 단계와 feasibility 지표를 확인합니다.
4. 마우스 오른쪽 드래그로 회전, 휠로 확대/축소, `H`로 패널을 숨깁니다.

`Flow only`, `Swim only`, `Combined` 버튼은 동일한 시작 위치에서 통제 실험을 시작합니다. 기본 카메라는 혈관에 고정된 laboratory frame이므로 절대 이동량을 비교할 수 있습니다. `Follow swimmer`는 꼬리 변형 관찰용이며 이동량 비교에는 적합하지 않습니다.

`Swim only`는 수 µm/s 자체 추진을 짧은 시간에 관찰할 수 있도록 `Motion view gain=100x`를 자동 적용합니다. 이는 위치 표시만 확대하며 UI의 RFT 속도, 추진력, Reynolds 수에는 영향을 주지 않습니다. `Flow only`와 `Combined`는 정량 비교를 위해 자동으로 `1x`를 사용합니다.

### Reynolds number 설정

`Direct Re`를 선택하면 로그 슬라이더로 `10^-5`부터 `1`까지 목표 Reynolds 수를 직접 설정할 수 있습니다. 시뮬레이터는 `Re=ρUL/μ`에서 필요한 점도 `μ=ρUL/Re`를 역산합니다. 혈류가 있을 때는 중심 혈류 속도, `Swim only`에서는 RFT 자체 추진 속도를 특성속도 `U`로 사용합니다. `Manual viscosity`를 선택하면 점도를 직접 조절하고 결과 Reynolds 수를 관찰할 수 있습니다.

빈 Scene에서도 런타임 부트스트랩이 전체 실험 환경을 자동 생성합니다.

## 포함된 모델

- 혈류: 원통 혈관 안의 층류 Poiseuille 속도장 `u(r)=u_max(1-r²/R²)`
- 미세수영체: `Re << 1`에서 관성을 제거하고 매 프레임 힘 평형을 푸는 overdamped Stokes 모델
- 추진: 64-segment resistive-force 기반 축방향 추진 근사
- 꼬리: 왕복 LCE 돔과 탄성 위상지연을 표현하는 수동 fringe 시각화
- DDS: 측면 전개 needle, tumor-vessel opening, 전달 단계 및 station-keeping interlock
- 머리: 구형, 장축형, 디스크형의 방향별 유체저항 계수
- 시각화: 단면 위치에 따라 서로 다른 속도로 흐르는 혈구 및 청색 방향 트레이서
- 실시간 표시: Reynolds 수, 유동 영역, 수영체 속도와 실제 길이

기본 maximum-thrust 파라미터는 길이 40 µm, 직경 12 µm, 혈관 직경 60 µm, 12 Hz, 변형 진폭 1.15, 파수 1.6, 혈액 밀도 1060 kg/m³, 점도 3.5 mPa·s, 중심 유속 1 mm/s입니다. 회전 clearance는 최적화 목적함수와 injection interlock에서 제외됩니다. 화면상의 이동 속도는 관찰이 가능하도록 별도의 world-space 배율을 씁니다. Reynolds 수 계산은 SI 값만 사용합니다.

## 확장 위치

- 돔/fringe 형상과 변형: `Assets/Scripts/LCETailVisual.cs`
- 추진 RFT와 station-keeping 계산: `Assets/Scripts/MicroSwimmerModel.cs`
- 머리 저항 텐서: 같은 파일의 `HeadDragMultiplier`
- 혈류 프로파일: `Assets/Scripts/BloodFlowField.cs`의 `VelocityAt`

현재 구현은 형상 탐색과 교육용 실시간 비교를 위한 reduced-order model입니다. 정량적인 생체 내 예측에는 혈관 벽, 비뉴턴 혈액, 적혈구 상호작용과 유체-구조 연성(FSI)을 검증된 FEM/BEM/CFD 솔버로 보정해야 합니다.

## 추진 물리와 Scallop test

꼬리를 64개 선분으로 이산화하고 각 선분에 국소 저항력 이론을 적용합니다.

`f = -[ξ_perp v + (ξ_parallel-ξ_perp)(v·t)t]`

여기서 `t`는 선분 접선이고 `ξ_parallel`, `ξ_perp`는 가느다란 필라멘트의 접선/수직 저항계수입니다. 매 프레임 다음 무관성 힘 평형을 풀어 혈류 대비 수영 속도 `U`를 얻습니다.

`(ζ_head + ζ_tail) U + F_shape = 0`

`Traveling`은 물고기형 진행파, 나선 회전, 리본 진행파처럼 한 주기 동안 형상 공간에서 닫힌 면적을 만드는 비가역 구동입니다. `Reciprocal`은 한 자유도 변형을 정확히 역순으로 되짚으므로 Stokes 유동의 scallop theorem에 따라 한 주기 평균 자체 추진이 0이 됩니다. `Off`에서는 자체 추진 없이 혈류에 수동 이류됩니다. UI의 순간 속도와 완료된 주기 평균으로 이를 비교할 수 있습니다.
# LCE-NANO
# LCE-NANO
