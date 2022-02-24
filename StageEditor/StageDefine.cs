//
// 关卡编辑器 相关数据
// create by zhoudikai 2021.10.28
//
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

//工具使用的模式
public enum ToolMode
{
    StageInfomation = 0,      //关卡信息
    EventTrigger = 1,         //事件触发
    MapEditor = 2,            //地图编辑
    EventEditor = 3,          //事件编辑
    TagManager = 4,           //标签管理
    VariableManager = 5,      //变量管理
}

//关卡配置类
[Serializable]
public class StageConfig
{
    public int Id = 0; //关卡编号
    public string des = ""; //关卡描述

    public StageInfomation stageInfo = new StageInfomation(); //关卡信息
    public EventTrigger eventTrigger = new EventTrigger(); //事件触发
    public MapEditor mapInfo = new MapEditor(); //地图编辑
    public EventEdit eventEdit = new EventEdit(); //事件编辑
    public TagManager tagManager = new TagManager(); //标签管理
    public VariableManager variableManager = new VariableManager(); //变量管理

    //for editor
    [NonSerialized]
    public string[] toolDes = new string[]{"关卡信息", "事件触发", "地图编辑", "事件编辑", "标签管理", "变量管理"}; //工具模式描述
    [NonSerialized]
    public ToolMode toolMode = ToolMode.StageInfomation; //选择的工具模式

    public void Clear()
    {
        Id = 0;
        des = "";
        stageInfo = new StageInfomation();
        eventTrigger.Clear();
        mapInfo.Clear();
        eventEdit.Clear();
        tagManager = new TagManager();
        variableManager = new VariableManager();

        toolMode = ToolMode.StageInfomation;
    }
}

#region 关卡信息
//关卡信息
[JsonConverter(typeof(StageInfoConverter))]
public class StageInfomation
{
    //胜利条件
    [StageExport]
    public Condition enemyDead = new Condition("敌方全灭", false); //敌方全灭
    [StageExport]
    public ConditionWithData timeLimit = new ConditionWithData("限时守护", false, "时间（秒）", 0); //限时守护
    [StageExport]
    public Custom winCustom = new Custom("自定义", false); //胜利自定义

    //失败条件
    [StageExport]
    public Condition baseDestroy = new Condition("基地毁灭", false); //基地毁灭
    [StageExport]
    public ConditionWithData battleOvertime = new ConditionWithData("战斗超时", false, "时间（秒）", 0); //战斗超时
    [StageExport]
    public Custom defeatCustom = new Custom("自定义", false); //失败自定义

    [StageExport]
    public BattleLimit battleLimit = new BattleLimit(); //战场限制

    [StageExport]
    public SettleType settleType = SettleType.无; //关卡结算方式
}

/// <summary>
/// 关卡结算方式 枚举
/// </summary>
public enum SettleType
{
    无 = 0,
    按时间结算 = 1,
    按波次结算 = 2,
}

//胜利失败条件信息（自定义）
public class Custom : BaseCondition
{
    [StageExport]
    public int variableId = 0; //变量Id
    [StageExport]
    public CalculationType calculationType = CalculationType.小于; //计算方式
    [StageExport]
    public int number = 0; //数值

    public Custom(string _des, bool _isSelected)
    {
        des = _des;
        isSelected = _isSelected;
    }
}

//胜利失败条件信息（Base）
public class BaseCondition
{
    public string des = string.Empty; //条件描述
    public bool isSelected = false; //是否选中
}

//胜利失败条件信息（没有附带数据）
public class Condition : BaseCondition
{
    public Condition(string _des, bool _isSelected)
    {
        des = _des;
        isSelected = _isSelected;
    }
}

//胜利失败条件信息（有附带数据）
public class ConditionWithData : BaseCondition
{
    public string limitDes = string.Empty; //附带数据的描述
    [StageExport]
    public int limitData = 0; //附带数据

    public ConditionWithData(string _des, bool _isSelected, string _limitDes, int _limitData)
    {
        des = _des;
        isSelected = _isSelected; 
        limitDes = _limitDes;
        limitData = _limitData;
    }
}

//战场限制
public class BattleLimit
{
    [StageExport]
    public bool haveLimit = false; //是否有限制
    [StageExport]
    public BattleLimitType limitType = BattleLimitType.职业; //限制的类型
    [StageExport]
    public List<BaseBattleLimit> battleLimits = new List<BaseBattleLimit>();
}

