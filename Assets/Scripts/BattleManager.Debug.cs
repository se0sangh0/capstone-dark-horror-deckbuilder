// ============================================================
// BattleManager.Debug.cs  (#14 디버깅 툴, 2026-06-11)
// 디버그 전용 공개 훅 — DebugToolPanel 에서만 호출한다.
// 에디터/개발 빌드 전용 (릴리즈에서는 컴파일 제외).
// ============================================================
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Linq;
using UnityEngine;

public partial class BattleManager
{
    /// <summary>전투 화면이 활성 상태인지 (디버그 버튼 가드용).</summary>
    public bool DebugInBattle => this != null && isActiveAndEnabled && gameObject.activeInHierarchy;

    /// <summary>적 전멸 + 즉시 승리 판정 (입력 대기 없이 결과 화면 진입).</summary>
    public void DebugKillAllEnemies()
    {
        foreach (var e in enemies)
            if (e != null && !e.isDead) e.CurrentHp = 0;
        Debug.Log("[디버그] 적 전멸 — 즉시 승리 판정");
        DebugForceBattleEnd();
    }

    /// <summary>아군 전멸 + 즉시 패배 판정 (입력 대기 없이 게임오버 진입).</summary>
    public void DebugKillAllAllies()
    {
        foreach (var a in allies)
            if (a != null && !a.isDead) a.CurrentHp = 0;
        Debug.Log("[디버그] 아군 전멸 — 즉시 패배 판정");
        DebugForceBattleEnd();
    }

    /// <summary>
    /// 진행 중인 전투 루프(입력 대기 포함)를 끊고 곧장 전투 종료 판정으로 점프.
    /// 페이즈 전환 때만 판정하는 일반 흐름과 달리 디버그는 즉시 결과를 보여야 한다.
    /// </summary>
    private void DebugForceBattleEnd()
    {
        StopAllCoroutines();   // BattleLoop·연출 코루틴 정지 (디버그 전용 강제 점프)
        currentPhase = BattlePhase.BattleEnd;
        StartCoroutine(HandleBattleEnd());
    }

    /// <summary>아군 전원 풀힐 + 스트레스 0.</summary>
    public void DebugFullHeal()
    {
        foreach (var a in allies)
        {
            if (a == null || a.isDead) continue;
            a.CurrentHp = a.maxHp;
            a.currentStress = 0;
        }
        Debug.Log("[디버그] 아군 풀힐 + 스트레스 0");
    }

    /// <summary>아군 전원이 N번째 스킬을 순차 시전 (스택 소모 없음 — 연출/데미지 파이프라인 테스트).</summary>
    public void DebugCastAllAllySkills(int skillIndex)
    {
        StartCoroutine(DebugCastAllAllySkillsRoutine(skillIndex));
    }

    private IEnumerator DebugCastAllAllySkillsRoutine(int skillIndex)
    {
        foreach (var a in allies.Where(x => x != null && !x.isDead).ToList())
        {
            var skills = a.GetSkills();
            if (skills == null || skillIndex < 0 || skillIndex >= skills.Count || skills[skillIndex] == null) continue;
            yield return StartCoroutine(UseSkill(a, skills[skillIndex]));
            yield return new WaitForSeconds(0.15f);
        }
        Debug.Log($"[디버그] 아군 전원 스킬{skillIndex + 1} 시전 완료");
    }

    /// <summary>적 전원이 N번째 스킬을 실제 시전 — 모션+효과(데미지·소환·순간이동·수확) 풀 파이프라인 (보스=0~3).</summary>
    public void DebugCastAllEnemySkills(int skillIndex) => StartCoroutine(DebugCastAllEnemySkillsRoutine(skillIndex));

    private IEnumerator DebugCastAllEnemySkillsRoutine(int skillIndex)
    {
        foreach (var e in enemies.Where(x => x != null && !x.isDead && !x.isPassive).ToList())
        {
            if (e.skillIds == null || skillIndex < 0 || skillIndex >= e.skillIds.Length)
            {
                Debug.Log($"[디버그] {e?.displayName} — 스킬{skillIndex + 1} 없음(보유 {e?.skillIds?.Length ?? 0}개), 스킵");
                continue;
            }
            var skill = EnemySkillDatabase.Instance != null ? EnemySkillDatabase.Instance.GetSkill(e.skillIds[skillIndex]) : null;
            if (skill == null) continue;
            Debug.Log($"[디버그] {e.displayName} 스킬{skillIndex + 1} 시전 — {skill.displayName}");
            yield return StartCoroutine(ExecuteEnemySkillCast(e, skill));
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>살아있는 적 전원이 1회씩 행동 (스킬 자동 선택 — 적 연출 테스트).</summary>
    public void DebugEnemyTurnOnce() => StartCoroutine(DebugEnemyTurnRoutine());

    private IEnumerator DebugEnemyTurnRoutine()
    {
        foreach (var e in enemies.Where(x => x != null && !x.isDead).ToList())
        {
            yield return StartCoroutine(ExecuteEnemyTurn(e));
            yield return new WaitForSeconds(0.15f);
        }
        Debug.Log("[디버그] 적 전원 1회 행동 완료");
    }

    /// <summary>전 유닛 피격 모션 1회 (Hit 애니메이션 테스트).</summary>
    public void DebugHitMotionAll()
    {
        foreach (var a in allies)  if (a != null && !a.isDead) a.OnDamaged?.Invoke(0, 1);
        foreach (var e in enemies) if (e != null && !e.isDead) e.OnDamaged?.Invoke(0, 1);
        Debug.Log("[디버그] 전 유닛 피격 모션");
    }
}
#endif
