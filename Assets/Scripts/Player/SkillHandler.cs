using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillHandler : MonoBehaviour
{
    private HeroState heroState;
    private ISkill[] skillSlots = new ISkill[4];

    [SerializeField] private GameObject skillPrefab; // 스킬 프리팹 (필요시 사용)
    [SerializeField] private LayerMask _groundMask = ~0;     // 지면 레이어 마스크
    [SerializeField] private float _raycastDistance = 2000f; // 레이캐스트 최대 거리

    private Transform _mouseTarget; // 재사용용 마우스 타깃 트랜스폼

    public void SetSkill(int skillIndex, ISkill skill)
    {
        skillSlots[skillIndex] = skill;
    }
    private void Awake()
    {
        if (!TryGetComponent(out heroState))
        {
            this.gameObject.AddComponent<HeroState>();
            heroState = GetComponent<HeroState>();
        }
        skillSlots = skillPrefab.GetComponents<ISkill>();

        // 재사용할 빈 트랜스폼 생성(씬 오염 방지용)
        var go = new GameObject("MouseTarget(Reused)");
        go.hideFlags = HideFlags.HideInHierarchy;
        _mouseTarget = go.transform;
    }
    void Update()
    {

    }

    private Transform GetMouseWorldPoint()
    {
        if (heroState == null || heroState.cam == null) return null;

        Ray ray = heroState.cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _groundMask, QueryTriggerInteraction.Ignore))
        {
            _mouseTarget.position = hit.point;
            _mouseTarget.rotation = Quaternion.identity; // 필요 시 정렬
            return _mouseTarget;
        }
        return null;
    }

    #region Input Actions
    private void OnPressSkill1(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[0] != null) skillSlots[0].Press(this.gameObject, target);
    }
    private void OnReleaseSkill1(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[0] != null) skillSlots[0].Release(this.gameObject, target);
    }
    private void OnPressSkill2(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[1] != null) skillSlots[1].Press(this.gameObject, target);
    }
    private void OnReleaseSkill2(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[1] != null) skillSlots[1].Release(this.gameObject, target);
    }
    private void OnPressSkill3(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[2] != null) skillSlots[2].Press(this.gameObject, target);
    }
    private void OnReleaseSkill3(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[2] != null) skillSlots[2].Release(this.gameObject, target);
    }
    private void OnPressSkill4(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[3] != null) skillSlots[3].Press(this.gameObject, target);
    }
    private void OnReleaseSkill4(InputValue value)
    {
        var target = GetMouseWorldPoint();
        if (skillSlots[3] != null) skillSlots[3].Release(this.gameObject, target);
    }
    #endregion
}