//战场限制类型
public enum BattleLimitType
{
    职业 = 0,
    射程 = 1,
    定位 = 2,
    特性 = 3,
}

public class BaseBattleLimit
{
    public bool isSelect = false; //是否选中
    public string des = string.Empty; //描述
    public int type = 0; //类型

    public BaseBattleLimit(string _des, int _type)
    {
        this.des = _des;
        this.type = _type;
    }
}
#endregion

#region 地图编辑
/// <summary>
/// 自定义Vector3 用于工具和序列化
/// </summary>
public struct JsonVector3
{
    public float x;
    public float y;
    public float z;

    /// <summary>
    /// JsonVector3.zero属性 (0f, 0f, 0f)
    /// </summary>
    public static JsonVector3 zero => zeroVector;
    private static readonly JsonVector3 zeroVector = new JsonVector3(0f, 0f, 0f);

    /// <summary>
    /// JsonVector3.one属性 (1f, 1f, 1f)
    /// </summary>
    public static JsonVector3 one => oneVector;
    private static readonly JsonVector3 oneVector = new JsonVector3(1f, 1f, 1f);

    //构造方法
    public JsonVector3(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    /// <summary>
    /// 将JsonVector3转换为Unity的Vector3
    /// </summary>
    /// <param name="v">JsonVector3</param>
    /// <returns>Unity Vector3</returns>
    public static UnityEngine.Vector3 JsonV3ToUnityV3(JsonVector3 v)
    {
        return new UnityEngine.Vector3(v.x, v.y, v.z);
    }

    /// <summary>
    /// 将Unity的Vector3转换为JsonVector3
    /// </summary>
    /// <param name="v">Unity Vector3</param>
    /// <returns>JsonVector3</returns>
    public static JsonVector3 UnityV3ToJsonV3(UnityEngine.Vector3 v)
    {
        return new JsonVector3(v.x, v.y, v.z);
    }

    /// <summary>
    /// 输出JsonVector3的x,y,z
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("({0}, {1}, {2})", this.x, this.y, this.z);
    }
}

//地图编辑 创建类型
public enum CreateMode
{
    Camera = 0, //摄像机
    BirthPoint = 1, //基地
    BenchPosition = 2, //本阵位置
    SpawnPoint = 3, //出怪点
    Area = 4, //地块
    Npc = 5, //Npc
    Monster = 6, //怪物
    FriendForce = 7, //友军
}

//地图编辑
[Serializable]
public class MapEditor
{
    public MapCamera Camera = new MapCamera(); //摄像机
    public MapBirthPoint birthPoint = new MapBirthPoint(); //基地点
    public MapBenchPosition benchPosition = new MapBenchPosition(); //本阵位置
    public List<MapSpawnPoint> mSpawnPoint_List = new List<MapSpawnPoint>(); //怪物出生点列表
    public List<MapArea> mArea_List = new List<MapArea>(); //地块列表
    public List<MapNpc> mNpc_List = new List<MapNpc>(); //Npc列表
    public List<MapMonster> mMonster_List = new List<MapMonster>(); //怪物列表
    public List<MapFriendForce> mFriendForce_List = new List<MapFriendForce>(); //友军列表

    [NonSerialized]
    public CreateMode createMode = CreateMode.Camera; //用于子菜单页签
    [NonSerialized]
    public string[] createDes = new string[]{"摄像机", "基地", "本阵位置","出怪点", "地块", "Npc", "怪物", "友军"}; //子菜单描述
    [NonSerialized]
    public Vector2 spawnPointScrollPos = Vector2.zero; //用于控制出怪点滚动列表
    [NonSerialized]
    public Vector2 areaScrollPos = Vector2.zero; //用于控制地块滚动列表
    [NonSerialized]
    public Vector2 npcScrollPos = Vector2.zero; //用于控制Npc滚动列表
    [NonSerialized]
    public Vector2 monsterScrollPos = Vector2.zero; //用于控制怪物滚动列表
    [NonSerialized]
    public Vector2 friendForceScrollPos = Vector2.zero; //用于控制友军滚动列表

