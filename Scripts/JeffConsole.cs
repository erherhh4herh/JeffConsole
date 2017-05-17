#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/*
 * JeffConsole, a cutomized console with multiple functions
 * To Log call the several JeffConsole.Log functions
 */

#if UNITY_EDITOR
public class JeffConsole : EditorWindow
#else
public class JeffConsole
#endif
{
#if UNITY_EDITOR
    // Console components
    JeffConsoleHeaderBar _headerBar;
    JeffConsoleTopSection _topSection;
    JeffConsoleSplitWindow _splitWindow;
    JeffConsoleBotSection _botSection;
    JeffConsoleData _data;  // Asset paths

    public const string ASSETS_PATH = "Assets/JeffConsole";
    public const string SKINS_DIR = "Skins";
    public const string SPRITES_DIR = "Sprites";

    [MenuItem("Window/Jeff Console")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(JeffConsole));
    }

    public void OnEnable()
    {
        // Create the singleton and intialize
        _data = JeffConsoleData.Instance;
        _data.currentScrollViewHeight = this.position.height / 2;
        _data.mainEditorConsole = this;

        Application.logMessageReceivedThreaded += systemLogReceiver;
        //Application.RegisterLogCallback(JeffConsoleData.Instance.HandleLog);
        //Application.RegisterLogCallbackThreaded(JeffConsoleData.Instance.HandleLog);

        // Init components
        _headerBar = new JeffConsoleHeaderBar();
        _topSection = new JeffConsoleTopSection();
        _splitWindow = new JeffConsoleSplitWindow();
        _botSection = new JeffConsoleBotSection();
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= systemLogReceiver;
    }

    // Draw Editor
    public void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        titleContent.text = "Jeff Console";
        EditorGUILayout.EndVertical();
        _headerBar.drawHeaderBar();

        // TOP
        GUILayout.BeginVertical();
        _topSection.drawTopSection(this.position.width, this.position.height);
        JeffConsoleData data = JeffConsoleData.Instance;

        if ((!data.canCollapse && data.selectedLogMessage.hashKey() != new LogMessage().hashKey())
            || (data.canCollapse && data.selectedCollapsedMessage.message.hashKey() != new LogMessage().hashKey()))
        {


            // MID
            _splitWindow.drawWindow(this.position.width);

            // BOT
            GUILayout.BeginHorizontal();

            _botSection.drawBotSection(this.position.width, this.position.height);

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        if (_data.repaint)
            Repaint();


    }

    public void systemLogReceiver(String logString, String stackTrace, LogType type)
    {
        JeffConsole.Log(logString, JeffConsoleData.SYSTEM_TAG, convertLogTypes(type));
    }

#endif

    #region static Log functions
    public static void Log(string log)
    {
        Log(log, JeffConsoleData.EMPTY_TAG);
    }

    public static void Log(string log, string tag)
    {
        Log(log, tag, JeffLogType.NORMAL);
    }

    public static void Log(string log, string tag, JeffLogType type)
    {
        Log(log, tag, type, StackTraceEntry.EMPTY_STACK_TRACE);
    }

    private static JeffLogType convertLogTypes(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                return JeffLogType.ERROR;
            case LogType.Warning:
                return JeffLogType.WARNING;
            case LogType.Log:
                return JeffLogType.NORMAL;
        }
        return JeffLogType.NORMAL;
    }
    private static void Log(string log, string tag, JeffLogType type, string stackTrace)
    {
#if UNITY_EDITOR
        LogMessage message;
        JeffConsoleData _data = JeffConsoleData.Instance;

        if (stackTrace == StackTraceEntry.EMPTY_STACK_TRACE)
            message = new LogMessage(log, tag, type, Environment.StackTrace);
        else
            message = new LogMessage(log, tag, type, stackTrace);

        _data.logs.Add(message);

        _data.logCounter[(int)type]++;

        string hashKey = message.hashKey();

        if (!_data.collapsedHash.ContainsKey(hashKey))
        {
            CollapsedMessage collapsed = new CollapsedMessage(message);
            _data.collapsedHash.Add(hashKey, collapsed);
        }
        else
        {
            CollapsedMessage collapsed = _data.collapsedHash[hashKey];
            collapsed.counter++;
            _data.collapsedHash[hashKey] = collapsed;
        }

        if (!_data.tags.Contains(message.tag))
        {
            _data.tags.Add(message.tag);
            _data.selectedTags.Add(message.tag);
        }

        // See if should be added to currently showing
        if (_data.searchFilter == JeffConsoleData.DEFAULT_SEARCH_STR || _data.searchFilter == "")
        {
            _data.showingLogs.Add(message);
        }
        else
        {
            if (!_data.canCollapse)
            {
                if (message.log.IndexOf(_data.searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    _data.showingLogs.Add(message);
            }
            else if (_data.collapsedHash[hashKey].counter == 1) // first instance
            {
                if (message.log.IndexOf(_data.searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    _data.showingLogs.Add(_data.collapsedHash[hashKey]);
            }
        }
        if (_data.mainEditorConsole != null)
        {
            _data.mainEditorConsole.Repaint();
        }

#endif
    }

    public static void LogError(string log)
    {
        Log(log, JeffConsoleData.EMPTY_TAG, JeffLogType.ERROR);
    }

    public static void LogError(string log, string tag)
    {
        Log(log, tag, JeffLogType.ERROR);
    }

    public static void LogWarning(string log)
    {
        Log(log, JeffConsoleData.EMPTY_TAG, JeffLogType.WARNING);
    }

    public static void LogWarning(string log, string tag)
    {
        Log(log, tag, JeffLogType.WARNING);
    }
    #endregion
}
