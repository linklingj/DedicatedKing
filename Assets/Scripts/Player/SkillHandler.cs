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
    }
    void Update()
    {

    }

    private Transform GetMouseWorldPoint()
    {
        Ray ray = heroState.cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            GameObject temp = new GameObject("MouseTarget");
            temp.transform.position = hit.point;
            Destroy(temp, 0.1f); // 0.1초 후 임시 오브젝트 삭제
            return temp.transform;
        }
        return null;
    }

    #region Input Actions
    private void OnPressSkill1(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[0].Press(this.gameObject, target);
    }
    private void OnReleaseSkill1(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[0].Release(this.gameObject, target);
    }
    private void OnPressSkill2(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[1].Press(this.gameObject, target);
    }
    private void OnReleaseSkill2(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[1].Release(this.gameObject, target);
    }
    private void OnPressSkill3(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[2].Press(this.gameObject, target);
    }
    private void OnReleaseSkill3(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[2].Release(this.gameObject, target);
    }
    private void OnPressSkill4(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[3].Press(this.gameObject, target);
    }
    private void OnReleaseSkill4(InputValue value)
    {
        var target = GetMouseWorldPoint();
        skillSlots[3].Release(this.gameObject, target);
    }
    #endregion
}