    public void Clear()
    {
        Camera = new MapCamera();
        birthPoint = new MapBirthPoint();
        benchPosition = new MapBenchPosition();
        mSpawnPoint_List.Clear();
        mArea_List.Clear();
        mNpc_List.Clear();
        mMonster_List.Clear();
        mFriendForce_List.Clear();

        createMode = CreateMode.Camera;
        spawnPointScrollPos = Vector2.zero;
        areaScrollPos = Vector2.zero;
        npcScrollPos = Vector2.zero;
        monsterScrollPos = Vector2.zero;
        friendForceScrollPos = Vector2.zero;
    }
}

//地图编辑 基本原点（基类）
[Serializable]
public class MapBasePoint
{
    [JsonPropertyAttribute(Order = -100)]
    public JsonVector3 pos = JsonVector3.zero; //坐标

    //for editor
    [NonSerialized]
    public bool isUseAssist = false; //是否正在使用辅助（即在Scene界面进行操作）
}

//地图编辑 摄像机
[Serializable]
public class MapCamera : MapBasePoint
{
    public JsonVector3 rot = JsonVector3.zero; //朝向
}

//地图编辑 基地点
[Serializable]
public class MapBirthPoint : MapBasePoint
{
    public int modelId = 0; //模型Id(ModelCfg表)
    public JsonVector3 rot = JsonVector3.zero; //朝向
}

//地图编辑 本阵位置
[Serializable]
public class MapBenchPosition : MapBasePoint
{
    public JsonVector3 rot = JsonVector3.zero; //旋转
}

//地图编辑 出怪点
[Serializable]
public class MapSpawnPoint : MapBasePoint
{
    public JsonVector3 rot = JsonVector3.zero; //朝向
    public int Id = 0; //序号
    public int unitId = 0; //资源Id
    public float radius = 1f; //半径
    public Faction faction = Faction.敌对; //出怪点阵营
    public int deadEvent = -1; //死亡事件

    public MapSpawnPoint(int _Id)
    {
        Id = _Id;
        unitId = 0;
        radius = 1f;
        deadEvent = -1;
    }
}

//地图编辑 地块
[Serializable]
public class MapArea : MapBasePoint
{
    public int Id = 0; //序号
    public int effectId = 0; //特效Id
    public float radius = 1f; //半径
    public bool state = true; //是否启用

    public MapArea(int _Id)
    {
        Id = _Id;
    }
}

//地图编辑 Npc
[Serializable]
public class MapNpc : MapBasePoint
{
    public JsonVector3 rot = JsonVector3.zero; //朝向
    public int Id; //序号
    public int tagId = 0; //标签
    public int unitId = 0; //unitId
    public bool canInteract = false; //是否可以互动
    public int eventId = -1; //响应事件编号
    public bool show = true; //是否在初始时显示（用于区分开始时创建与中途创建的Npc）
    public BuffTarget faction = BuffTarget.第三方; //阵营

    public MapNpc(int _Id)
    {
        Id = _Id;
    }
}

/// <summary>
/// 地图编辑 怪物、出怪点的阵营 枚举
/// </summary>
public enum Faction
{
    敌对 = 1, //敌对
    中立 = 2, //中立
}

//地图编辑 怪物
[Serializable]
public class MapMonster : MapFriendForce
{
    public new Faction faction = Faction.敌对; //阵营

    public MapMonster(int _Id) : base(_Id)
    {
        Id = _Id;
    }
}

//地图编辑 友军
[Serializable]
public class MapFriendForce : MapBasePoint
{
    public JsonVector3 rot = JsonVector3.zero; //朝向
    public int Id; //序号
    public int tagId = 0; //标签
    public int unitId = 0; //unitId
    public bool canMove = false; //是否可以移动
    public bool attackFirst = false; //是否会主动攻击
    public bool beAttacked = false; //是否可受攻击
    public bool show = true; //是否在初始时显示（用于区分开始时创建与中途创建的友军）
    public int deadEvent = -1; //死亡事件
    public BuffTarget faction = BuffTarget.友军; //阵营

    public MapFriendForce(int _Id)
    {
        Id = _Id;
    }
}
#endregion

#region 事件触发
//事件触发
[Serializable]
public class EventTrigger
{
    public List<WaveData> eventWaveData_List = new List<WaveData>(); //波次事件列表
    public List<TotalEvent> totalEvent_List = new List<TotalEvent>(); //全局事件列表

