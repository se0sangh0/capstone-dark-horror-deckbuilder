// GameLogService.cs
// Application.logMessageReceived 를 구독해 최근 로그를 모아 두는 싱글톤.
// LogPopup 이 OnLogAdded 이벤트를 구독해 UI 에 표시한다.
//
// ── 사용 ────────────────────────────────────────────────────────
//   GameLogService.Instance.GetAll()    → 모은 로그 전체(개행 join)
//   GameLogService.Instance.OnLogAdded  → 새 로그가 추가될 때 발화
//
// ── 영속 ────────────────────────────────────────────────────────
//   persistAcrossScenes 체크하면 씬 전환에도 유지.

using System.Collections.Generic;
using UnityEngine;

public class GameLogService : Singleton<GameLogService>
{
    [SerializeField, Tooltip("보관할 최대 로그 개수")]
    private int maxLogs = 200;

    private readonly Queue<string> _logs = new();
    public  event System.Action<string> OnLogAdded;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        string prefix = type switch
        {
            LogType.Error     => "[ERR] ",
            LogType.Warning   => "[WRN] ",
            LogType.Exception => "[EXC] ",
            LogType.Assert    => "[ASR] ",
            _                 => "",
        };
        string line = prefix + condition;

        _logs.Enqueue(line);
        while (_logs.Count > maxLogs) _logs.Dequeue();

        OnLogAdded?.Invoke(line);
    }

    public string GetAll() => string.Join("\n", _logs);

    public void Clear()
    {
        _logs.Clear();
        OnLogAdded?.Invoke(""); // 빈 갱신
    }
}
