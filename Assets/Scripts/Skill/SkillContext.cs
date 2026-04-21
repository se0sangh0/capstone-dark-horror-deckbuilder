// SkillContext.cs — 스킬 실행에 필요한 컨텍스트 데이터 묶음
using System;
using System.Collections.Generic;

public class SkillContext
{
    public FellowData          User;
    public List<FellowData>    Allies;
    public List<EnemyData>     Enemies;
    public Action<FellowData>  OnAllyHpChanged; // HP 변경 후 UI 갱신 콜백
}