    [NonSerialized]
    public int selectPage = 0; //选择的子菜单页签
    [NonSerialized]
    public string[] toolbarDes = new string[]{"波次事件", "全局事件"}; //子工具栏描述
    [NonSerialized]
    public Vector2 waveDataScrollPos = Vector2.zero; //用于波次事件滚动列表
    [NonSerialized]
    public Vector2 totalEventScrollPos = Vector2.zero; //用于全局事件滚动列表

    public void Clear()
    {
        eventWaveData_List.Clear();
        totalEvent_List.Clear();
        selectPage = 0;
        waveDataScrollPos = Vector2.zero;
        totalEventScrollPos = Vector2.zero;
    }
}

/// <summary>
/// 事件触发 添加波次 波次间隔类型 枚举
/// </summary>
public enum WaveIntervalType
{
    敌方全灭 = 0, //敌方全灭
    开始时间 = 1, //开始时间
}

/// <summary>
/// 事件触发 EventBase
/// 全局事件与波次事件的基类
/// </summary>
public class EventBase
{
    public List<EventData> triggerEvent_List = new List<EventData>(); //触发事件列表

    //for editor
    [NonSerialized]
    public bool ShowEvent = true; //用于控制是否显示事件
    [NonSerialized]
    public bool ShowAddEvent = false; //用于控制是否显示"添加事件"
    [NonSerialized]
    public string[] eventConditionDes = new string[]{"多条件触发", "时间触发", "地块触发", "当前怪物数量触发", "累计怪物数量触发", "变量触发", "血量触发"}; //触发添加的描述

    public EventBase()
    {

    }
}

//事件触发 全局事件
[Serializable]
public class TotalEvent : EventBase
{
    public TotalEvent()
    {

    }
}

//事件触发 波次信息
[Serializable]
public class WaveData : EventBase
{
    public int waveId; //序号
    public WaveIntervalType intervalType = WaveIntervalType.敌方全灭; //间隔类型
    public int intervalTime = 0; //间隔时间

    public WaveData()
    {

    }

    public WaveData(int _waveId)
    {
        waveId = _waveId;
    }
}

//事件触发 波次 事件信息
[JsonConverter(typeof(EventDataConverter))]
public class EventData
{
    public int eventId; //序号
    public bool isMulti = false; //是否是多条件事件
    //以下属性，仅在多条件事件时使用
    public bool isLoop = false; //是否循环
    public ConditionTriggerType eventMode = ConditionTriggerType.全部; //多条件的触发方式
    public string eventDes = string.Empty; //多条件事件的描述

    public List<BaseEventCondition> eventConditions = new List<BaseEventCondition>(); //事件触发条件列表
    public BaseEventBehavior eventBehavior; //事件触发效果

    //only for editor
    public bool ShowAddBehavior = false; //用于控制是否显示"添加效果"

    public EventData()
    {
    
    }

    public EventData(int _eventId)
    {
        eventId = _eventId;
        eventBehavior = new NullBehaviour();
    }
}

/// <summary>
/// 事件条件触发方式（仅在多条件事件时）
/// </summary>
public enum ConditionTriggerType
{
    全部 = 0, //全部
    任一 = 1, //任一
}

//事件触发 事件触发条件 枚举
public enum EventConditionType
{
    TimeTrigger = 1, //时间触发
    AreaTrigger = 2, //地块触发
    CurrentMonsterNum = 3, //当前怪物数量触发
    TotalMonsterNum = 4, //累计怪物数量触发
    VariableTrigger = 5, //变量触发
    HpTrigger = 6, //血量触发
}

//事件触发 事件触发条件（Base）
public class BaseEventCondition
{
    public int index; //序号
    public EventConditionType conditionType; //触发条件类型

    public BaseEventCondition()
    {
    
    }
}

//事件触发条件 时间触发
public class TimeTrigger : BaseEventCondition
{
    public int time = 0; //时间

    public TimeTrigger()
    {
        conditionType = EventConditionType.TimeTrigger;
    }

    public TimeTrigger(int _index, int _time)
    {
        conditionType = EventConditionType.TimeTrigger;
        time = _time;
        index = _index;
    }
}

//事件触发条件 地块触发
public class AreaTrigger : BaseEventCondition
{
    public AreaTargetType targetType = AreaTargetType.全部; //目标类型
    public int areaIndex = 0; //地块Id
    public AreaAction areaAction = AreaAction.进入; //地块动作类型（进入或离开）

