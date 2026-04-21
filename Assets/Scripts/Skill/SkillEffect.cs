// SkillEffect.cs — 스킬 실행 로직 추상 기반 클래스
// 각 스킬 스크립트(Skill_Draw.cs 등)는 이 클래스를 상속합니다.

public abstract class SkillEffect
{
    public abstract string SkillId { get; }
    public abstract void Execute(SkillContext ctx, SkillData skill);
}
