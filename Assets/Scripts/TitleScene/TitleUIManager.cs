using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public enum TitleState {
    Title,
    CreateLobby,
    FindLobby,
    InLobby,
    Options,
}

[Serializable]
public class StateUIPair {
    public TitleState state;
    public GameObject ui;
}

public class TitleUIManager : MonoBehaviour {
    // 현재 타이틀 UI의 상태. Odin Inspector의 EnumToggleButtons로 인스펙터에서 상태 전환을 테스트하기 위함
    // 상태 전환 시 카메라 연출(이동/회전/페이드 등)을 담당하는 컴포넌트 참조
    [EnumToggleButtons] private TitleState state;
    // 상태별로 표시할 UI 패널 매핑 (인스펙터에서 상태→UI를 등록)
    [SerializeField] TitleCamera titleCamera;
    [SerializeField] List<StateUIPair> stateUIList;
    // UI 표시/숨김 애니메이션 총 소요 시간 (DOTween)
    [SerializeField] private float uiTransitionDuration = 0.6f;

    // 런타임에서 빠른 조회를 위한 상태→UI 딕셔너리 (Awake에서 stateUIList 기반으로 구성)
    private Dictionary<TitleState, GameObject> uiByState;

    private void Awake()
    {
        // 인스펙터에 등록된 stateUIList를 딕셔너리로 변환하여 상태별 UI를 빠르게 찾을 수 있도록 준비
        uiByState = new Dictionary<TitleState, GameObject>();
        foreach (var pair in stateUIList)
        {
            if (pair != null && pair.ui != null)
            {
                uiByState[pair.state] = pair.ui;
            }
        }
    }

    private void Start()
    {
        /* 시작 상태를 Title로 고정하고 카메라를 해당 상태로 세팅
           모든 UI를 비활성화하고 Y스케일을 0으로 만들어 감춰둔 뒤, 초기 상태의 UI만 표시 */
        state = TitleState.Title;
        titleCamera.SetState(state, 0);

        foreach (var ui in uiByState.Values)
        {
            ui.SetActive(false);
            Vector3 scale = ui.transform.localScale;
            scale.y = 0;
            ui.transform.localScale = scale;
        }

        if (uiByState.TryGetValue(state, out var initialUI))
        {
            ShowUI(initialUI);
        }
    }
    
    public void ToTitle() => ChangeState(TitleState.Title);
    public void ToCreateLobby() => ChangeState(TitleState.CreateLobby);
    public void ToFindLobby() => ChangeState(TitleState.FindLobby);
    public void ToInLobby() => ChangeState(TitleState.InLobby);
    public void ToOptions() => ChangeState(TitleState.Options);
    public void QuitGame() => Application.Quit();

    // 상태 전환 엔트리 포인트
    // 1) 동일 상태면 무시
    // 2) 카메라 연출 호출
    // 3) 현재 켜져있는 모든 UI를 숨김 애니메이션 → 모두 끝난 뒤 대상 UI를 표시
    [Button]
    void ChangeState(TitleState newState)
    {
        if (state == newState) return;

        state = newState;

        if (titleCamera != null) {
            titleCamera.SetState(newState, uiTransitionDuration);
        }

        GameObject targetUI = null;
        uiByState.TryGetValue(newState, out targetUI);

        // 현재 활성화되어 있는(켜진) UI들을 모두 수집
        List<GameObject> activeUIs = new List<GameObject>();
        foreach (var kvp in uiByState)
        {
            if (kvp.Value.activeSelf)
            {
                activeUIs.Add(kvp.Value);
            }
        }

        if (activeUIs.Count == 1 && activeUIs[0] == targetUI)
            return;

        int hideCount = activeUIs.Count;
        if (hideCount == 0)
        {
            if (targetUI != null)
                ShowUI(targetUI);
            return;
        }

        // 모든 활성 UI를 순차가 아닌 병렬로 숨김 처리. 각 숨김이 끝날 때마다 카운트를 줄여,
        // 마지막 하나가 끝났을 때 목표 UI를 표시
        foreach (var ui in activeUIs)
        {
            HideUI(ui, () =>
            {
                hideCount--;
                if (hideCount == 0)
                {
                    if (targetUI != null)
                        ShowUI(targetUI);
                }
            });
        }
    }

    // UI 표시: 활성화 후 Y스케일 0→1로 트윈하여 펼쳐지는 연출
    void ShowUI(GameObject ui)
    {
        ui.SetActive(true);
        Vector3 scale = ui.transform.localScale;
        scale.y = 0;
        ui.transform.localScale = scale;
        ui.transform.DOScaleY(1f, uiTransitionDuration/2).SetEase(Ease.OutBack);
    }

    // UI 숨김: Y스케일 1→0으로 트윈 후 비활성화. 완료 콜백으로 다음 작업(표시 시작) 트리거
    void HideUI(GameObject ui, Action onComplete = null)
    {
        ui.transform.DOScaleY(0f, uiTransitionDuration/2).SetEase(Ease.InBack).OnComplete(() =>
        {
            ui.SetActive(false);
            onComplete?.Invoke();
        });
    }
}