    public AreaTrigger()
    {
        conditionType = EventConditionType.AreaTrigger;
    }

    public AreaTrigger(int _index)
    {
        index = _index;
        conditionType = EventConditionType.AreaTrigger;
    }
}

/// <summary>
/// 事件触发条件 地块触发 目标类型 枚举
/// </summary>
public enum AreaTargetType
{
    全部 = 1, //全部
    我方单位 = 2, //我方单位
    敌方单位 = 3, //敌方单位
    中立单位 = 5, //中立单位
}

/// <summary>
/// 地块触发 地块行为类型
/// </summary>
public enum AreaAction
{
    进入 = 0, //进入
    离开 = 1, //离开
}

/// <summary>
/// 事件触发条件 累计怪物数量触发
/// </summary>
public class TotalMonster : BaseEventCondition
{
    public MonsterType monsterType = MonsterType.全部怪物; //怪物数量统计类型
    public int monsterId = 0; //怪物Id（当统计类型为"指定"时的子参数）
    public int pointId = 0; //出怪点Id（当统计类型为"出怪点怪物"时的子参数）
    public CalculationType calculationType; //计算类型
    public int number = 0; //数量

    public TotalMonster()
    {
        conditionType = EventConditionType.TotalMonsterNum;
    }

    public TotalMonster(int _index)
    {
        index = _index;
        conditionType = EventConditionType.TotalMonsterNum;
    }
}

/// <summary>
/// 事件触发条件 当前怪物数量触发
/// </summary>
/// <remarks>
/// 继承自TotalMonster
/// </remarks>
public class CurrentMonster : TotalMonster
{
    public CurrentMonster()
    {
        conditionType = EventConditionType.CurrentMonsterNum;
    }

    public CurrentMonster(int _index) : base(_index)
    {
        index = _index;
        conditionType = EventConditionType.CurrentMonsterNum;
    }
}

//怪物数量触发 怪物统计的类型 枚举
public enum MonsterType
{
    全部怪物 = 0, //全部怪物
    指定怪物 = 1, //指定怪物
    出怪点怪物 = 2, //出怪点怪物
}

//变量计算类型 枚举
public enum CalculationType
{
    小于 = 0,
    小于等于 = 1,
    大于 = 2,
    大于等于 = 3,
    等于 = 4,
}

//事件触发条件 变量触发
public class VariableTrigger : BaseEventCondition
{
    public int variableId = 0; //变量Id
    public CalculationType calculationType; //计算的类型
    public int value; //计算数值

    public VariableTrigger()
    {
        conditionType = EventConditionType.VariableTrigger;
    }

    public VariableTrigger(int _index)
    {
        index = _index;
        conditionType = EventConditionType.VariableTrigger;
    }
}

//事件触发条件 血量触发
public class HpTrigger : BaseEventCondition
{
    public int tagId = 0; //标签
    public CalculationType calculationType = CalculationType.小于; //计算的类型
    public int Hp = 0; //血量

    public HpTrigger()
    {
        conditionType = EventConditionType.HpTrigger;
    }

    public HpTrigger(int _index)
    {
        index = _index;
        conditionType = EventConditionType.HpTrigger;
    }
}

//事件触发 触发效果 枚举
public enum EventBehaviorType
{
    Null = 0, //空触发效果（用于序列化）
    SpawnMonster = 1, //出怪
    TriggerEvent = 2, //触发事件
    AreaControl = 3, //地块控制
    MoveControl = 4, //移动目标控制
    VariableChange = 5, //变量变更
    StarControl = 6, //星级控制
    ShowAbility = 7, //触发技能
    AreaBuff = 8, //地块Buff
    UnitBuff = 9, //单位Buff
    AddFriendForce = 10, //添加友军
    AddNpc = 11, //添加Npc
    DeleteUnit = 12, //删除单位
    ShowPlot = 13, //剧情演绎
    EndLoop = 14, //结束循环
    WaveCount = 15, //波次计数
}

//事件触发 事件触发效果（Base）
public class BaseEventBehavior
{
    public EventBehaviorType behaviorType; //触发效果类型

    public BaseEventBehavior()
    {
    
    }
}

