using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

// 카메라의 위치/회전/FOV를 하나의 프리셋으로 묶은 데이터 컨테이너
[Serializable]
public class CameraSetting {
    // 카메라 위치
    public Vector3 position;
    // 카메라 회전(Euler 각)
    public Vector3 rotation;
    // 카메라 시야각
    public float fieldOfView = 60f;

    // 에디터에서 버튼으로 현재 메인 카메라에 이 세팅을 즉시 적용
    [Button]
    public void Set()
    {
        Camera.main.transform.position = position;
        Camera.main.transform.rotation = Quaternion.Euler(rotation);
        Camera.main.fieldOfView = fieldOfView;
    }
}

public class TitleCamera : MonoBehaviour
{
    // 캐시된 Camera 컴포넌트 참조 (FOV 트윈용)
    private Camera cam;

    // 상태별 카메라 세팅 목록 (인스펙터에서 상태→세팅 등록)
    [TableList]
    [SerializeField]
    public List<StateCameraSetting> stateCameraSettings = new List<StateCameraSetting>();
    // 런타임 조회용 상태→세팅 맵 (Awake에서 구성)
    private Dictionary<TitleState, CameraSetting> stateToCameraSetting;

    // 로비 진입 후 스냅될 최종 카메라 세팅 (페이드 아웃 직후 위치/회전/FOV를 즉시 이 값으로 적용)
    [SerializeField] private CameraSetting lobbyCameraSetting;
    // 화면 페이드 인/아웃용 캔버스 그룹 (전체 화면을 덮는 검은 이미지에 부착 권장)
    [SerializeField] private CanvasGroup fadeCanvas;

    private TitleState curState;

    // 카메라 컴포넌트 캐싱 및 상태→세팅 딕셔너리 구성
    private void Awake() {
        cam = GetComponent<Camera>();
        stateToCameraSetting = new Dictionary<TitleState, CameraSetting>();
        foreach (var item in stateCameraSettings) {
            stateToCameraSetting[item.state] = item.setting;
        }
    }

    // 외부에서 상태 전환을 요청할 때 진입점
    // - 현재 상태가 InLobby이면: 로비에서 나가는 전환(OutLobbyTransition)
    // - 새 상태가 InLobby이면: 로비로 들어가는 전환(LobbyTransition)
    // - 그 외: 일반 세팅 트윈(ApplySetting)
    public void SetState(TitleState state, float duration) {
        if (stateToCameraSetting != null && stateToCameraSetting.TryGetValue(state, out var setting)) {
            if (curState == TitleState.InLobby)
                OutLobbyTransition(setting, duration);
            else if (state == TitleState.InLobby)
                LobbyTransition(setting, duration);
            else
                ApplySetting(setting, duration);
        }
        curState = state;
    }
    
    // 일반 상태 전환: 위치/회전/FOV를 주어진 시간 동안 트윈
    public void ApplySetting(CameraSetting setting, float duration) {
        transform.DOMove(setting.position, duration).SetEase(Ease.OutSine);
        transform.DORotate(setting.rotation, duration).SetEase(Ease.InSine);
        if (cam != null) {
            cam.DOFieldOfView(setting.fieldOfView, duration).SetEase(Ease.OutSine);
        }
    }
    
    // 로비로 들어가는 전환
    // 1) 중간 세팅으로 이동/회전/FOV 트윈과 동시에 페이드 아웃
    // 2) 트윈 완료 콜백에서 즉시 로비 세팅으로 스냅
    // 3) 이후 페이드 인
    private void LobbyTransition(CameraSetting setting, float seqADuration, float seqBDuration = 0.5f)
    {
        var sequence = DOTween.Sequence();

        sequence.Join(transform.DOMove(setting.position, seqADuration).SetEase(Ease.OutSine));
        sequence.Join(transform.DORotate(setting.rotation, seqADuration).SetEase(Ease.InSine));
        if (cam != null)
            sequence.Join(cam.DOFieldOfView(setting.fieldOfView, seqADuration).SetEase(Ease.OutSine));
        
        if (fadeCanvas != null)
            sequence.Join(fadeCanvas.DOFade(1f, seqADuration));

        sequence.AppendCallback(() =>
        {
            transform.position = lobbyCameraSetting.position;
            transform.rotation = Quaternion.Euler(lobbyCameraSetting.rotation);
            if (cam != null)
            {
                cam.fieldOfView = lobbyCameraSetting.fieldOfView;
            }
        });

        if (fadeCanvas != null)
        {
            sequence.Append(fadeCanvas.DOFade(0f, seqBDuration));
        }
    }
    
    // 로비에서 나가는 전환
    // 1) 반 시간 동안 페이드 아웃
    // 2) 목표 세팅으로 즉시 스냅
    // 3) 나머지 반 시간 동안 페이드 인
    private void OutLobbyTransition(CameraSetting setting, float duration)
    {
        var sequence = DOTween.Sequence();
        
        if (fadeCanvas != null)
            sequence.Join(fadeCanvas.DOFade(1f, duration/2));
        sequence.AppendCallback(() =>
        {
            transform.position = setting.position;
            transform.rotation = Quaternion.Euler(setting.rotation);
            if (cam != null)
            {
                cam.fieldOfView = setting.fieldOfView;
            }
        });
        
        if (fadeCanvas != null)
        {
            sequence.Append(fadeCanvas.DOFade(0f, duration/2));
        }
    }
}

[Serializable]
public class StateCameraSetting {
    public TitleState state;
    public CameraSetting setting = new CameraSetting();
}
