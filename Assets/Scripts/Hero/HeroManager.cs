using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    public static HeroManager Instance { get; private set; }

    [SerializeField] readonly List<HeroMain> heroes = new List<HeroMain>();

    void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    //HeroMain 등록 (HeroMain.Awake에서 자동 호출)
    public void RegisterHero(HeroMain hero)
    {
        if (!heroes.Contains(hero))
        {
            heroes.Add(hero);
        }
    }
    
    // HeroMain 해제 (OnDestroy에서 자동 호출)
    public void UnregisterHero(HeroMain hero)
    {
        if (heroes.Contains(hero))
        {
            heroes.Remove(hero);
        }
    }
    
    // 전체 Hero 리스트 반환
    public IReadOnlyList<HeroMain> GetAllHeroes()
    {
        return heroes.AsReadOnly();
    }
    
    // 첫 번째 영웅 가져오기 (싱글플레이처럼 쓸 때)
    public HeroMain GetFirstHero()
    {
        return heroes.Count > 0 ? heroes[0] : null;
    }
    
    // 특정 인덱스로 접근
    public HeroMain GetHeroByIndex(int index)
    {
        if (index < 0 || index >= heroes.Count) return null;
        return heroes[index];
    }
    
    // 영웅 수
    public int HeroCount => heroes.Count;
}