//空的事件触发效果（用于序列化）
public class NullBehaviour : BaseEventBehavior
{
    public NullBehaviour()
    {
        behaviorType = EventBehaviorType.Null;
    }
}

//事件触发效果 出怪
public class SpawnMonster : BaseEventBehavior
{
    public int tagId = 0; //标签
    public int unitId; //资源Id
    public SpawnType spawnType = SpawnType.全部; //出怪方式
    public int spawnPoint; //出怪点
    public int totalNumber = 1; //出怪的总数
    public int NumberPerOrder = 1; //逐次出怪情况下，每次的数量
    public int lv = 1; //等级
    public int deadEvent = -1; //死亡事件

    public SpawnMonster()
    {
        behaviorType = EventBehaviorType.SpawnMonster;
    }
}

//出怪方式
public enum SpawnType
{
    全部 = 0, //全部
    逐次 = 1, //逐次
}

//事件触发效果 触发事件
public class TriggerEvent : BaseEventBehavior
{
    public int eventNumber = -1; //触发事件的Id

    public TriggerEvent()
    {
        behaviorType = EventBehaviorType.TriggerEvent;
    }
}

//事件触发效果 地块控制
public class AreaControl : BaseEventBehavior
{
    public int areaId; //地块编号
    public AreaState areaState = AreaState.启用; //地块状态

    public AreaControl()
    {
        behaviorType = EventBehaviorType.AreaControl;
    }
}

//地块状态
public enum AreaState
{
    启用 = 0, //启用
    禁用 = 1, //禁用
}

//事件触发效果 移动目标控制
public class MoveControl : BaseEventBehavior
{
    public int areaId = 0;
    public MoveControlType controlType = MoveControlType.添加;

    public MoveControl()
    {
        behaviorType = EventBehaviorType.MoveControl;
    }
}

//移动目标控制 操作类型
public enum MoveControlType
{
    添加 = 0, //添加
    取消 = 1, //取消
}

//事件触发效果 变量变更
public class VariableChange : BaseEventBehavior
{
    public int variableId; //变量Id
    public string Formula; //变更公式

    public VariableChange()
    {
        behaviorType = EventBehaviorType.VariableChange;
    }
}

//事件触发效果 星级控制
public class StarControl : BaseEventBehavior
{
    public int starLevel = 1; //星级等级
    public StarState isMatch = StarState.满足; //是否满足

    public StarControl()
    {
        behaviorType = EventBehaviorType.StarControl;
    }
}

//事件触发效果 星级是否满足
public enum StarState
{
    满足 = 0, //满足
    不满足 = 1, //不满足
}

//事件触发效果 触发技能
public class ShowAbility : BaseEventBehavior
{
    public int tagId = 0; //标签
    public int abilityId; //技能编号

    public ShowAbility()
    {
        behaviorType = EventBehaviorType.ShowAbility;
    }
}

//事件触发效果 地块Buff
public class AreaBuff : BaseEventBehavior
{
    public int areaId; //地块编号
    public BuffAction buffAction = BuffAction.添加; //Buff的行为（添加或删除）
    public BuffTarget buffTarget = BuffTarget.全部; //目标类型
    public int subType = 0; //目标的子类型
    public int buffId = 0; //Buff编号
    public string audioName = string.Empty; //音频名

    public AreaBuff()
    {
        behaviorType = EventBehaviorType.AreaBuff;
    }
}

//事件触发效果 地块Buff与单位Buff 操作行为
public enum BuffAction
{
    添加 = 0, //添加
    删除 = 1, //删除
}

//事件触发效果 地块Buff与单位Buff Buff的目标
public enum BuffTarget
{
    全部 = 100, //全部
    我方 = 0, //我方
    敌方 = 1, //敌方
    友军 = 2, //友军
    第三方 = 3, //第三方（中立单位）
}

//事件触发效果 单位Buff
public class UnitBuff : BaseEventBehavior
{
    public BuffTarget buffTarget = BuffTarget.全部; //目标类型
    public int subType = 0; //目标的子类型
    public int tagId = 0; //标签
    public BuffAction buffAction = BuffAction.添加; //Buff的行为（添加或删除）
    public int buffId = 0; //Buff编号
    public string audioName = string.Empty; //音频名

    public UnitBuff()
    {
        behaviorType = EventBehaviorType.UnitBuff;
    }
}

