//
// 关卡编辑器 编辑器工具
// Create by zhoudikai 2021.10.28
//
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;

public class StageEditor : EditorWindow
{
    static StageEditor instance;

    string defaultSavePath = ""; //文件保存路径

    public List<string> nameList = new List<string>(); //仅用于编辑器中显示一些列表选项
    public List<int> optionList = new List<int>(); //仅用于编辑器中显示一些列表

    StageConfig curStage = new StageConfig(); //当前关卡

    [MenuItem("Tools/关卡编辑器", false, -1)]
    static void ShowWindow()
    {
        instance = (StageEditor)GetWindow(typeof(StageEditor)); //实例化
        instance.position = new Rect(300, 200, 1550, 700); //Rect参数:[x, y, 宽度, 高度]
        instance.titleContent = new GUIContent("关卡编辑器"); //窗口标题;
    }

    static void OnSceneFunc(SceneView view)
    {
        if(instance)
            instance.CustomSceneGUI(view);
    }

    void OnGUI()
    {
        DrawTitlePanel(); //绘制顶部静态部分的UI
        DrawTool(curStage.toolMode); //绘制下方工具子菜单的UI
    }

    #region 工具顶部区域
    //绘制顶部区域的UI
    void DrawTitlePanel()
    {
        //标题
        EditorGUILayout.Space(10);
        GUILayout.Label("关卡编辑器", "flow node hex 6", GUILayout.Width(150), GUILayout.Height(30));
        EditorGUILayout.Space(15);

        //关卡信息
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("关卡编号", GUILayout.Width(50));
        curStage.Id = EditorGUILayout.IntField(curStage.Id, GUILayout.Width(80));
        GUILayout.Space(30);

        EditorGUILayout.LabelField("关卡信息描述", GUILayout.Width(75));
        curStage.des = EditorGUILayout.TextField(curStage.des, GUILayout.Width(150));
        GUILayout.Space(60);

        //文件操作模式
        int fileModeSelect = -1; //选择的文件操作模式
        string[] fileMode = new string[]{"新建", "保存", "加载"};
        fileModeSelect = GUILayout.Toolbar(fileModeSelect, fileMode, "LargeButton");
        switch(fileModeSelect)
        {
            case 0: //新建
                CreateNewFile();
                break;
            case 1: //保存
                SaveFile();
                break;
            case 2: //加载
                LoadFile();
                break;
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUI.BeginChangeCheck();
        curStage.toolMode = (ToolMode)GUILayout.Toolbar((int)curStage.toolMode, curStage.toolDes); //工具栏菜单
        if(EditorGUI.EndChangeCheck())
        {
            GUI.FocusControl(null);
        }
        GUILayout.Space(10);
    }
    #endregion

    //绘制下方工具部分
    void DrawTool(ToolMode selectMode)
    {
        switch(selectMode)
        {
            case ToolMode.StageInfomation: //关卡信息
                ShowStageInfomation(curStage.stageInfo);
                break;
            case ToolMode.EventTrigger: //事件触发
                ShowEventTrigger(curStage.eventTrigger);
                break;
            case ToolMode.MapEditor: //地图编辑
                ShowMapEditor(curStage.mapInfo);
                break;
            case ToolMode.EventEditor: //事件编辑
                ShowEventEditor(curStage.eventEdit);
                break;
            case ToolMode.TagManager: //标签管理
                ShowTags(curStage.tagManager);
                break;
            case ToolMode.VariableManager: //变量管理
                ShowVariableMagager(curStage.variableManager);
                break;
        }
    }

    #region 关卡信息
    //绘制编辑器子菜单 关卡信息
    void ShowStageInfomation(StageInfomation info)
    {
        GUILayout.Label("其他信息", "WarningOverlay");
        //关卡结算方式
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("关卡结算方式", GUILayout.Width(75));
        info.settleType = (SettleType)EditorGUILayout.EnumPopup(info.settleType, GUILayout.Width(80));
        GUILayout.EndHorizontal();

        //战场限制
        BattleLimit battleLimit = info.battleLimit;
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("战场限制", GUILayout.Width(50));
        GUILayout.Space(10);
        battleLimit.haveLimit = EditorGUILayout.ToggleLeft("有无限制", battleLimit.haveLimit, GUILayout.Width(70));
        if(battleLimit.haveLimit) //如果有限制
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("限制类型", GUILayout.Width(50));
            EditorGUI.BeginChangeCheck();
            battleLimit.limitType = (BattleLimitType)EditorGUILayout.EnumPopup(battleLimit.limitType, GUILayout.Width(60));
            if(EditorGUI.EndChangeCheck())
            {
                AddBattleLimit(battleLimit.limitType, ref battleLimit.battleLimits);
            }
            for(int i = 0; i < battleLimit.battleLimits.Count; i++)
            {
                BaseBattleLimit item = battleLimit.battleLimits[i];
                item.isSelect = EditorGUILayout.ToggleLeft(item.des, item.isSelect, GUILayout.Width(50));
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);        

        //胜利条件title
        GUILayout.Label("胜利条件", "WarningOverlay");
        GUILayout.BeginHorizontal();
        GUILayout.Label("胜利条件", GUILayout.Width(60));
        GUILayout.BeginVertical();

        //胜利条件选项
        //敌方全灭
        GUILayout.BeginHorizontal();
        var enemyDead = info.enemyDead;
        enemyDead.isSelected = EditorGUILayout.ToggleLeft(enemyDead.des, enemyDead.isSelected, GUILayout.Width(80));
        GUILayout.EndHorizontal();

        //限时守护
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(info.battleOvertime.isSelected);
        var timelimit = info.timeLimit;
        timelimit.isSelected = EditorGUILayout.ToggleLeft(timelimit.des, timelimit.isSelected, GUILayout.Width(80));
        EditorGUI.BeginDisabledGroup(!timelimit.isSelected);
        GUILayout.Label(timelimit.limitDes, GUILayout.Width(60));
        timelimit.limitData = EditorGUILayout.IntField(timelimit.limitData, GUILayout.Width(80));
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        if(timelimit.isSelected == false)
        {
            timelimit.limitData = 0;
        }

        //胜利自定义
        GUILayout.BeginHorizontal();
        var winCustom = info.winCustom;
        winCustom.isSelected = EditorGUILayout.ToggleLeft(winCustom.des, winCustom.isSelected, GUILayout.Width(80));
        EditorGUI.BeginDisabledGroup(!winCustom.isSelected);
        EditorGUILayout.LabelField("关卡变量", GUILayout.Width(50));
        AddAllVariableToList();
        winCustom.variableId = EditorGUILayout.IntPopup(winCustom.variableId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("计算", GUILayout.Width(25));
        winCustom.calculationType = (CalculationType)EditorGUILayout.EnumPopup(winCustom.calculationType, GUILayout.Width(70));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("数值", GUILayout.Width(25));
        winCustom.number = EditorGUILayout.IntField(winCustom.number, GUILayout.Width(50));
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        if(winCustom.isSelected == false)
        {
            winCustom.variableId = 0;
            winCustom.calculationType = CalculationType.小于;
            winCustom.number = 0;
        }

        GUILayout.Space(10);

        //失败条件title
        GUILayout.Label("失败条件", "WarningOverlay");
        GUILayout.BeginHorizontal();
        GUILayout.Label("失败条件", GUILayout.Width(60));
        GUILayout.BeginVertical();

        //失败条件选项
        //基地毁灭
        GUILayout.BeginHorizontal();
        var baseDestroy = info.baseDestroy;
        baseDestroy.isSelected = EditorGUILayout.ToggleLeft(baseDestroy.des, baseDestroy.isSelected, GUILayout.Width(80));
        GUILayout.EndHorizontal();

        //战斗超时
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(info.timeLimit.isSelected);
        var battleOvertime = info.battleOvertime;
        battleOvertime.isSelected = EditorGUILayout.ToggleLeft(battleOvertime.des, battleOvertime.isSelected, GUILayout.Width(80));
        EditorGUI.BeginDisabledGroup(!battleOvertime.isSelected);
        GUILayout.Label(battleOvertime.limitDes, GUILayout.Width(60));
        battleOvertime.limitData = EditorGUILayout.IntField(battleOvertime.limitData, GUILayout.Width(80));
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        if(battleOvertime.isSelected == false)
        {
            battleOvertime.limitData = 0;
        }

        //失败自定义
        GUILayout.BeginHorizontal();
        var defeatCustom = info.defeatCustom;
        defeatCustom.isSelected = EditorGUILayout.ToggleLeft(defeatCustom.des, defeatCustom.isSelected, GUILayout.Width(80));
        EditorGUI.BeginDisabledGroup(!defeatCustom.isSelected);
        EditorGUILayout.LabelField("关卡变量", GUILayout.Width(50));
        AddAllVariableToList();
        defeatCustom.variableId = EditorGUILayout.IntPopup(defeatCustom.variableId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("计算", GUILayout.Width(25));
        defeatCustom.calculationType = (CalculationType)EditorGUILayout.EnumPopup(defeatCustom.calculationType, GUILayout.Width(70));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("数值", GUILayout.Width(25));
        defeatCustom.number = EditorGUILayout.IntField(defeatCustom.number, GUILayout.Width(50));
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        if(defeatCustom.isSelected == false)
        {
            defeatCustom.variableId = 0;
            defeatCustom.calculationType = CalculationType.小于;
            defeatCustom.number = 0;
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Space(40);
        GUILayout.Label("说明：胜利条件与失败条件均可多选，但胜利条件中的\"限时守护\"与失败条件中的\"战斗超时\"互斥。", "TV Ping");
        GUILayout.Space(30);
    }

    void AddBattleLimit(BattleLimitType limitType, ref List<BaseBattleLimit> battleLimits)
    {
        switch(limitType)
        {
            case BattleLimitType.职业:
                battleLimits = new List<BaseBattleLimit>{new BaseBattleLimit("战士", 0),
                                                         new BaseBattleLimit("铁盾", 1),
                                                         new BaseBattleLimit("火力", 2),
                                                         new BaseBattleLimit("救助", 3),
                                                         new BaseBattleLimit("支援", 4),
                                                         new BaseBattleLimit("特殊", 5)};
                break;
            case BattleLimitType.射程:
                battleLimits = new List<BaseBattleLimit>{new BaseBattleLimit("近战", 0),
                                                         new BaseBattleLimit("中程", 1),
                                                         new BaseBattleLimit("远程", 2)};
                break;
            case BattleLimitType.定位:
                battleLimits = new List<BaseBattleLimit>{new BaseBattleLimit("输出", 0),
                                                         new BaseBattleLimit("防御", 1),
                                                         new BaseBattleLimit("辅助", 2)};
                break;
            case BattleLimitType.特性:
                battleLimits = new List<BaseBattleLimit>{new BaseBattleLimit("爆发", 0),
                                                         new BaseBattleLimit("持续", 1),
                                                         new BaseBattleLimit("控制", 2),
                                                         new BaseBattleLimit("治疗", 3),
                                                         new BaseBattleLimit("增幅", 4),
                                                         new BaseBattleLimit("削弱", 5),
                                                         new BaseBattleLimit("统御", 6),
                                                         new BaseBattleLimit("维护", 7),
                                                         new BaseBattleLimit("召唤", 8)};
                break;
        }
    }
    #endregion

    #region 事件触发
    //绘制编辑器子菜单 事件触发
    void ShowEventTrigger(EventTrigger info)
    {
        info.selectPage = GUILayout.Toolbar(info.selectPage, info.toolbarDes);
        switch(info.selectPage)
        {
            case 0: //波次事件
                ShowWaveEvent(info);
                break;
            case 1: //全局事件
                ShowTotalEvent(info);
                break;
        }
    }

    //事件触发 显示波次事件
    void ShowWaveEvent(EventTrigger info)
    {
        GUILayout.Label("波次事件", "WarningOverlay");
        if(GUILayout.Button("添加波次", "LargeButton", GUILayout.Width(100)))
        {
            int listCount = info.eventWaveData_List.Count;
            info.eventWaveData_List.Add(new WaveData(listCount));
        }
        info.waveDataScrollPos = EditorGUILayout.BeginScrollView(info.waveDataScrollPos);
        for(int i = 0; i < info.eventWaveData_List.Count; i++)
        {
            _drawContent_WaveData(info.eventWaveData_List[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    //事件触发 显示全局事件
    void ShowTotalEvent(EventTrigger info)
    {
        GUILayout.Label("全局事件", "WarningOverlay");
        if(GUILayout.Button("添加全局事件", "LargeButton", GUILayout.Width(100)))
        {
            int listCount = info.totalEvent_List.Count;
            info.totalEvent_List.Add(new TotalEvent());
        }
        info.totalEventScrollPos = EditorGUILayout.BeginScrollView(info.totalEventScrollPos);
        for(int i = 0; i < info.totalEvent_List.Count; i++)
        {
            _drawContent_TotalEvent(info.totalEvent_List[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    //事件触发 绘制每一条全局事件
    void _drawContent_TotalEvent(TotalEvent totalEvent)
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        totalEvent.ShowEvent = EditorGUILayout.Foldout(totalEvent.ShowEvent, "全局事件");
        if(GUILayout.Button("删除全局事件", GUILayout.Width(120)))
        {
            if(EditorUtility.DisplayDialog("警告！", "是否删除当前全局事件的数据？", "是", "否"))
            {
                curStage.eventTrigger.totalEvent_List.Remove(totalEvent);
            }
        }
        GUILayout.EndHorizontal();
        if(totalEvent.ShowEvent)
        {
            EditorGUI.indentLevel++;
            totalEvent.ShowAddEvent = EditorGUILayout.Foldout(totalEvent.ShowAddEvent, "添加条件");
            EditorGUI.indentLevel--;
            if(totalEvent.ShowAddEvent)
            {
                foreach(string conditionDes in totalEvent.eventConditionDes)
                {
                    GUILayout.BeginHorizontal("box");
                    GUILayout.Space(30);
                    GUILayout.Label(conditionDes, GUILayout.Width(100));
                    GUILayout.Space(20);
                    if(GUILayout.Button("", "OL Plus"))
                    {
                        AddEventCondition(conditionDes, totalEvent);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            ShowEventContent(totalEvent); //显示触发事件的内容
        }
        GUILayout.EndVertical();
    }

    //事件触发 绘制每一条波次事件
    void _drawContent_WaveData(WaveData data)
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        data.ShowEvent = EditorGUILayout.Foldout(data.ShowEvent, "波次" + data.waveId);
        EditorGUILayout.LabelField("间隔类型", GUILayout.Width(50));
        data.intervalType = (WaveIntervalType)EditorGUILayout.EnumPopup(data.intervalType, GUILayout.Width(70));
        GUILayout.Space(20);
        GUILayout.Label("时间", GUILayout.Width(28));
        data.intervalTime = EditorGUILayout.IntField(data.intervalTime, GUILayout.Width(80));
        GUILayout.Space(20);
        if(GUILayout.Button("复制波次", GUILayout.Width(70)))
        {
            int Count = curStage.eventTrigger.eventWaveData_List.Count;
            WaveData newData = CloneWaveData(data);
            newData.waveId = Count;
            curStage.eventTrigger.eventWaveData_List.Add(newData);
        }
        var waveList = curStage.eventTrigger.eventWaveData_List;
        if(waveList.IndexOf(data) > 0)
        {
            if(GUILayout.Button("上移", GUILayout.Width(40)))
            {
                int index = waveList.IndexOf(data);
                var temp = waveList[index - 1];
                temp.waveId++;
                data.waveId--;
                waveList[index - 1] = data;
                waveList[index] = temp;
                GUI.FocusControl(null);
            }
        }
        if(waveList.IndexOf(data) < waveList.Count - 1)
        {
            if(GUILayout.Button("下移", GUILayout.Width(40)))
            {
                int index = waveList.IndexOf(data);
                var temp = waveList[index + 1];
                temp.waveId--;
                data.waveId++;
                waveList[index + 1] = data;
                waveList[index] = temp;
                GUI.FocusControl(null);
            }
        }
        if(GUILayout.Button("删除波次", GUILayout.Width(70)))
        {
            if(EditorUtility.DisplayDialog("警告！", "是否删除当前波次的数据？", "是", "否"))
            {
                curStage.eventTrigger.eventWaveData_List.Remove(data);
                int i = 0;
                curStage.eventTrigger.eventWaveData_List.ForEach((x) => //重新设置序号
                {
                    x.waveId = i++;
                });
            }
        }
        GUILayout.EndHorizontal();
        if(data.ShowEvent)
        {
            EditorGUI.indentLevel++;
            data.ShowAddEvent = EditorGUILayout.Foldout(data.ShowAddEvent, "添加条件");
            EditorGUI.indentLevel--;
            if(data.ShowAddEvent)
            {
                foreach(string conditionDes in data.eventConditionDes)
                {
                    GUILayout.BeginHorizontal("box");
                    GUILayout.Space(30);
                    GUILayout.Label(conditionDes, GUILayout.Width(100));
                    GUILayout.Space(20);
                    if(GUILayout.Button("", "OL Plus"))
                    {
                        AddEventCondition(conditionDes, data);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            ShowEventContent(data); //显示触发事件的内容
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 克隆波次数据（深度拷贝）
    /// </summary>
    /// <param name="data">旧的波次数据</param>
    /// <returns>克隆的波次数据</returns>
    public static WaveData CloneWaveData(WaveData oldWaveData)
    {
        WaveData newWaveData = new WaveData();
        newWaveData.waveId = oldWaveData.waveId;
        newWaveData.intervalType = oldWaveData.intervalType;
        newWaveData.intervalTime = oldWaveData.intervalTime;
        List<EventData> newList = new List<EventData>();
        for(int i = 0; i < oldWaveData.triggerEvent_List.Count; i++)
        {
            EventData oldEventData = oldWaveData.triggerEvent_List[i];
            EventData newEventData = CloneEventData(oldEventData);
            newList.Add(newEventData);
        }
        newWaveData.triggerEvent_List = newList;

        return newWaveData;
    }

    //添加事件触发条件
    void AddEventCondition(string conditionDes, EventBase data)
    {
        int Count = data.triggerEvent_List.Count;
        data.triggerEvent_List.Add(new EventData(Count)); //先再事件列表中加一条事件
        var eventData = data.triggerEvent_List[Count];
        switch(conditionDes)
        {
            case "时间触发":
                eventData.eventConditions.Add(new TimeTrigger(0, 0));
                break;
            case "地块触发":
                eventData.eventConditions.Add(new AreaTrigger(0));
                break;
            case "当前怪物数量触发":
                eventData.eventConditions.Add(new CurrentMonster(0));
                break;
            case "累计怪物数量触发":
                eventData.eventConditions.Add(new TotalMonster(0));
                break;
            case "变量触发":
                eventData.eventConditions.Add(new VariableTrigger(0));
                break;
            case "血量触发":
                eventData.eventConditions.Add(new HpTrigger(0));
                break;
            case "多条件触发":
                eventData.isMulti = true;
                int i = 0;
                data.triggerEvent_List.ForEach((e) =>
                {
                    if(e.isMulti)
                    {
                        i++;
                    }
                });
                eventData.eventDes = String.Format("多条件事件{0}", i);
                break;
        }
    }

    //显示触发事件的内容
    void ShowEventContent(EventBase data)
    {
        for(int i = 0; i < data.triggerEvent_List.Count; i++)
        {
            GUILayout.Space(5);
            EventData eventData = data.triggerEvent_List[i];
            GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));

            if(eventData.isMulti == false) //单条件
            {
                BaseEventCondition eventCondition = eventData.eventConditions[0];
                ShowEventConditionInEditor(eventCondition);
            }
            else //多条件
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("多条件触发", GUILayout.Width(63));
                GUILayout.Space(10);
                eventData.isLoop = EditorGUILayout.ToggleLeft("循环", eventData.isLoop, GUILayout.Width(45));
                GUILayout.Space(10);
                EditorGUILayout.LabelField("触发方式", GUILayout.Width(50));
                eventData.eventMode = (ConditionTriggerType)EditorGUILayout.EnumPopup(eventData.eventMode, GUILayout.Width(50));
                GUILayout.Space(10);
                EditorGUILayout.LabelField("事件说明", GUILayout.Width(50));
                eventData.eventDes = EditorGUILayout.TextField(eventData.eventDes, GUILayout.Width(80));
                GUILayout.Space(10);
                if(GUILayout.Button("添加条件", GUILayout.Width(60)))
                {
                    ShowAddEventConditionMenu(eventData);
                }
                GUILayout.EndHorizontal();
                for(int j = 0; j < eventData.eventConditions.Count; j++)
                {
                    GUILayout.BeginHorizontal();
                    ShowEventConditionInEditor(eventData.eventConditions[j]);
                    if(GUILayout.Button("删除", GUILayout.Width(50)))
                    {
                        eventData.eventConditions.RemoveAt(j);
                        int k = 0;
                        eventData.eventConditions.ForEach((x) =>
                        {
                            x.index = k++;
                        });
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(3);
                GUILayout.EndVertical();
            }

            EditorGUILayout.LabelField("|", GUILayout.Width(5)); //分隔符

            //显示触发效果
            if(eventData.eventBehavior.behaviorType == EventBehaviorType.Null) //当触发效果为空时
            {
                if(GUILayout.Button("添加效果", GUILayout.Width(80)))
                {
                    ShowAddBehaviorMenu(eventData, data);
                }
            }
            else //触发效果不为空 显示触发效果对应编辑界面
            {
                ShowBehaviorInEditor(eventData.eventBehavior, data.triggerEvent_List); //触发效果
            }
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("重置效果"))
            {
                eventData.eventBehavior = new NullBehaviour();
            }
            if(GUILayout.Button("复制事件"))
            {
                int Count = data.triggerEvent_List.Count;
                EventData newData = CloneEventData(eventData);
                newData.eventId = Count;
                data.triggerEvent_List.Add(newData);
            }
            if(data.triggerEvent_List.IndexOf(eventData) > 0)
            {
                if(GUILayout.Button("上移"))
                {
                    int index = data.triggerEvent_List.IndexOf(eventData);
                    var temp = data.triggerEvent_List[index - 1];
                    temp.eventId++;
                    eventData.eventId--;
                    data.triggerEvent_List[index - 1] = eventData;
                    data.triggerEvent_List[index] = temp;
                    GUI.FocusControl(null);
                }
            }
            if(data.triggerEvent_List.IndexOf(eventData) < data.triggerEvent_List.Count - 1)
            {
                if(GUILayout.Button("下移"))
                {
                    int index = data.triggerEvent_List.IndexOf(eventData);
                    var temp = data.triggerEvent_List[index + 1];
                    temp.eventId--;
                    eventData.eventId++;
                    data.triggerEvent_List[index + 1] = eventData;
                    data.triggerEvent_List[index] = temp;
                    GUI.FocusControl(null);
                }
            }
            if(GUILayout.Button("", "OL Minus")) //删除按钮
            {
                data.triggerEvent_List.Remove(eventData);
                int k = 0;
                data.triggerEvent_List.ForEach((x) => //重新设置序号
                {
                    x.eventId = k++;
                });
            }

            GUILayout.EndHorizontal();
        }
        GUILayout.Space(3);
    }

    /// <summary>
    /// 克隆触发事件（深度拷贝）
    /// </summary>
    /// <param name="oldEventData">旧的触发事件数据</param>
    /// <returns>克隆的触发事件数据</returns>
    public static EventData CloneEventData(EventData oldEventData)
    {
        EventData newEventData = new EventData();
        newEventData.eventId = oldEventData.eventId;

        BaseEventBehavior newEventBehavior = DeepCopyByReflection(oldEventData.eventBehavior);
        newEventData.eventBehavior = newEventBehavior;

        newEventData.isMulti = oldEventData.isMulti;
        newEventData.isLoop = oldEventData.isLoop;
        newEventData.eventMode = oldEventData.eventMode;

        for(int i = 0; i < oldEventData.eventConditions.Count; i++)
        {
            newEventData.eventConditions.Add(DeepCopyByReflection(oldEventData.eventConditions[i]));
        }
        return newEventData;
    }

    /// <summary>
    /// 显示添加事件触发条件的菜单
    /// </summary>
    void ShowAddEventConditionMenu(EventData eventData)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("时间触发"), false, AddTimeTrigger, eventData);
        menu.AddItem(new GUIContent("地块触发"), false, AddAreaTrigger, eventData);
        menu.AddItem(new GUIContent("当前怪物数量触发"), false, AddCurrentMonster, eventData);
        menu.AddItem(new GUIContent("累计怪物数量触发"), false, AddTotalMonster, eventData);
        menu.AddItem(new GUIContent("变量触发"), false, AddVariableTrigger, eventData);
        menu.AddItem(new GUIContent("血量触发"), false, AddHpTrigger, eventData);
        menu.ShowAsContext();
    }

    /// <summary>
    /// 添加事件触发条件 时间触发
    /// </summary>
    void AddTimeTrigger(dynamic eventData)
    {
        int Count = eventData.eventConditions.Count;
        eventData.eventConditions.Add(new TimeTrigger(Count, 0));
    }

    /// <summary>
    /// 添加事件触发条件 地块触发
    /// </summary>
    void AddAreaTrigger(dynamic eventData)
    {
        int Count = eventData.eventConditions.Count;
        eventData.eventConditions.Add(new AreaTrigger(Count));
    }

    /// <summary>
    /// 添加事件触发条件 当前怪物数量触发
    /// </summary>
    void AddCurrentMonster(dynamic eventData)
    {
        int Count = eventData.eventConditions.Count;
        eventData.eventConditions.Add(new CurrentMonster(Count));
    }

    /// <summary>
    /// 添加事件触发条件 累计怪物数量触发
    /// </summary>
    void AddTotalMonster(dynamic eventData)
    {
        int Count = eventData.eventConditions.Count;
        eventData.eventConditions.Add(new TotalMonster(Count));
    }

    /// <summary>
    /// 添加事件触发条件 变量触发
    /// </summary>
    void AddVariableTrigger(dynamic eventData)
    {
        int Count = eventData.eventConditions.Count;
        eventData.eventConditions.Add(new VariableTrigger(Count));
    }

    /// <summary>
    /// 添加事件触发条件 血量触发
    /// </summary>
    void AddHpTrigger(dynamic eventData)
    {
        int Count = eventData.eventConditions.Count;
        eventData.eventConditions.Add(new HpTrigger(Count));
    }

    /// <summary>
    /// 在编辑器中显示事件触发条件
    /// </summary>
    /// <param name="eventCondition"></param>
    void ShowEventConditionInEditor(BaseEventCondition eventCondition)
    {
        GUILayout.BeginHorizontal();
        if(eventCondition.conditionType == EventConditionType.TimeTrigger) //时间触发
        {
            TimeTrigger timeTrigger = (TimeTrigger)eventCondition;
            EditorGUILayout.LabelField("时间触发", GUILayout.Width(50));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("刷新时间", GUILayout.Width(50));
            timeTrigger.time = EditorGUILayout.IntField(timeTrigger.time, GUILayout.Width(70));
        }
        if(eventCondition.conditionType == EventConditionType.AreaTrigger) //地块触发
        {
            AreaTrigger areaTrigger = (AreaTrigger)eventCondition;
            EditorGUILayout.LabelField("地块触发", GUILayout.Width(50));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("目标", GUILayout.Width(25));
            areaTrigger.targetType = (AreaTargetType)EditorGUILayout.EnumPopup(areaTrigger.targetType, GUILayout.Width(70));
            GUILayout.Space(20);
            EditorGUILayout.LabelField("地块", GUILayout.Width(25));
            AddAllAreaIdToList();
            areaTrigger.areaIndex = EditorGUILayout.IntPopup(areaTrigger.areaIndex, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(70));
            areaTrigger.areaAction = (AreaAction)EditorGUILayout.EnumPopup(areaTrigger.areaAction, GUILayout.Width(45));
        }
        if(eventCondition.conditionType == EventConditionType.CurrentMonsterNum) //当前怪物数量触发
        {
            CurrentMonster currentMonster = (CurrentMonster)eventCondition;
            EditorGUILayout.LabelField("当前怪物数量触发", GUILayout.Width(100));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("类型", GUILayout.Width(25));
            currentMonster.monsterType = (MonsterType)EditorGUILayout.EnumPopup(currentMonster.monsterType, GUILayout.Width(80));
            if(currentMonster.monsterType == MonsterType.指定怪物) //指定怪物
            {
                currentMonster.monsterId = EditorGUILayout.IntField(currentMonster.monsterId, GUILayout.Width(60));
            }
            if(currentMonster.monsterType == MonsterType.出怪点怪物) //出怪点怪物
            {
                AddAllSpawnPointToList();
                currentMonster.pointId = EditorGUILayout.IntPopup(currentMonster.pointId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(75));
            }
            GUILayout.Space(10);
            EditorGUILayout.LabelField("计算", GUILayout.Width(25));
            currentMonster.calculationType = (CalculationType)EditorGUILayout.EnumPopup(currentMonster.calculationType, GUILayout.Width(70));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("数量", GUILayout.Width(25));
            currentMonster.number = EditorGUILayout.IntField(currentMonster.number, GUILayout.Width(50));
        }
        if(eventCondition.conditionType == EventConditionType.TotalMonsterNum) //累计怪物数量触发
        {
            TotalMonster totalMonster = (TotalMonster)eventCondition;
            EditorGUILayout.LabelField("累计怪物数量触发", GUILayout.Width(100));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("类型", GUILayout.Width(25));
            totalMonster.monsterType = (MonsterType)EditorGUILayout.EnumPopup(totalMonster.monsterType, GUILayout.Width(80));
            if(totalMonster.monsterType == MonsterType.指定怪物) //指定怪物
            {
                totalMonster.monsterId = EditorGUILayout.IntField(totalMonster.monsterId, GUILayout.Width(60));
            }
            if(totalMonster.monsterType == MonsterType.出怪点怪物) //出怪点怪物
            {
                AddAllSpawnPointToList();
                totalMonster.pointId = EditorGUILayout.IntPopup(totalMonster.pointId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(75));
            }
            GUILayout.Space(10);
            EditorGUILayout.LabelField("计算", GUILayout.Width(25));
            totalMonster.calculationType = (CalculationType)EditorGUILayout.EnumPopup(totalMonster.calculationType, GUILayout.Width(70));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("数量", GUILayout.Width(25));
            totalMonster.number = EditorGUILayout.IntField(totalMonster.number, GUILayout.Width(50));
        }
        if(eventCondition.conditionType == EventConditionType.VariableTrigger) //变量触发
        {
            VariableTrigger variableTrigger = (VariableTrigger)eventCondition;
            EditorGUILayout.LabelField("变量触发", GUILayout.Width(50));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("关卡变量", GUILayout.Width(50));
            AddAllVariableToList();
            variableTrigger.variableId = EditorGUILayout.IntPopup(variableTrigger.variableId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("计算", GUILayout.Width(25));
            variableTrigger.calculationType = (CalculationType)EditorGUILayout.EnumPopup(variableTrigger.calculationType, GUILayout.Width(70));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("数值", GUILayout.Width(25));
            variableTrigger.value = EditorGUILayout.IntField(variableTrigger.value, GUILayout.Width(50));
        }
        if(eventCondition.conditionType == EventConditionType.HpTrigger) //血量触发
        {
            HpTrigger hpTrigger = (HpTrigger)eventCondition;
            EditorGUILayout.LabelField("血量触发", GUILayout.Width(50));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("标签", GUILayout.Width(25));
            AddFriendAndMonsterTagIdToList();
            hpTrigger.tagId = EditorGUILayout.IntPopup(hpTrigger.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("计算", GUILayout.Width(25));
            hpTrigger.calculationType = (CalculationType)EditorGUILayout.EnumPopup(hpTrigger.calculationType, GUILayout.Width(70));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("血量", GUILayout.Width(25));
            hpTrigger.Hp = EditorGUILayout.IntField(hpTrigger.Hp, GUILayout.Width(50));
        }
        GUILayout.EndHorizontal();
    }

    //显示添加效果菜单
    void ShowAddBehaviorMenu(EventData eventData, EventBase data)
    {
        GenericMenu behaviorMenu = new GenericMenu(); //菜单
        behaviorMenu.AddItem(new GUIContent("出怪"), false, AddBehavior_SpawnMonster, eventData); //出怪菜单项
        behaviorMenu.AddSeparator("");
        behaviorMenu.AddItem(new GUIContent("触发事件"), false, AddBehavior_TriggerEvent, eventData); //触发事件菜单项
        behaviorMenu.AddItem(new GUIContent("地块控制"), false, AddBehavior_AreaControl, eventData); //地块控制菜单项
        behaviorMenu.AddItem(new GUIContent("移动目标控制"), false, AddBehavior_MoveControl, eventData); //移动目标控制菜单项
        behaviorMenu.AddItem(new GUIContent("变量变更"), false, AddBehavior_VariableChange, eventData); //变量变更菜单项
        behaviorMenu.AddItem(new GUIContent("星级控制"), false, AddBehavior_StarControl, eventData); //星级控制菜单项
        behaviorMenu.AddItem(new GUIContent("触发技能"), false, AddBehavior_ShowAbility, eventData); //触发技能菜单项
        behaviorMenu.AddSeparator("");
        behaviorMenu.AddItem(new GUIContent("地块Buff"), false, AddBehavior_AreaBuff, eventData); //地块Buff菜单项
        behaviorMenu.AddItem(new GUIContent("单位Buff"), false, AddBehavior_UnitBuff, eventData); //单位Buff菜单项
        behaviorMenu.AddSeparator("");
        behaviorMenu.AddItem(new GUIContent("添加友军"), false, AddBehavior_AddFriendForce, eventData); //添加友军菜单项
        behaviorMenu.AddItem(new GUIContent("添加Npc"), false, AddBehavior_AddNpc, eventData); //添加Npc菜单项
        behaviorMenu.AddItem(new GUIContent("删除单位"), false, AddBehavior_DeleteUnit, eventData); //删除单位菜单项
        behaviorMenu.AddSeparator("");
        behaviorMenu.AddItem(new GUIContent("剧情演绎"), false, AddBehavior_ShowPlot, eventData); //剧情演绎菜单项
        behaviorMenu.AddSeparator("");
        behaviorMenu.AddItem(new GUIContent("结束循环"), false, AddBehavior_EndLoop, eventData); //结束循环菜单项
        if(data.GetType() == typeof(TotalEvent))
        {
            behaviorMenu.AddItem(new GUIContent("波次计数"), false, AddBehavior_WaveCount, eventData); //波次计数菜单项（仅在全局事件中生效）
        }
        behaviorMenu.ShowAsContext();
    }

    //添加触发效果 出怪
    void AddBehavior_SpawnMonster(dynamic eventData)
    {
        eventData.eventBehavior = new SpawnMonster();
    }

    //添加触发效果 触发事件
    void AddBehavior_TriggerEvent(dynamic eventData)
    {
        eventData.eventBehavior = new TriggerEvent();
    }

    //添加触发效果 地块控制
    void AddBehavior_AreaControl(dynamic eventData)
    {
        eventData.eventBehavior = new AreaControl();
    }

    //添加触发效果 移动目标控制
    void AddBehavior_MoveControl(dynamic eventData)
    {
        eventData.eventBehavior = new MoveControl();
    }

    //添加触发效果 变量变更
    void AddBehavior_VariableChange(dynamic eventData)
    {
        eventData.eventBehavior = new VariableChange();
    }

    //添加触发效果 星级控制
    void AddBehavior_StarControl(dynamic eventData)
    {
        eventData.eventBehavior = new StarControl();
    }

    //添加触发效果 触发技能
    void AddBehavior_ShowAbility(dynamic eventData)
    {
        eventData.eventBehavior = new ShowAbility();
    }

    //添加触发效果 地块Buff
    void AddBehavior_AreaBuff(dynamic eventData)
    {
        eventData.eventBehavior = new AreaBuff();
    }

    //添加触发效果 单位Buff
    void AddBehavior_UnitBuff(dynamic eventData)
    {
        eventData.eventBehavior = new UnitBuff();
    }

    //添加触发效果 添加友军
    void AddBehavior_AddFriendForce(dynamic eventData)
    {
        eventData.eventBehavior = new AddFriendForce();
    }

    //添加触发效果 添加Npc
    void AddBehavior_AddNpc(dynamic eventData)
    {
        eventData.eventBehavior = new AddNpc();
    }

    //添加触发效果 删除单位
    void AddBehavior_DeleteUnit(dynamic eventData)
    {
        eventData.eventBehavior = new DeleteUnit();
    }

    //添加触发效果 剧情演绎
    void AddBehavior_ShowPlot(dynamic eventData)
    {
        eventData.eventBehavior = new ShowPlot();
    }

    //添加触发效果 结束循环
    void AddBehavior_EndLoop(dynamic eventData)
    {
        eventData.eventBehavior = new EndLoop();
    }

    //添加触发效果 波次计数（波次计数仅在全局事件中出现）
    void AddBehavior_WaveCount(dynamic eventData)
    {
        eventData.eventBehavior = new WaveCount();
    }

    //编辑器中根据触发效果类型显示不同界面
    void ShowBehaviorInEditor(BaseEventBehavior eventBehavior, List<EventData> eventList)
    {
        switch(eventBehavior.behaviorType) //触发效果的类型
        {
            case EventBehaviorType.SpawnMonster: //出怪
                SpawnMonster spawnMonster = (SpawnMonster)eventBehavior;
                ShowSpawnMonsterInEditor(spawnMonster);
                break;
            case EventBehaviorType.TriggerEvent: //触发事件
                TriggerEvent triggerEvent = (TriggerEvent)eventBehavior;
                ShowTriggerEventInEditor(triggerEvent);
                break;
            case EventBehaviorType.AreaControl: //地块控制
                AreaControl areaControl = (AreaControl)eventBehavior;
                ShowAreaControlInEditor(areaControl);
                break;
            case EventBehaviorType.MoveControl: //移动目标控制
                MoveControl moveControl = (MoveControl)eventBehavior;
                ShowMoveControlInEditor(moveControl);
                break;
            case EventBehaviorType.VariableChange: //变量变更
                VariableChange variableChange = (VariableChange)eventBehavior;
                ShowVariableChangeInEditor(variableChange);
                break;
            case EventBehaviorType.StarControl: //星级控制
                StarControl starControl = (StarControl)eventBehavior;
                ShowStarControlInEditor(starControl);
                break;
            case EventBehaviorType.ShowAbility: //触发技能
                ShowAbility showAbility = (ShowAbility)eventBehavior;
                ShowShowAbilityInEditor(showAbility);
                break;
            case EventBehaviorType.AreaBuff: //地块Buff
                AreaBuff areaBuff = (AreaBuff)eventBehavior;
                ShowAreaBuffInEditor(areaBuff);
                break;
            case EventBehaviorType.UnitBuff: //单位Buff
                UnitBuff unitBuff = (UnitBuff)eventBehavior;
                ShowUnitBuffInEditor(unitBuff);
                break;
            case EventBehaviorType.AddFriendForce: //添加友军
                AddFriendForce addFriendForce = (AddFriendForce)eventBehavior;
                ShowAddFriendForceInEditor(addFriendForce);
                break;
            case EventBehaviorType.AddNpc: //添加Npc
                AddNpc addNpc = (AddNpc)eventBehavior;
                ShowAddNpcInEditor(addNpc);
                break;
            case EventBehaviorType.DeleteUnit: //删除单位
                DeleteUnit deleteUnit = (DeleteUnit)eventBehavior;
                ShowDeleteUnitInEditor(deleteUnit);
                break;
            case EventBehaviorType.ShowPlot: //剧情演绎
                ShowPlot showPlot = (ShowPlot)eventBehavior;
                DrawShowPlot(showPlot);
                break;
            case EventBehaviorType.EndLoop: //结束循环
                EndLoop endLoop = (EndLoop)eventBehavior;
                DrawEndLoop(endLoop, eventList);
                break;
            case EventBehaviorType.WaveCount: //波次计数
                WaveCount waveCount = (WaveCount)eventBehavior;
                DrawWaveCount(waveCount);
                break;
        }
    }

    //编辑器中显示触发效果 出怪
    void ShowSpawnMonsterInEditor(SpawnMonster spawnMonster)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("出怪", GUILayout.Width(25));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("标签", GUILayout.Width(25));
        AddTagManagerTagIdToList();
        spawnMonster.tagId = EditorGUILayout.IntPopup(spawnMonster.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(90));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("unitId", GUILayout.Width(32));
        spawnMonster.unitId = EditorGUILayout.IntField(spawnMonster.unitId, GUILayout.Width(60));
        GUILayout.Space(10);
        AddAllSpawnPointToList();
        EditorGUILayout.LabelField("出怪点", GUILayout.Width(37));
        spawnMonster.spawnPoint = EditorGUILayout.IntPopup(spawnMonster.spawnPoint, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(75));

        GUILayout.Space(10);
        EditorGUILayout.LabelField("出怪总数", GUILayout.Width(50));
        spawnMonster.totalNumber = EditorGUILayout.IntField(spawnMonster.totalNumber, GUILayout.Width(50));
        if(spawnMonster.totalNumber < 1)
        {
            spawnMonster.totalNumber = 1; //避免异常，强制总数大于1
        }
        GUILayout.Space(10);
        EditorGUILayout.LabelField("出怪方式", GUILayout.Width(50));
        spawnMonster.spawnType = (SpawnType)EditorGUILayout.EnumPopup(spawnMonster.spawnType, GUILayout.Width(45));
        if(spawnMonster.spawnType == SpawnType.逐次)
        {
            spawnMonster.NumberPerOrder = EditorGUILayout.IntField(spawnMonster.NumberPerOrder, GUILayout.Width(30));
        }
        if(spawnMonster.NumberPerOrder < 1)
        {
            spawnMonster.NumberPerOrder = 1; //避免异常，强制逐次数量大于1
        }
        if(spawnMonster.NumberPerOrder > spawnMonster.totalNumber)
        {
            spawnMonster.NumberPerOrder = spawnMonster.totalNumber; //避免异常，逐次不得超过总数
        }
        if(spawnMonster.spawnType == SpawnType.全部)
        {
            spawnMonster.NumberPerOrder = 1;
        }
        GUILayout.Space(10);
        EditorGUILayout.LabelField("等级", GUILayout.Width(25));
        spawnMonster.lv = EditorGUILayout.IntField(spawnMonster.lv, GUILayout.Width(50));
        if(spawnMonster.lv < 1)
        {
            spawnMonster.lv = 1; //避免异常，强制等级大于1
        }
        GUILayout.Space(10);
        EditorGUILayout.LabelField("死亡事件", GUILayout.Width(50));
        AddAllEventToList();
        spawnMonster.deadEvent = EditorGUILayout.IntPopup(spawnMonster.deadEvent, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 触发事件
    void ShowTriggerEventInEditor(TriggerEvent triggerEvent)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("触发事件", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("事件", GUILayout.Width(25));
        AddAllEventToList();
        triggerEvent.eventNumber = EditorGUILayout.IntPopup(triggerEvent.eventNumber, nameList.ToArray(), optionList.ToArray(),GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 地块控制
    void ShowAreaControlInEditor(AreaControl areaControl)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("地块控制", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("地块", GUILayout.Width(25));
        AddAllAreaIdToList();
        areaControl.areaId = EditorGUILayout.IntPopup(areaControl.areaId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        areaControl.areaState = (AreaState)EditorGUILayout.EnumPopup(areaControl.areaState, GUILayout.Width(45));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 移动目标控制
    void ShowMoveControlInEditor(MoveControl moveControl)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("移动目标控制", GUILayout.Width(75));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("移动目标", GUILayout.Width(50));
        AddAllAreaIdToList();
        moveControl.areaId = EditorGUILayout.IntPopup(moveControl.areaId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        moveControl.controlType = (MoveControlType)EditorGUILayout.EnumPopup(moveControl.controlType, GUILayout.Width(45));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 变量变更
    void ShowVariableChangeInEditor(VariableChange variableChange)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("变量变更", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("变量", GUILayout.Width(25));
        AddAllVariableToList();
        variableChange.variableId = EditorGUILayout.IntPopup(variableChange.variableId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("公式", GUILayout.Width(25));
        variableChange.Formula = EditorGUILayout.TextField(variableChange.Formula, GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 星级控制
    void ShowStarControlInEditor(StarControl starControl)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("星级控制", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("星级", GUILayout.Width(25));

        nameList.Clear();
        optionList.Clear();
        for(int i = 0; i < 3; i++)
        {
            nameList.Add($"第{i + 1}星");
            optionList.Add(i + 1);
        }

        starControl.starLevel = EditorGUILayout.IntPopup(starControl.starLevel, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(55));
        starControl.isMatch = (StarState)EditorGUILayout.EnumPopup(starControl.isMatch, GUILayout.Width(57));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 触发技能
    void ShowShowAbilityInEditor(ShowAbility showAbility)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("触发技能", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("标签", GUILayout.Width(25));
        AddFriendAndMonsterTagIdToList();
        showAbility.tagId = EditorGUILayout.IntPopup(showAbility.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("技能编号", GUILayout.Width(50));
        showAbility.abilityId = EditorGUILayout.IntField(showAbility.abilityId, GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 地块Buff
    void ShowAreaBuffInEditor(AreaBuff areaBuff)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("地块Buff", GUILayout.Width(50));
        GUILayout.Space(10);
        AddAllAreaIdToList();
        EditorGUILayout.LabelField("地块", GUILayout.Width(25));
        areaBuff.areaId = EditorGUILayout.IntPopup(areaBuff.areaId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(70)); //地块下拉菜单
        areaBuff.buffAction = (BuffAction)EditorGUILayout.EnumPopup(areaBuff.buffAction, GUILayout.Width(45));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("目标", GUILayout.Width(26));
        EditorGUI.BeginChangeCheck();
        areaBuff.buffTarget = (BuffTarget)EditorGUILayout.EnumPopup(areaBuff.buffTarget, GUILayout.Width(57)); //目标下拉菜单
        if(EditorGUI.EndChangeCheck())
        {
            areaBuff.subType = 1;
        }
        nameList.Clear();
        optionList.Clear();
        if(areaBuff.buffTarget == BuffTarget.全部 || areaBuff.buffTarget == BuffTarget.友军 || areaBuff.buffTarget == BuffTarget.第三方) //目标为全部或者友军或者第三方
        {
            nameList.Add("全部");
            optionList.Add(100);
        }
        if(areaBuff.buffTarget == BuffTarget.我方) //目标为我方
        {
            nameList.Add("全部");
            nameList.Add("建筑");
            nameList.Add("英雄");
            optionList.Add(100);
            optionList.Add(3);
            optionList.Add(1);
        }
        if(areaBuff.buffTarget == BuffTarget.敌方) //目标为敌方
        {
            nameList.Add("全部");
            nameList.Add("小怪");
            nameList.Add("精英");
            nameList.Add("Boss");
            optionList.Add(100);
            optionList.Add(2);
            optionList.Add(5);
            optionList.Add(1);
        }
        areaBuff.subType = EditorGUILayout.IntPopup(areaBuff.subType, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(50)); //目标的子类型
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Buff编号", GUILayout.Width(50));
        areaBuff.buffId = EditorGUILayout.IntField(areaBuff.buffId, GUILayout.Width(80)); //Buff编号输入框
        GUILayout.Space(10);
        EditorGUILayout.LabelField("音频名", GUILayout.Width(40));
        areaBuff.audioName = EditorGUILayout.TextField(areaBuff.audioName, GUILayout.Width(90)); //音频名输入框
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 单位Buff
    void ShowUnitBuffInEditor(UnitBuff unitBuff)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("单位Buff", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("生效目标", GUILayout.Width(50));
        EditorGUI.BeginChangeCheck();
        unitBuff.buffTarget = (BuffTarget)EditorGUILayout.EnumPopup(unitBuff.buffTarget, GUILayout.Width(57)); //目标下拉菜单
        if(EditorGUI.EndChangeCheck())
        {
            unitBuff.subType = 1;
        }
        nameList.Clear();
        optionList.Clear();
        if(unitBuff.buffTarget == BuffTarget.全部 ) //目标为全部
        {
            nameList.Add("全部");
            optionList.Add(100);
        }
        if(unitBuff.buffTarget == BuffTarget.我方) //目标为我方
        {
            nameList.Add("全部");
            nameList.Add("建筑");
            nameList.Add("英雄");
            optionList.Add(100);
            optionList.Add(3);
            optionList.Add(1);
        }
        if(unitBuff.buffTarget == BuffTarget.敌方) //目标为敌方
        {
            nameList.Add("全部");
            nameList.Add("小怪");
            nameList.Add("精英");
            nameList.Add("Boss");
            nameList.Add("指定");
            optionList.Add(100);
            optionList.Add(2);
            optionList.Add(5);
            optionList.Add(1);
            optionList.Add(101);
        }
        if(unitBuff.buffTarget == BuffTarget.友军 || unitBuff.buffTarget == BuffTarget.第三方) //目标为友军或者第三方
        {
            nameList.Add("全部");
            nameList.Add("指定");
            optionList.Add(100);
            optionList.Add(101);
        }
        unitBuff.subType = EditorGUILayout.IntPopup(unitBuff.subType, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(50)); //目标的子类型
        
        if(unitBuff.subType == 101) //如果子类型是"指定"
        {
            GUILayout.Space(10);
            AddFriendAndMonsterTagIdToList();
            EditorGUILayout.LabelField("标签", GUILayout.Width(25));
            unitBuff.tagId = EditorGUILayout.IntPopup(unitBuff.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        }
        unitBuff.buffAction = (BuffAction)EditorGUILayout.EnumPopup(unitBuff.buffAction, GUILayout.Width(45));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Buff编号", GUILayout.Width(50));
        unitBuff.buffId = EditorGUILayout.IntField(unitBuff.buffId, GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("音频名", GUILayout.Width(40));
        unitBuff.audioName = EditorGUILayout.TextField(unitBuff.audioName, GUILayout.Width(90));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 添加友军
    void ShowAddFriendForceInEditor(AddFriendForce addFriendForce)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("添加友军", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("友军标签", GUILayout.Width(50));
        nameList.Clear();
        optionList.Clear();
        curStage.mapInfo.mFriendForce_List.ForEach((x) =>
        {
            nameList.Add(curStage.tagManager.tags[x.tagId].Des);
            optionList.Add(x.tagId);
        });
        addFriendForce.tagId = EditorGUILayout.IntPopup(addFriendForce.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 添加Npc
    void ShowAddNpcInEditor(AddNpc addNpc)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("添加Npc", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Npc标签", GUILayout.Width(50));
        nameList.Clear();
        optionList.Clear();
        curStage.mapInfo.mNpc_List.ForEach((x) =>
        {
            nameList.Add(curStage.tagManager.tags[x.tagId].Des);
            optionList.Add(x.tagId);
        });
        addNpc.tagId = EditorGUILayout.IntPopup(addNpc.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 删除单位
    void ShowDeleteUnitInEditor(DeleteUnit deleteUnit)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("删除单位", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("标签", GUILayout.Width(25));
        deleteUnit.tagId = EditorGUILayout.IntPopup(deleteUnit.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 剧情演绎
    void DrawShowPlot(ShowPlot showPlot)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("剧情演绎", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("剧情编号", GUILayout.Width(50));
        showPlot.plotId = EditorGUILayout.IntField(showPlot.plotId, GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("音频名", GUILayout.Width(40));
        showPlot.audioName = EditorGUILayout.TextField(showPlot.audioName, GUILayout.Width(90));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 结束循环
    void DrawEndLoop(EndLoop endLoop, List<EventData> eventList)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("结束循环", GUILayout.Width(50));
        GUILayout.Space(10);

        nameList.Clear();
        optionList.Clear();
        eventList.ForEach((e) =>
        {
            if(e.isMulti && e.isLoop) //多条件事件并且是"循环"
            {
                nameList.Add(e.eventDes);
                optionList.Add(e.eventId);
            }
        });
        EditorGUILayout.LabelField("多条件事件", GUILayout.Width(62));
        endLoop.eventId = EditorGUILayout.IntPopup(endLoop.eventId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    //编辑器中显示触发效果 波次计数（波次计数仅在全局事件中出现）
    void DrawWaveCount(WaveCount waveCount)
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("波次计数", GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("当前波次计数增加", GUILayout.Width(100));
        waveCount.count = EditorGUILayout.IntField(waveCount.count, GUILayout.Width(80));
        GUILayout.EndHorizontal();
    }
    #endregion

    #region 地图编辑 编辑器工具部分
    //绘制编辑器子菜单 地图编辑
    void ShowMapEditor(MapEditor info)
    {
        EditorGUI.BeginChangeCheck();
        info.createMode = (CreateMode)GUILayout.Toolbar((int)info.createMode, info.createDes);
        if(EditorGUI.EndChangeCheck())
        {
            GUI.FocusControl(null);
        }
        GUILayout.Space(5);
        switch(info.createMode)
        {
            case CreateMode.Camera: //摄像机
                EditorDrawCamera(info.Camera);
                break;
            case CreateMode.BirthPoint: //基地
                EditorDrawBirthPoint(info.birthPoint);
                break;
            case CreateMode.BenchPosition: //本阵位置
                EditorDrawBenchPosition(info.benchPosition);
                break;
            case CreateMode.SpawnPoint: //出怪点
                EditorDrawSpawnPoint(info);
                break;
            case CreateMode.Area: //地块
                EditorDrawArea(info);
                break;
            case CreateMode.Npc: //Npc
                EditorDrawNpc(info);
                break;
            case CreateMode.Monster: //怪物
                EditorDrawMonster(info);
                break;
            case CreateMode.FriendForce: //友军
                EditorDrawFriendForce(info);
                break;
        }
    }

    //编辑器界面UI 地图编辑 摄像机
    void EditorDrawCamera(MapCamera p)
    {
        GameObject camTarget = GameObject.Find("CamTarget");
        GameObject CMVCam1 = GameObject.Find("CMVCam1");
        if(camTarget == null || CMVCam1 == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", "CN EntryErrorIconSmall");
            GUILayout.Label("当前场景没有名为\"CamTarget\"或者\"CMVCam1\"的对象！", "CN StatusError");
            GUILayout.EndHorizontal();
            return;
        }

        GUILayout.Label("摄像机", "WarningOverlay");
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        camTarget.transform.position = EditorGUILayout.Vector3Field("", camTarget.transform.position, GUILayout.Width(250));
        p.pos = JsonVector3.UnityV3ToJsonV3(camTarget.transform.position);
        GUILayout.Space(20);
        EditorGUILayout.LabelField("旋转（y轴）", GUILayout.Width(65));
        nameList.Clear();
        optionList.Clear();
        nameList.Add("0");
        nameList.Add("90");
        nameList.Add("180");
        nameList.Add("270");
        optionList.Add(0);
        optionList.Add(90);
        optionList.Add(180);
        optionList.Add(270);
        p.rot.x = CMVCam1.transform.rotation.eulerAngles.x;
        p.rot.y = EditorGUILayout.IntPopup((int)p.rot.y, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(50));
        p.rot.z = CMVCam1.transform.rotation.eulerAngles.z;
        p.rot.x = float.Parse(p.rot.x.ToString("F2"));
        p.rot.y = float.Parse(p.rot.y.ToString("F2"));
        p.rot.z = float.Parse(p.rot.z.ToString("F2"));
        GUILayout.Space(100);
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos));
        }
        GUILayout.EndHorizontal();
    }

    //编辑器界面UI 地图编辑 基地点
    void EditorDrawBirthPoint(MapBirthPoint p)
    {
        GUILayout.Label("基地", "WarningOverlay");
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("模型Id", GUILayout.Width(35));
        p.modelId = EditorGUILayout.IntField(p.modelId, GUILayout.Width(80));
        GUILayout.Space(20);
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        p.pos = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.pos), GUILayout.Width(250)));
        p.pos.x = float.Parse(p.pos.x.ToString("F2")); //ToSting(F2),保留两位小数。float.Parse()将string转换为float
        p.pos.y = float.Parse(p.pos.y.ToString("F2"));
        p.pos.z = float.Parse(p.pos.z.ToString("F2"));
        GUILayout.Space(20);
        EditorGUILayout.LabelField("朝向", GUILayout.Width(25));
        p.rot.y = EditorGUILayout.FloatField(p.rot.y, GUILayout.Width(50));
        p.rot.y = float.Parse(p.rot.y.ToString("F2"));
        GUILayout.Space(100);
        if(!p.isUseAssist)
        {
            if(GUILayout.Button("设置坐标", "sv_label_3", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("请点击场景，设置基地坐标"));
                p.isUseAssist = true;
            }
        }
        else
        {
            if(GUILayout.Button("完成设置", "sv_label_6", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("完成基地坐标设置"));
                p.isUseAssist = false;
            }
        }
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos)); //lastActiveSceneView最近获得焦点的SceneView  LookAt聚焦到一个目标
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// //编辑器界面UI 地图编辑 绘制本阵位置
    /// </summary>
    void EditorDrawBenchPosition(MapBenchPosition p)
    {
        GUILayout.Label("本阵位置", "WarningOverlay");
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        p.pos = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.pos), GUILayout.Width(250)));
        p.pos.x = float.Parse(p.pos.x.ToString("F2"));
        p.pos.y = float.Parse(p.pos.y.ToString("F2"));
        p.pos.z = float.Parse(p.pos.z.ToString("F2"));
        GUILayout.Space(15);
        EditorGUILayout.LabelField("旋转", GUILayout.Width(25));
        p.rot = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.rot), GUILayout.Width(250)));
        p.rot.x = float.Parse(p.rot.x.ToString("F2"));
        p.rot.y = float.Parse(p.rot.y.ToString("F2"));
        p.rot.z = float.Parse(p.rot.z.ToString("F2"));
        GUILayout.Space(20);
        if(!p.isUseAssist)
        {
            if(GUILayout.Button("设置坐标", "sv_label_3", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("请点击场景，设置本阵位置"));
                p.isUseAssist = true;
            }
        }
        else
        {
            if(GUILayout.Button("完成设置", "sv_label_6", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("完成本阵位置设置"));
                p.isUseAssist = false;
            }
        }
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos)); //lastActiveSceneView最近获得焦点的SceneView  LookAt聚焦到一个目标
        }
        GUILayout.EndHorizontal();
    }

    //编辑器界面UI 地图编辑 绘制出怪点
    void EditorDrawSpawnPoint(MapEditor info)
    {
        GUILayout.Label("出怪点", "WarningOverlay");
        if(GUILayout.Button("添加出怪点", "LargeButton", GUILayout.Width(100)))
        {
            int listCount = info.mSpawnPoint_List.Count;
            info.mSpawnPoint_List.Add(new MapSpawnPoint(listCount));
            ShowNotification(new GUIContent("已添加出怪点"));
        }
        info.spawnPointScrollPos = EditorGUILayout.BeginScrollView(info.spawnPointScrollPos);
        for(int i = 0; i < info.mSpawnPoint_List.Count; i++)
        {
            _drawContent_SpawnPoint(info.mSpawnPoint_List[i]); //绘制每一条出怪点信息
        }
        EditorGUILayout.EndScrollView();
    }

    //编辑器界面UI 地图编辑 绘制出怪点详细信息
    void _drawContent_SpawnPoint(MapSpawnPoint p)
    {
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("序号 " + p.Id, GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("unitId", GUILayout.Width(32));
        p.unitId = EditorGUILayout.IntField(p.unitId, GUILayout.Width(80));
        GUILayout.Space(15);
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        p.pos = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.pos), GUILayout.Width(250)));
        p.pos.x = float.Parse(p.pos.x.ToString("F2")); //ToSting(F2),保留两位小数。float.Parse()将string转换为float
        p.pos.y = float.Parse(p.pos.y.ToString("F2"));
        p.pos.z = float.Parse(p.pos.z.ToString("F2"));
        GUILayout.Space(15);
        EditorGUILayout.LabelField("朝向", GUILayout.Width(25));
        p.rot.y = EditorGUILayout.FloatField(p.rot.y, GUILayout.Width(50));
        p.rot.y = float.Parse(p.rot.y.ToString("F2"));
        GUILayout.Space(15);
        EditorGUILayout.LabelField("半径", GUILayout.Width(25));
        p.radius = EditorGUILayout.FloatField(p.radius, GUILayout.Width(50));
        p.radius = float.Parse(p.radius.ToString("F2"));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("阵营:", GUILayout.Width(30));
        p.faction = (Faction)EditorGUILayout.EnumPopup(p.faction, GUILayout.Width(45));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("死亡事件", GUILayout.Width(50));
        AddAllEventToList();
        p.deadEvent = EditorGUILayout.IntPopup(p.deadEvent, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(120));
        GUILayout.Space(100);
        if(!p.isUseAssist)
        {
            if(GUILayout.Button("设置坐标", "sv_label_3", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("请点击场景，设置出怪点坐标"));
                p.isUseAssist = true;
            }
        }
        else
        {
            if(GUILayout.Button("完成设置", "sv_label_6", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("完成出怪点坐标设置"));
                p.isUseAssist = false;
            }
        }
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos));
        }
        if(GUILayout.Button("删除", GUILayout.Width(100)))
        {
            curStage.mapInfo.mSpawnPoint_List.Remove(p); //删除列表元素
            int i = 0;
            curStage.mapInfo.mSpawnPoint_List.ForEach((p) => //重新设置序号
            {
                p.Id = i++;
            });
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 添加所有出怪点到nameList与optionList中
    /// </summary>
    void AddAllSpawnPointToList()
    {
        nameList.Clear();
        optionList.Clear();
        curStage.mapInfo.mSpawnPoint_List.ForEach((p) =>
        {
            nameList.Add("出怪点" + p.Id);
            optionList.Add(p.Id);
        });
    }

    //编辑器界面UI 地图编辑 绘制地块
    void EditorDrawArea(MapEditor info)
    {
        GUILayout.Label("地块", "WarningOverlay");
        if(GUILayout.Button("添加地块", "LargeButton", GUILayout.Width(100)))
        {
            int listCount = info.mArea_List.Count;
            info.mArea_List.Add(new MapArea(listCount));
            ShowNotification(new GUIContent("已添加地块"));
        }
        info.areaScrollPos = EditorGUILayout.BeginScrollView(info.areaScrollPos);
        for(int i = 0; i < info.mArea_List.Count; i++)
        {
            _drawContent_MapArea(info.mArea_List[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    //编辑器界面UI 地图编辑 绘制地块详细信息
    void _drawContent_MapArea(MapArea p)
    {
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("序号 " + p.Id, GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("特效Id", GUILayout.Width(35));
        p.effectId = EditorGUILayout.IntField(p.effectId, GUILayout.Width(60));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        p.pos = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.pos), GUILayout.Width(250)));
        p.pos.x = float.Parse(p.pos.x.ToString("F2"));
        p.pos.y = float.Parse(p.pos.y.ToString("F2"));
        p.pos.z = float.Parse(p.pos.z.ToString("F2"));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("半径", GUILayout.Width(25));
        p.radius = EditorGUILayout.FloatField(p.radius, GUILayout.Width(50));
        p.radius = float.Parse(p.radius.ToString("F2"));
        GUILayout.Space(10);
        p.state = EditorGUILayout.ToggleLeft("是否启用", p.state, GUILayout.Width(80));
        GUILayout.Space(150);
        if(!p.isUseAssist)
        {
            if(GUILayout.Button("设置坐标", "sv_label_3", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("请点击场景，设置坐标"));
                p.isUseAssist = true;
            }
        }
        else
        {
            if(GUILayout.Button("完成设置", "sv_label_6", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("完成设置"));
                p.isUseAssist = false;
            }
        }
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos));
        }
        if(GUILayout.Button("删除", GUILayout.Width(100)))
        {
            curStage.mapInfo.mArea_List.Remove(p);
            int i = 0;
            curStage.mapInfo.mArea_List.ForEach((p) => //重新设置序号
            {
                p.Id = i++;
            });
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 添加所有地块的Id到nameList与optionList中
    /// </summary>
    void AddAllAreaIdToList()
    {
        nameList.Clear();
        optionList.Clear();
        curStage.mapInfo.mArea_List.ForEach((x) =>
        {
            nameList.Add("地块" + x.Id);
            optionList.Add(x.Id);
        });
    }

    //编辑器界面UI 地图编辑 绘制Npc
    void EditorDrawNpc(MapEditor info)
    {
        GUILayout.Label("Npc", "WarningOverlay");
        if(GUILayout.Button("添加Npc", "LargeButton", GUILayout.Width(100)))
        {
            int listCount = info.mNpc_List.Count;
            info.mNpc_List.Add(new MapNpc(listCount));
            ShowNotification(new GUIContent("已添加Npc"));
        }
        info.npcScrollPos = EditorGUILayout.BeginScrollView(info.npcScrollPos);
        for(int i = 0; i < info.mNpc_List.Count; i++)
        {
            _drawContent_MapNpc(info.mNpc_List[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    //编辑器界面UI 地图编辑 绘制Npc详细信息
    void _drawContent_MapNpc(MapNpc p)
    {
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("序号 " + p.Id, GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("标签", GUILayout.Width(25));
        AddTagManagerTagIdToList();
        p.tagId = EditorGUILayout.IntPopup(p.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("unitId", GUILayout.Width(32));
        p.unitId = EditorGUILayout.IntField(p.unitId, GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        p.pos = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.pos), GUILayout.Width(250)));
        p.pos.x = float.Parse(p.pos.x.ToString("F2"));
        p.pos.y = float.Parse(p.pos.y.ToString("F2"));
        p.pos.z = float.Parse(p.pos.z.ToString("F2"));
        GUILayout.Space(15);
        EditorGUILayout.LabelField("朝向", GUILayout.Width(25));
        p.rot.y = EditorGUILayout.FloatField(p.rot.y, GUILayout.Width(50));
        p.rot.y = float.Parse(p.rot.y.ToString("F2"));
        GUILayout.Space(20);
        p.canInteract = EditorGUILayout.ToggleLeft("可交互", p.canInteract, GUILayout.Width(60));
        GUILayout.Space(10);
        p.show = EditorGUILayout.ToggleLeft("初始可见", p.show, GUILayout.Width(70));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("响应事件", GUILayout.Width(50));
        AddAllEventToList();
        p.eventId = EditorGUILayout.IntPopup(p.eventId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.Space(60);
        if(!p.isUseAssist)
        {
            if(GUILayout.Button("设置坐标", "sv_label_3", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("请点击场景，设置Npc坐标"));
                p.isUseAssist = true;
            }
        }
        else
        {
            if(GUILayout.Button("完成设置", "sv_label_6", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("完成设置"));
                p.isUseAssist = false;
            }
        }
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos));
        }
        if(GUILayout.Button("删除", GUILayout.Width(100)))
        {
            curStage.mapInfo.mNpc_List.Remove(p);
            int i = 0;
            curStage.mapInfo.mNpc_List.ForEach((p) => //重新设置序号
            {
                p.Id = i++;
            });
        }
        GUILayout.EndHorizontal();
    }

    //编辑器界面UI 地图编辑 绘制怪物
    void EditorDrawMonster(MapEditor info)
    {
        GUILayout.Label("怪物", "WarningOverlay");
        if(GUILayout.Button("添加怪物", "LargeButton", GUILayout.Width(100)))
        {
            int listCount = info.mMonster_List.Count;
            info.mMonster_List.Add(new MapMonster(listCount));
            ShowNotification(new GUIContent("已添加怪物"));
        }
        info.monsterScrollPos = EditorGUILayout.BeginScrollView(info.monsterScrollPos);
        for(int i = 0; i < info.mMonster_List.Count; i++)
        {
            _drawContent_MapMonster(info.mMonster_List[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    //编辑器界面UI 地图编辑 绘制怪物详细信息
    void _drawContent_MapMonster(MapMonster p)
    {
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("序号 " + p.Id, GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("标签", GUILayout.Width(25));
        AddTagManagerTagIdToList();
        p.tagId = EditorGUILayout.IntPopup(p.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("unitId", GUILayout.Width(32));
        p.unitId = EditorGUILayout.IntField(p.unitId, GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        p.pos = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.pos), GUILayout.Width(250)));
        p.pos.x = float.Parse(p.pos.x.ToString("F2"));
        p.pos.y = float.Parse(p.pos.y.ToString("F2"));
        p.pos.z = float.Parse(p.pos.z.ToString("F2"));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("朝向", GUILayout.Width(25));
        p.rot.y = EditorGUILayout.FloatField(p.rot.y, GUILayout.Width(50));
        p.rot.y = float.Parse(p.rot.y.ToString("F2"));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("死亡事件", GUILayout.Width(50));
        AddAllEventToList();
        p.deadEvent = EditorGUILayout.IntPopup(p.deadEvent, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.Space(10);
        p.canMove = EditorGUILayout.ToggleLeft("可移动", p.canMove, GUILayout.Width(60));
        p.attackFirst = EditorGUILayout.ToggleLeft("主动攻击", p.attackFirst, GUILayout.Width(70));
        p.beAttacked = EditorGUILayout.ToggleLeft("可受攻击", p.beAttacked, GUILayout.Width(70));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("阵营：", GUILayout.Width(30));
        p.faction = (Faction)EditorGUILayout.EnumPopup(p.faction, GUILayout.Width(45));
        GUILayout.Space(10);
        p.show = EditorGUILayout.ToggleLeft("初始可见", p.show, GUILayout.Width(70));
        GUILayout.Space(10);
        if(!p.isUseAssist)
        {
            if(GUILayout.Button("设置坐标", "sv_label_3", GUILayout.Width(100)))
            {
                ShowNotification(new GUIContent("请点击场景，设置怪物坐标"));
                p.isUseAssist = true;
            }
        }
        else
        {
            if(GUILayout.Button("完成设置", "sv_label_6", GUILayout.Width(100)))
            {
                ShowNotification(new GUIContent("完成设置"));
                p.isUseAssist = false;
            }
        }
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos));
        }
        if(GUILayout.Button("删除", GUILayout.Width(100)))
        {
            curStage.mapInfo.mMonster_List.Remove(p);
            int i = 0;
            curStage.mapInfo.mMonster_List.ForEach((p) => //重新设置序号
            {
                p.Id = i++;
            });
        }
        GUILayout.EndHorizontal();
    }

    //编辑器界面UI 地图编辑 绘制友军
    void EditorDrawFriendForce(MapEditor info)
    {
        GUILayout.Label("友军", "WarningOverlay");
        if(GUILayout.Button("添加友军", "LargeButton", GUILayout.Width(100)))
        {
            int listCount = info.mFriendForce_List.Count;
            info.mFriendForce_List.Add(new MapFriendForce(listCount));
            ShowNotification(new GUIContent("已添加友军"));
        }
        info.friendForceScrollPos = EditorGUILayout.BeginScrollView(info.friendForceScrollPos);
        for(int i = 0; i < info.mFriendForce_List.Count; i++)
        {
            _drawContent_MapFriendForce(info.mFriendForce_List[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    //编辑器界面UI 地图编辑 绘制友军详细信息
    void _drawContent_MapFriendForce(MapFriendForce p)
    {
        GUILayout.Space(3);
        GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
        EditorGUILayout.LabelField("序号 " + p.Id, GUILayout.Width(50));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("标签", GUILayout.Width(25));
        AddTagManagerTagIdToList();
        p.tagId = EditorGUILayout.IntPopup(p.tagId, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("unitId", GUILayout.Width(32));
        p.unitId = EditorGUILayout.IntField(p.unitId, GUILayout.Width(80));
        GUILayout.Space(10);
        EditorGUILayout.LabelField("坐标", GUILayout.Width(25));
        p.pos = JsonVector3.UnityV3ToJsonV3(EditorGUILayout.Vector3Field("", JsonVector3.JsonV3ToUnityV3(p.pos), GUILayout.Width(250)));
        p.pos.x = float.Parse(p.pos.x.ToString("F2"));
        p.pos.y = float.Parse(p.pos.y.ToString("F2"));
        p.pos.z = float.Parse(p.pos.z.ToString("F2"));
        GUILayout.Space(15);
        EditorGUILayout.LabelField("朝向", GUILayout.Width(25));
        p.rot.y = EditorGUILayout.FloatField(p.rot.y, GUILayout.Width(50));
        p.rot.y = float.Parse(p.rot.y.ToString("F2"));
        GUILayout.Space(10);
        AddAllEventToList();
        EditorGUILayout.LabelField("死亡事件", GUILayout.Width(50));
        p.deadEvent = EditorGUILayout.IntPopup(p.deadEvent, nameList.ToArray(), optionList.ToArray(), GUILayout.Width(100));
        GUILayout.Space(10);
        p.canMove = EditorGUILayout.ToggleLeft("可移动", p.canMove, GUILayout.Width(60));
        p.attackFirst = EditorGUILayout.ToggleLeft("主动攻击", p.attackFirst, GUILayout.Width(70));
        p.beAttacked = EditorGUILayout.ToggleLeft("可受攻击", p.beAttacked, GUILayout.Width(70));
        p.show = EditorGUILayout.ToggleLeft("初始可见", p.show, GUILayout.Width(70));
        GUILayout.Space(20);
        if(!p.isUseAssist)
        {
            if(GUILayout.Button("设置坐标", "sv_label_3", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("请点击场景，设置友军坐标"));
                p.isUseAssist = true;
            }
        }
        else
        {
            if(GUILayout.Button("完成设置", "sv_label_6", GUILayout.Width(120)))
            {
                ShowNotification(new GUIContent("完成设置"));
                p.isUseAssist = false;
            }
        }
        if(GUILayout.Button("指向", GUILayout.Width(100)))
        {
            SceneView.lastActiveSceneView.LookAt(JsonVector3.JsonV3ToUnityV3(p.pos));
        }
        if(GUILayout.Button("删除", GUILayout.Width(100)))
        {
            curStage.mapInfo.mFriendForce_List.Remove(p);
            int i = 0;
            curStage.mapInfo.mFriendForce_List.ForEach((p) => //重新设置序号
            {
                p.Id = i++;
            });
        }
        GUILayout.EndHorizontal();
    }
    #endregion

    #region 地图编辑 Scene界面自定义绘制部分
    //Scene界面自定义GUI 绘制
    void CustomSceneGUI(SceneView view)
    {
        if(curStage == null)
            return;

        DrawBirthPointInScene(curStage.mapInfo.birthPoint); //Scene界面 基地
        DrawBenchPositionInScene(curStage.mapInfo.benchPosition); //Scene界面 本阵位置

        //Scene界面 出怪点
        curStage.mapInfo.mSpawnPoint_List.ForEach((p) =>
        {
            Vector3 pos = JsonVector3.JsonV3ToUnityV3(p.pos);
            Vector3 rot = JsonVector3.JsonV3ToUnityV3(p.rot);
            _checkRaycastInEditorScene(ref pos, ref p.isUseAssist);
            Handles.color = Color.white;
            p.radius = Handles.ScaleSlider(p.radius, pos, new Vector3(1f, 0f, 1f), Quaternion.identity, 2f, 1f); //用于控制半径的控制器
            Color color = Color.red;
            color.a = 0.1f;
            _drawCircleInEditor(pos, p.radius, color);
            _drawTransformHandleInEditor(ref pos, ref rot.y, "出怪点 " + p.Id, "sv_label_6");
            p.pos = JsonVector3.UnityV3ToJsonV3(pos);
            p.rot = JsonVector3.UnityV3ToJsonV3(rot);
        });

        //Scene界面 地块
        curStage.mapInfo.mArea_List.ForEach((p) =>
        {
            Vector3 pos = JsonVector3.JsonV3ToUnityV3(p.pos);
            _checkRaycastInEditorScene(ref pos, ref p.isUseAssist);
            Handles.color = Color.white;
            p.radius = Handles.ScaleSlider(p.radius, pos, new Vector3(-1f, 0f, -1f), Quaternion.identity, 2f, 1f); //用于控制半径的控制器
            Color color = Color.green;
            color.a = 0.1f;
            _drawCircleInEditor(pos, p.radius, color);
            _drawPositionHandleInEditor(ref pos, "地块" + p.Id, "sv_label_2");
            p.pos = JsonVector3.UnityV3ToJsonV3(pos);
        });

        //Scene界面 Npc
        curStage.mapInfo.mNpc_List.ForEach((p) =>
        {
            Vector3 pos = JsonVector3.JsonV3ToUnityV3(p.pos);
            Vector3 rot = JsonVector3.JsonV3ToUnityV3(p.rot);
            _checkRaycastInEditorScene(ref pos, ref p.isUseAssist);
            _drawTransformHandleInEditor(ref pos, ref rot.y, "Npc " + p.Id, "sv_label_7");
            p.pos = JsonVector3.UnityV3ToJsonV3(pos);
            p.rot = JsonVector3.UnityV3ToJsonV3(rot);
        });

        //Scene界面 怪物
        curStage.mapInfo.mMonster_List.ForEach((p) =>
        {
            Vector3 pos = JsonVector3.JsonV3ToUnityV3(p.pos);
            Vector3 rot = JsonVector3.JsonV3ToUnityV3(p.rot);
            _checkRaycastInEditorScene(ref pos, ref p.isUseAssist);
            _drawTransformHandleInEditor(ref pos, ref rot.y, "怪物 " + p.Id, "sv_label_5");
            p.pos = JsonVector3.UnityV3ToJsonV3(pos);
            p.rot = JsonVector3.UnityV3ToJsonV3(rot);
        });

        //Scene界面 友军
        curStage.mapInfo.mFriendForce_List.ForEach((p) =>
        {
            Vector3 pos = JsonVector3.JsonV3ToUnityV3(p.pos);
            Vector3 rot = JsonVector3.JsonV3ToUnityV3(p.rot);
            _checkRaycastInEditorScene(ref pos, ref p.isUseAssist);
            _drawTransformHandleInEditor(ref pos, ref rot.y, "友军 " + p.Id, "sv_label_3");
            p.pos = JsonVector3.UnityV3ToJsonV3(pos);
            p.rot = JsonVector3.UnityV3ToJsonV3(rot);
        });

        view.Repaint(); //Scene界面重绘
        this.Repaint(); //工具窗口重绘
    }

    /// <summary>
    /// 在Scene界面中绘制基地的位置
    /// </summary>
    /// <param name="p"></param>
    void DrawBirthPointInScene(MapBirthPoint p)
    {
        Vector3 pos = JsonVector3.JsonV3ToUnityV3(p.pos);
        Vector3 rot = JsonVector3.JsonV3ToUnityV3(p.rot);
        _checkRaycastInEditorScene(ref pos, ref p.isUseAssist);
        _drawTransformHandleInEditor(ref pos, ref rot.y, "基地", "sv_label_0");
        p.pos = JsonVector3.UnityV3ToJsonV3(pos);
        p.rot = JsonVector3.UnityV3ToJsonV3(rot);
    }

    /// <summary>
    /// 在Scene界面中绘制本阵的位置
    /// </summary>
    /// <param name="p"></param>
    void DrawBenchPositionInScene(MapBenchPosition p)
    {
        Vector3 pos = JsonVector3.JsonV3ToUnityV3(p.pos);
        Vector3 rot = JsonVector3.JsonV3ToUnityV3(p.rot);
        _checkRaycastInEditorScene(ref pos, ref p.isUseAssist);
        _drawTransformHandleInEditor(ref pos, ref rot.y, "本阵位置", "sv_label_1");
        p.pos = JsonVector3.UnityV3ToJsonV3(pos);
        p.rot = JsonVector3.UnityV3ToJsonV3(rot);
    }

    /// <summary>
    /// 在Scene界面中绘制位置控制器
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="label">标签文字</param>
    /// <param name="guiStyle">标签的guiStyle</param>
    void _drawPositionHandleInEditor(ref Vector3 pos, string label, string guiStyle)
    {
        pos = Handles.PositionHandle(pos, Quaternion.identity); //位置控制器
        Handles.Label(pos, label, guiStyle); //文本标签
    }

    /// <summary>
    /// 在Scene界面中绘制变换控制器（只含移动和旋转控制器）
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="rotY">旋转（y）</param>
    /// <param name="label">标签</param>
    /// <param name="guiStyle">标签的guiStyle</param>
    void _drawTransformHandleInEditor(ref Vector3 pos, ref float rotY, string label, string guiStyle)
    {
        Quaternion qua = Quaternion.Euler(0f, rotY, 0f);
        Handles.TransformHandle(ref pos, ref qua); //变换控制器
        rotY = qua.eulerAngles.y; //四元数转换为欧拉角，再将y分量赋给rotY
        Handles.Label(pos, label, guiStyle); //文本标签
    }

    /// <summary>
    /// 在Scene界面绘制圆形
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="radius">半径</param>
    /// <param name="color">颜色</param>
    void _drawCircleInEditor(Vector3 pos, float radius, Color color)
    {
        Handles.color = color;
        Handles.DrawSolidDisc(pos, Vector3.up, radius);
    }

    /// <summary>
    /// 射线查询
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="isUseAssist">是否在使用工具</param>
    /// <returns>鼠标点击位置</returns>
    bool _checkRaycastInEditorScene(ref Vector3 pos, ref bool isUseAssist)
    {
        if((Event.current.type == EventType.MouseDown && Event.current.button == 0) && isUseAssist)
        {
            RaycastHit hit;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if(Physics.Raycast(ray, out hit, 1000.0f))
            {
                pos = hit.point;
                isUseAssist = false;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region 事件编辑（多效果事件）
    //绘制编辑器子菜单 事件编辑
    void ShowEventEditor(EventEdit eventEdit)
    {
        GUILayout.Label("事件编辑", "WarningOverlay");
        if(GUILayout.Button("添加事件", "LargeButton", GUILayout.Width(100)))
        {
            int Count = eventEdit.events.Count;
            eventEdit.events.Add(new EventDataBase(Count, $"事件{Count}"));
        }
        eventEdit.scrollPos = EditorGUILayout.BeginScrollView(eventEdit.scrollPos);
        for(int i = 0; i < eventEdit.events.Count; i++)
        {
            _drawContent_EventEdit(eventEdit.events[i]);
        }
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 在"事件编辑"中，显示事件详细信息
    /// </summary>
    void _drawContent_EventEdit(EventDataBase data)
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.BeginHorizontal();
        data.show = EditorGUILayout.Foldout(data.show, "事件" + data.Id);
        GUILayout.Space(10);
        GUILayout.Label("事件描述", GUILayout.Width(52));
        data.des = EditorGUILayout.TextField(data.des, GUILayout.Width(100));
        GUILayout.Space(1100);
        GUILayout.EndHorizontal();
        if(GUILayout.Button("复制事件"))
        {
            int Count = curStage.eventEdit.events.Count;
            EventDataBase newData = DeepCopyByReflection(data);
            newData.Id = Count;
            List<BaseEventBehavior> newList = new List<BaseEventBehavior>();
            for(int i = 0; i < data.eventBehaviors.Count; i++)
            {
                newList.Add(DeepCopyByReflection(data.eventBehaviors[i]));
            }
            newData.eventBehaviors = newList;
            curStage.eventEdit.events.Add(newData);
        }
        if(GUILayout.Button("删除事件"))
        {
            if(EditorUtility.DisplayDialog("警告！", "是否删除当前事件？", "是", "否"))
            {
                curStage.eventEdit.events.Remove(data);
                int k = 0;
                curStage.eventEdit.events.ForEach((x) => //重新设置序号
                {
                    x.Id = k++;
                });
            }
        }
        GUILayout.EndHorizontal();
        if(data.show)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.BeginVertical();
            data.showAddBehavior = EditorGUILayout.Foldout(data.showAddBehavior, "添加效果");
            if(data.showAddBehavior)
            {
                foreach(string des in data.behaviorDes)
                {
                    GUILayout.BeginHorizontal("box");
                    GUILayout.Label(des, GUILayout.Width(100));
                    if(GUILayout.Button("", "OL Plus", GUILayout.Width(30)))
                    {
                        AddEventBehavior(des, data);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //事件信息
            for(int i = 0; i < data.eventBehaviors.Count; i++)
            {
                GUILayout.BeginHorizontal("OL box", GUILayout.Height(25));
                ShowBehaviorInEditor(data.eventBehaviors[i], null);
                GUILayout.FlexibleSpace();
                if(GUILayout.Button("复制效果"))
                {
                    BaseEventBehavior newBehavior = DeepCopyByReflection(data.eventBehaviors[i]);
                    data.eventBehaviors.Add(newBehavior);
                }
                if(GUILayout.Button("", "OL Minus"))
                {
                    data.eventBehaviors.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }
        GUILayout.EndVertical();
    }

    //添加事件触发效果
    void AddEventBehavior(string des, EventDataBase data)
    {
        switch(des)
        {
            case "出怪":
                data.eventBehaviors.Add(new SpawnMonster());
                break;
            case "地块控制":
                data.eventBehaviors.Add(new AreaControl());
                break;
            case "星级控制":
                data.eventBehaviors.Add(new StarControl());
                break;
            case "触发技能":
                data.eventBehaviors.Add(new ShowAbility());
                break;
            case "地块Buff":
                data.eventBehaviors.Add(new AreaBuff());
                break;
            case "单位Buff":
                data.eventBehaviors.Add(new UnitBuff());
                break;
            case "添加友军":
                data.eventBehaviors.Add(new AddFriendForce());
                break;
            case "添加Npc":
                data.eventBehaviors.Add(new AddNpc());
                break;
            case "删除单位":
                data.eventBehaviors.Add(new DeleteUnit());
                break;
            case "剧情演绎":
                data.eventBehaviors.Add(new ShowPlot());
                break;
        }
    }

    /// <summary>
    /// 将事件编辑中所有的事件添加到nameList与optionList中
    /// </summary>
    void AddAllEventToList()
    {
        nameList.Clear();
        optionList.Clear();
        nameList.Add("无事件");
        optionList.Add(-1);
        curStage.eventEdit.events.ForEach((x) =>
        {
            nameList.Add(x.des);
            optionList.Add(x.Id);
        });
    }
    #endregion

    #region 标签管理
    /// <summary>
    /// 编辑器界面显示标签管理
    /// </summary>
    /// <param name="tagManager"></param>
    void ShowTags(TagManager tagManager)
    {
        GUILayout.Label("标签管理", "WarningOverlay");
        if(GUILayout.Button("添加标签", "LargeButton", GUILayout.Width(80)))
        {
            int Count = tagManager.tags.Count;
            tagManager.tags.Add(new Tag(Count, "标签" + Count));
        }
        tagManager.scrollPos = EditorGUILayout.BeginScrollView(tagManager.scrollPos);
        for(int i = 0; i < tagManager.tags.Count; i++)
        {
            Tag tag = tagManager.tags[i];
            GUILayout.BeginHorizontal("box");
            GUILayout.Label(tag.Id.ToString(), GUILayout.Width(20));
            GUILayout.Space(40);
            GUILayout.Label("标签描述", GUILayout.Width(55));
            tag.Des = EditorGUILayout.TextField(tag.Des, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        GUILayout.Label("标签在工具中可添加，但不可删除。", "OL Ping");
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 添加标签管理中的TagId到List中
    /// </summary>
    void AddTagManagerTagIdToList()
    {
        nameList.Clear();
        optionList.Clear();

        curStage.tagManager.tags.ForEach((x) =>
        {
            nameList.Add(x.Des);
            optionList.Add(x.Id);
        });
    }

    /// <summary>
    /// 将友军、怪物中的TagId添加到List中（不包括Npc）
    /// </summary>
    void AddFriendAndMonsterTagIdToList()
    {
        nameList.Clear();
        optionList.Clear();

        curStage.mapInfo.mMonster_List.ForEach((x) =>
        {
            nameList.Add(curStage.tagManager.tags[x.tagId].Des);
            optionList.Add(x.tagId);
        }); //怪物

        curStage.mapInfo.mFriendForce_List.ForEach((x) =>
        {
            nameList.Add(curStage.tagManager.tags[x.tagId].Des);
            optionList.Add(x.tagId);
        }); //友军
    }
    #endregion

    #region 变量管理
    void ShowVariableMagager(VariableManager variableManager)
    {
        GUILayout.Label("变量管理", "WarningOverlay");
        GUILayout.BeginVertical();
        for(int i = 0; i < variableManager.variables.Length; i++)
        {
            Variable v = variableManager.variables[i];
            GUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("序号" + v.Id, GUILayout.Width(50));
            GUILayout.Space(20);
            EditorGUILayout.LabelField("变量描述", GUILayout.Width(50));
            v.des = EditorGUILayout.TextField(v.des, GUILayout.Width(80));
            GUILayout.Space(20);
            EditorGUILayout.LabelField("变量数值", GUILayout.Width(50));
            v.value = EditorGUILayout.IntField(v.value, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }
        GUILayout.Label("变量数量固定，工具中不可添加、删除，变量类型均为整型（整数）。", "OL Ping");
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 将所有变量的信息添加到nameList与optionList中
    /// </summary>
    void AddAllVariableToList()
    {
        nameList.Clear();
        optionList.Clear();

        for(int i = 0; i < curStage.variableManager.variables.Length; i++)
        {
            nameList.Add(curStage.variableManager.variables[i].des);
            optionList.Add(curStage.variableManager.variables[i].Id);
        }
    }
    #endregion

    /// <summary>
    /// 深度拷贝实例（用于复制实例）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCopyByReflection<T>(T obj)
    {
        if(obj is string || obj.GetType().IsValueType)
            return obj;

        object retval = Activator.CreateInstance(obj.GetType()); // 创建实例
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        foreach(var field in fields)
        {
            try
            {
                field.SetValue(retval, DeepCopyByReflection(field.GetValue(obj)));
            }
            catch
            {

            }
        }
        return (T)retval;
    }

    /// <summary>
    /// 新建（清空已经输入的信息）
    /// </summary>
    void CreateNewFile()
    {
        if(EditorUtility.DisplayDialog("警告", "新建将重置所有数据，是否继续？", "是", "否"))
        {
            curStage.Clear();
            ShowNotification(new GUIContent("已重置所有数据"));
        }
        GUI.FocusControl(null);
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    void SaveFile()
    {
        SaveData();
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    void LoadFile()
    {
        string filePath = EditorUtility.OpenFilePanel("请选择文件", defaultSavePath, "json");
        if(string.IsNullOrEmpty(filePath))
            return;
        LoadData(filePath);
        ShowNotification(new GUIContent($"已加载关卡\"{Path.GetFileNameWithoutExtension(filePath)}\"的数据"));
    }

    /// <summary>
    /// 存储数据
    /// </summary>
    void SaveData()
    {
        if(curStage == null)
            return;

        JsonSerializerSettings setting = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented, //设置json格式（有缩进与换行）
        };
        string json = JsonConvert.SerializeObject(curStage, setting); //序列化

        string savePath = EditorUtility.SaveFilePanel("请选择路径", defaultSavePath, curStage.Id + ".json", "json"); //文件保存窗口

        if(string.IsNullOrEmpty(savePath))
            return;

        File.WriteAllText(savePath, json); //写入文件
        AssetDatabase.Refresh();
        ShowNotification(new GUIContent("已保存"));  
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="path">路径</param>
    void LoadData(string path)
    {
        string json = File.ReadAllText(path); //读取文件
        curStage = JsonConvert.DeserializeObject<StageConfig>(json); //反序列化
    }

    void OnEnable()
    {
        if(instance == null)
            instance = (StageEditor)GetWindow(typeof(StageEditor));

        defaultSavePath = Application.dataPath + "/Res/Data/StageData";

        //若无此保存路径则创建
        if(!Directory.Exists(defaultSavePath))
            Directory.CreateDirectory(defaultSavePath);

        SceneView.duringSceneGui += OnSceneFunc;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneFunc;
        instance = null;
    }

    void OnDestory()
    {
        instance = null;
    }
}