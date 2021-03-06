﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PyConsole : MonoBehaviour {
    [SerializeField] PyInputField input;
    [SerializeField] Text consoleText;
    [SerializeField] ScrollRect scrollRect;

    [SerializeField] Toggle unityLogsToggle;
    [SerializeField] Toggle pythonLogsToggle;
    [SerializeField] Toggle expressionsToggle;

    List<Log> unityLogs = new List<Log>();
    List<Log> expressionLogs = new List<Log>();
    List<Log> pythonLogs = new List<Log>();

    public int maxCharsInRow = 200; 

    private void Start() {
        input.Init(this);
        unityLogsToggle.onValueChanged.AddListener((x) => { ReloadConsole(); });
        pythonLogsToggle.onValueChanged.AddListener((x) => { ReloadConsole(); });
        expressionsToggle.onValueChanged.AddListener((x) => { ReloadConsole(); });
    }

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }
    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string condition, string stackTrace, LogType type) {
        Color color = Color.black;
        switch (type) {
            case LogType.Error:
                color = Color.red;
                break;
            case LogType.Assert:
                break;
            case LogType.Warning:
                color = Color.yellow;
                break;
            case LogType.Log:
                break;
            case LogType.Exception:
                color = Color.red;
                break;
            default:
                break;
        }
        AddUnityLog(condition, color);
    }

    public void SetExecuteAction(Action<string> executeInputCommand) {
        input.SetExecuteAction(executeInputCommand);
    }

    internal string GetPrevCommand() {
        return expressionLogs.Last().text;
    }

    void AddUnityLog(string text, Color color) {
        if (string.IsNullOrEmpty(text))
            return;
        Log log = new Log(Time.realtimeSinceStartup, text, color);

        unityLogs.Add(log);

        if (unityLogsToggle.isOn)
            DisplayLog(log);
    }

    public void AddExpressionLog(string text) {
        if (string.IsNullOrEmpty(text))
            return;
        Log log = new Log(Time.realtimeSinceStartup, text, Color.blue);

        expressionLogs.Add(log);

        if (expressionsToggle.isOn)
            DisplayLog(log);
    }

    public void AddPythonLog(string text) {
        if (string.IsNullOrEmpty(text))
            return;

        Log log = new Log(Time.realtimeSinceStartup, text, Color.green);

        pythonLogs.Add(log);

        if (pythonLogsToggle.isOn)
            DisplayLog(log);
    }

    void DisplayLog(Log log, bool stayBottom = true) {
        if (stayBottom && scrollRect.normalizedPosition.y < 0.01)
            StartCoroutine(MoveToBottom());
        
        var output = Regex.Split(log.text, @"(.{1," + maxCharsInRow + @"})(?:\s|$)|(.{" + maxCharsInRow + @"})")
                  .Where(x => x.Length > 0)
                  .ToList();

        consoleText.text += string.Format(" {0} -> ", log.GetTime());
        for (int i = 0; i < output.Count; i++) {
            if (i > 0)
                consoleText.text += "\t\t";
            consoleText.text += string.Format("<color={0}>{1}</color>\n", log.GetHexColor(), output[i]); 
        }
    }

    // doit reload
    // join unity logs, expressions & python logs
    // order by time
    // foreach DisplayLog(log, false)

    void ReloadConsole() {
        consoleText.text = "";

        List<Log> logs = new List<Log>();
        if (unityLogsToggle.isOn)
            logs.AddRange(unityLogs);
        if (pythonLogsToggle.isOn)
            logs.AddRange(pythonLogs);
        if (expressionsToggle.isOn)
            logs.AddRange(expressionLogs);

        var orderedLogs = logs.OrderBy(x => x.time).ToList();
        foreach (var log in orderedLogs) {
            DisplayLog(log, false);
        }
    }

    private IEnumerator MoveToBottom() {
        yield return new WaitForEndOfFrame();
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}

[System.Serializable]
internal class Log {
    public string text;
    public float time;
    public Color color = Color.black;

    public Log(float time, string text, Color color) {
        this.time = time;
        this.text = text;
        this.color = color;
    }

    public string GetHexColor() {
        string hex = ColorUtility.ToHtmlStringRGBA(color);
        return "#" + hex;
    }

    public string GetTime() {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string timeText = "";
        if (timeSpan.Hours > 0)
            timeText += timeSpan.Hours + ":";
        if (timeSpan.Minutes > 0)
            timeText += timeSpan.Minutes + ":";

        timeText += timeSpan.Seconds + ":" + timeSpan.Milliseconds.ToString("00");

        return timeText;
    }
}