//事件触发效果 添加友军
public class AddFriendForce : BaseEventBehavior
{
    public int tagId = 0; //标签

    public AddFriendForce()
    {
        behaviorType = EventBehaviorType.AddFriendForce;
    }
}

//事件触发效果 添加Npc
public class AddNpc : BaseEventBehavior
{
    public int tagId = 0; //标签（地图编辑中Npc的标签）

    public AddNpc()
    {
        behaviorType= EventBehaviorType.AddNpc;
    }
}

//事件触发效果 删除单位
public class DeleteUnit : BaseEventBehavior
{
    public int tagId = 0; //标签

    public DeleteUnit()
    {
        behaviorType = EventBehaviorType.DeleteUnit;
    }
}

//事件触发效果 剧情演绎
public class ShowPlot : BaseEventBehavior
{
    public int plotId = 0; //剧情演绎编号
    public string audioName = string.Empty; //音频名

    public ShowPlot()
    {
        behaviorType = EventBehaviorType.ShowPlot;
    }
}

//事件触发效果 结束循环
public class EndLoop : BaseEventBehavior
{
    public int eventId = 0; //结束循环的事件的Id

    public EndLoop()
    {
        behaviorType = EventBehaviorType.EndLoop;
    }

    public EndLoop(int _eventId)
    {
        this.eventId = _eventId;
        behaviorType = EventBehaviorType.EndLoop;
    }
}

//事件触发效果 波次计数
public class WaveCount : BaseEventBehavior
{
    public int count = 0; //当前波次计数

    public WaveCount()
    {
        behaviorType = EventBehaviorType.WaveCount;
    }
}
#endregion

#region 事件编辑（多效果事件）
/// <summary>
/// 事件编辑（即多效果事件）
/// </summary>
[Serializable]
public class EventEdit
{
    public List<EventDataBase> events = new List<EventDataBase>();

    [NonSerialized]
    public Vector2 scrollPos = Vector2.zero; //用于控制滚动列表

    public void Clear()
    {
        events.Clear();
        scrollPos = Vector2.zero;
    }
}

/// <summary>
/// 
/// </summary>
[Serializable]
public class EventDataBase
{
    public int Id; //序号
    public string des = string.Empty; //事件描述
    public List<BaseEventBehavior> eventBehaviors = new List<BaseEventBehavior>(); //效果列表

    [NonSerialized]
    public bool show = false; //控制折叠菜单
    [NonSerialized]
    public bool showAddBehavior = false; //控制是否显示"添加效果"
    [NonSerialized]
    public string[] behaviorDes = new string[]{"出怪", "地块控制", "星级控制", "触发技能", "地块Buff", "单位Buff", "添加友军", "添加Npc", "删除单位", "剧情演绎"};

    public EventDataBase()
    {
    
    }

    public EventDataBase(int _Id, string _des)
    {
        this.Id = _Id;
        this.des = _des;
    }
}
#endregion

#region 标签管理
/// <summary>
/// 标签管理
/// </summary>
[JsonConverter(typeof(TagManagerConverter))]
[Serializable]
public class TagManager
{
    [StageExport]
    public List<Tag> tags = new List<Tag>(); //标签列表

    [NonSerialized]
    public Vector2 scrollPos = Vector2.zero; //用于控制滚动列表

    public TagManager()
    {
        this.tags.Add(new Tag(0, "默认标签"));
    }
}

/// <summary>
/// 标签
/// </summary>
[Serializable]
public class Tag
{
    public int Id; //序号
    public string Des = string.Empty; //描述

    public Tag(int _Id, string _Des)
    {
        this.Id = _Id;
        this.Des = _Des;
    }
}
#endregion

#region 变量管理
/// <summary>
/// 变量管理
/// </summary>
[Serializable]
public class VariableManager
{
    public Variable[] variables = new Variable[10]; //存放变量的数值

    public VariableManager()
    {
        for(int i = 0; i < variables.Length; i++)
        {
            variables[i] = new Variable(i, "变量" + i);
        }
    }
}

/// <summary>
/// 变量
/// </summary>
[Serializable]
public class Variable
{
    public int Id; //序号
    public string des; //描述
    public int value = 0; //变量值

    public Variable(int _Id, string _des)
    {
        this.Id = _Id;
        this.des = _des;
    }
}
#endregion