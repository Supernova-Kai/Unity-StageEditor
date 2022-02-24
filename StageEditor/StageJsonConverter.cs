//
// 实现关卡编辑器数据自定义序列化
// create by zhoudikai 2021.12.22
//
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//关卡信息 自定义序列化与反序列化
public class StageInfoConverter : JsonConverter //继承自JsonConverter必须重写下面三个方法
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    //读取Json（反序列化）
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jobj = serializer.Deserialize<JObject>(reader);

        StageInfomation stageInfo = new StageInfomation();


        //胜利条件
        if(jobj.ContainsKey("enemyDead")) //敌方全灭
        {
            stageInfo.enemyDead = new Condition("敌方全灭", true);
        }
        if(jobj.ContainsKey("timeLimit")) //限时守护
        {
            stageInfo.timeLimit = new ConditionWithData("限时守护", true, "时间（秒）", jobj["timeLimit"].Value<int>("limitData"));
        }
        if(jobj.ContainsKey("winCustom")) //胜利自定义
        {
            Custom winCustom = new Custom("自定义", true);
            winCustom.variableId = jobj["winCustom"].Value<int>("variableId");
            winCustom.calculationType = (CalculationType)jobj["winCustom"].Value<int>("calculationType");
            winCustom.number = jobj["winCustom"].Value<int>("number");
            stageInfo.winCustom = winCustom;
        }

        //失败条件
        if(jobj.ContainsKey("baseDestroy")) //基地毁灭
        {
            stageInfo.baseDestroy = new Condition("基地毁灭", true);
        }
        if(jobj.ContainsKey("battleOvertime")) //战斗超时
        {
            stageInfo.battleOvertime = new ConditionWithData("战斗超时", true, "时间（秒）", jobj["battleOvertime"].Value<int>("limitData"));
        }
        if(jobj.ContainsKey("defeatCustom")) //失败自定义
        {
            Custom defeatCustom = new Custom("自定义", true);
            defeatCustom.variableId = jobj["defeatCustom"].Value<int>("variableId");
            defeatCustom.calculationType = (CalculationType)jobj["defeatCustom"].Value<int>("calculationType");
            defeatCustom.number = jobj["defeatCustom"].Value<int>("number");
            stageInfo.defeatCustom = defeatCustom;
        }

        //战场限制
        if(jobj.ContainsKey("battleLimit"))
        {
            BattleLimit battleLimit = new BattleLimit();
            battleLimit.haveLimit = jobj["battleLimit"].Value<bool>("haveLimit");
            battleLimit.limitType = (BattleLimitType)jobj["battleLimit"].Value<int>("limitType");
            if(jobj["battleLimit"].Value<JArray>("battleLimits") != null)
            {
                JArray jArray = jobj["battleLimit"].Value<JArray>("battleLimits");
                foreach(var item in jArray)
                {
                    BaseBattleLimit temp = new BaseBattleLimit(item.Value<string>("des"), item.Value<int>("type"));
                    temp.isSelect = item.Value<bool>("isSelect");
                    battleLimit.battleLimits.Add(temp);
                }
                stageInfo.battleLimit = battleLimit;
            }         
        }

        stageInfo.settleType = (SettleType)jobj.Value<int>("settleType"); //结算方式

        return stageInfo;
    }

    //写入Json（序列化）
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var obj = value as StageInfomation;
        if(obj == null)
            return;

        writer.WriteStartObject();
        foreach (FieldInfo fieldInfo in obj.GetType().GetFields())
        {
            StageExportAttribute att = Attribute.GetCustomAttribute(fieldInfo, typeof(StageExportAttribute)) as StageExportAttribute;
            if (att == null)
                continue;

            if(fieldInfo.Name == "battleLimit") //战场限制
            {
                var battleLimit = fieldInfo.GetValue(obj) as BattleLimit;
                
                writer.WritePropertyName(fieldInfo.Name);
                var tempJobj = WriteBattleLimit(battleLimit);
                tempJobj.WriteTo(writer);
                continue;
            }

            if(fieldInfo.Name == "settleType") //关卡结算方式
            {
                writer.WritePropertyName(fieldInfo.Name);
                writer.WriteValue(fieldInfo.GetValue(obj));
                continue;
            }

            var baseCondition = fieldInfo.GetValue(obj) as BaseCondition;
            if (!baseCondition.isSelected)
                continue;
            
            writer.WritePropertyName(fieldInfo.Name);

            var jobj = WriteCondition(baseCondition);
            jobj.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    public JObject WriteBattleLimit(BattleLimit battleLimit)
    {
        var jobj = new JObject();
        foreach(FieldInfo fieldInfo in battleLimit.GetType().GetFields()) //获取obj中的字段
        {
            StageExportAttribute att = Attribute.GetCustomAttribute(fieldInfo, typeof(StageExportAttribute)) as StageExportAttribute;
            if(att == null)
                continue;

            if(battleLimit.haveLimit == false)
            {
                jobj.Add(fieldInfo.Name, JToken.FromObject(fieldInfo.GetValue(battleLimit)));
                return jobj;
            }
            else
            {
                jobj.Add(fieldInfo.Name, JToken.FromObject(fieldInfo.GetValue(battleLimit)));
            }    
        }
        return jobj;
    }

    public JObject WriteCondition(BaseCondition obj)
    {
        var jobj = new JObject();
        foreach(FieldInfo fieldInfo in obj.GetType().GetFields()) //获取obj中的字段
        {
            StageExportAttribute att = Attribute.GetCustomAttribute(fieldInfo, typeof(StageExportAttribute)) as StageExportAttribute;
            if(att == null)
                continue;

            jobj.Add(fieldInfo.Name, JToken.FromObject(fieldInfo.GetValue(obj)));
        }
        return jobj;
    }
}

public class EventDataConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jobj = serializer.Deserialize<JObject>(reader);
        if(jobj == null)
            return null;

        EventData eventData = new EventData();
        eventData.eventId = jobj.Value<int>("eventId");
        eventData.isMulti = jobj.Value<bool>("isMulti");
        if(eventData.isMulti) //多条件事件
        {
            eventData.isLoop = jobj.Value<bool>("isLoop");
            eventData.eventMode = (ConditionTriggerType)jobj.Value<int>("triggerType");
            eventData.eventDes = jobj.Value<string>("eventDes");
        }
        else //单条件事件
        {
            eventData.isLoop = false;
            eventData.eventMode = ConditionTriggerType.全部;
            eventData.eventDes = String.Empty;
        }

        //事件触发条件
        JArray jArray = jobj.Value<JArray>("eventConditions");
        foreach(JToken condition in jArray)
        {
            if(condition.Value<int>("conditionType") == (int)EventConditionType.TimeTrigger) //时间触发
            {
                eventData.eventConditions.Add(new TimeTrigger(condition.Value<int>("index"), condition.Value<int>("time")));
            }
            if(condition.Value<int>("conditionType") == (int)EventConditionType.AreaTrigger) //地块触发
            {
                AreaTrigger areaTrigger = new AreaTrigger(condition.Value<int>("index"));
                areaTrigger.targetType = (AreaTargetType)condition.Value<int>("targetType");
                areaTrigger.areaIndex = condition.Value<int>("areaIndex");
                areaTrigger.areaAction = (AreaAction)condition.Value<int>("areaAction");
                eventData.eventConditions.Add(areaTrigger);
            }
            if(condition.Value<int>("conditionType") == (int)EventConditionType.CurrentMonsterNum) //当前怪物数量触发
            {
                CurrentMonster currentMonster = new CurrentMonster(condition.Value<int>("index"));
                currentMonster.monsterType = (MonsterType)condition.Value<int>("monsterType");
                currentMonster.monsterId = condition.Value<int>("monsterId");
                currentMonster.pointId = condition.Value<int>("pointId");
                currentMonster.calculationType = (CalculationType)condition.Value<int>("calculationType");
                currentMonster.number = condition.Value<int>("number");
                eventData.eventConditions.Add(currentMonster);
            }
            if(condition.Value<int>("conditionType") == (int)EventConditionType.TotalMonsterNum) //累计怪物数量触发
            {
                TotalMonster totalMonster = new TotalMonster(condition.Value<int>("index"));
                totalMonster.monsterType = (MonsterType)condition.Value<int>("monsterType");
                totalMonster.monsterId = condition.Value<int>("monsterId");
                totalMonster.pointId = condition.Value<int>("pointId");
                totalMonster.calculationType = (CalculationType)condition.Value<int>("calculationType");
                totalMonster.number = condition.Value<int>("number");
                eventData.eventConditions.Add(totalMonster);
            }
            if(condition.Value<int>("conditionType") == (int)EventConditionType.VariableTrigger) //变量触发
            {
                VariableTrigger variableTrigger = new VariableTrigger(condition.Value<int>("index"));
                variableTrigger.variableId = condition.Value<int>("variableId");
                variableTrigger.calculationType = (CalculationType)condition.Value<int>("calculationType");
                variableTrigger.value = condition.Value<int>("value");
                eventData.eventConditions.Add(variableTrigger);
            }
            if(condition.Value<int>("conditionType") == (int)EventConditionType.HpTrigger) //血量触发
            {
                HpTrigger hpTrigger = new HpTrigger(condition.Value<int>("index"));
                hpTrigger.tagId = condition.Value<int>("tagId");
                hpTrigger.calculationType = (CalculationType)condition.Value<int>("calculationType");
                hpTrigger.Hp = condition.Value<int>("Hp");
                eventData.eventConditions.Add(hpTrigger);
            }
        }

        JObject behavior = jobj.Value<JObject>("eventBehavior");

        //事件触发效果
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.Null) //空的触发效果
        {
            NullBehaviour nullBehaviour = new NullBehaviour();
            eventData.eventBehavior = nullBehaviour;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.SpawnMonster) //出怪
        {
            SpawnMonster spawnMonster = new SpawnMonster();
            spawnMonster.tagId = behavior.Value<int>("tagId");
            spawnMonster.unitId = behavior.Value<int>("unitId");
            spawnMonster.spawnType = (SpawnType)behavior.Value<int>("spawnType");
            if((SpawnType)behavior.Value<int>("spawnType") == SpawnType.全部)
            {
                spawnMonster.NumberPerOrder = 0;
            }
            else
            {
                spawnMonster.NumberPerOrder = behavior.Value<int>("NumberPerOrder");
            }
            spawnMonster.spawnPoint = behavior.Value<int>("spawnPoint");
            spawnMonster.totalNumber = behavior.Value<int>("totalNumber");
            spawnMonster.lv = behavior.Value<int>("lv");
            spawnMonster.deadEvent = behavior.Value<int>("deadEvent");
            eventData.eventBehavior = spawnMonster;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.TriggerEvent) //触发事件
        {
            TriggerEvent triggerEvent = new TriggerEvent();
            triggerEvent.eventNumber = behavior.Value<int>("eventNumber");
            eventData.eventBehavior = triggerEvent;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.AreaControl) //地块控制
        {
            AreaControl areaControl = new AreaControl();
            areaControl.areaId = behavior.Value<int>("areaId");
            areaControl.areaState = (AreaState)behavior.Value<int>("areaState");
            eventData.eventBehavior = areaControl;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.MoveControl) //移动目标控制
        {
            MoveControl moveControl = new MoveControl();
            moveControl.areaId = behavior.Value<int>("areaId");
            moveControl.controlType = (MoveControlType)behavior.Value<int>("controlType");
            eventData.eventBehavior = moveControl;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.VariableChange) //变量变更
        {
            VariableChange variableChange = new VariableChange();
            variableChange.variableId = behavior.Value<int>("variableId");
            variableChange.Formula = behavior.Value<string>("Formula");
            eventData.eventBehavior = variableChange;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.StarControl) //星级控制
        {
            StarControl starControl = new StarControl();
            starControl.starLevel = behavior.Value<int>("starLevel");
            starControl.isMatch = (StarState)behavior.Value<int>("isMatch");
            eventData.eventBehavior = starControl;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.ShowAbility) //触发技能
        {
            ShowAbility showAbility = new ShowAbility();
            showAbility.tagId = behavior.Value<int>("tagId");
            showAbility.abilityId = behavior.Value<int>("abilityId");
            eventData.eventBehavior = showAbility;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.AreaBuff) //地块Buff
        {
            AreaBuff areaBuff = new AreaBuff();
            areaBuff.areaId = behavior.Value<int>("areaId");
            areaBuff.buffAction = (BuffAction)behavior.Value<int>("buffAction");
            areaBuff.buffTarget = (BuffTarget)behavior.Value<int>("buffTarget");
            areaBuff.subType = behavior.Value<int>("subType");
            areaBuff.buffId = behavior.Value<int>("buffId");
            areaBuff.audioName = behavior.Value<string>("audioName");
            eventData.eventBehavior = areaBuff;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.UnitBuff) //单位Buff
        {
            UnitBuff unitBuff = new UnitBuff();
            unitBuff.buffTarget = (BuffTarget)behavior.Value<int>("buffTarget");
            unitBuff.subType = behavior.Value<int>("subType");
            unitBuff.tagId = behavior.Value<int>("tagId");
            unitBuff.buffAction = (BuffAction)behavior.Value<int>("buffAction");
            unitBuff.buffId = behavior.Value<int>("buffId");
            unitBuff.audioName = behavior.Value<string>("audioName");
            eventData.eventBehavior = unitBuff;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.AddFriendForce) //添加友军
        {
            AddFriendForce addFriendForce = new AddFriendForce();
            addFriendForce.tagId = behavior.Value<int>("tagId");
            eventData.eventBehavior = addFriendForce;
        }
        if(jobj.Value<int>("behaviorType") == (int)EventBehaviorType.AddNpc) //添加Npc
        {
            AddNpc addNpc = new AddNpc();
            addNpc.tagId = jobj.Value<int>("tagId");
            eventData.eventBehavior = addNpc;
        }
        if(jobj.Value<int>("behaviorType") == (int)EventBehaviorType.DeleteUnit) //删除单位
        {
            DeleteUnit deleteUnit = new DeleteUnit();
            deleteUnit.tagId = jobj.Value<int>("tagId");
            eventData.eventBehavior = deleteUnit;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.ShowPlot) //剧情演绎
        {
            ShowPlot showPlot = new ShowPlot();
            showPlot.plotId = behavior.Value<int>("plotId");
            showPlot.audioName = behavior.Value<string>("audioName");
            eventData.eventBehavior = showPlot;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.EndLoop) //结束循环
        {
            EndLoop endLoop = new EndLoop(behavior.Value<int>("eventId"));
            eventData.eventBehavior = endLoop;
        }
        if(behavior.Value<int>("behaviorType") == (int)EventBehaviorType.WaveCount) //波次计数
        {
            WaveCount waveCount = new WaveCount();
            waveCount.count = behavior.Value<int>("count");
            eventData.eventBehavior = waveCount;
        }

        return eventData;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        EventData eventData = (EventData)value;
        if(eventData == null)
            return;

        writer.WriteStartObject();
        foreach(FieldInfo fieldInfo in eventData.GetType().GetFields())
        {
            if(fieldInfo.Name == "ShowAddBehavior")
            {
                continue;
            }
            if(!eventData.isMulti && fieldInfo.Name == "eventDes")
            {
                continue;
            }
            else
            {
                if(fieldInfo.Name == "eventConditions") //事件触发条件列表
                {
                    List<BaseEventCondition> eventConditions = eventData.eventConditions;
                    writer.WritePropertyName(fieldInfo.Name);
                    writer.WriteStartArray();
                    foreach(BaseEventCondition eventCondition in eventConditions)
                    {
                        writer.WriteStartObject();
                        foreach(FieldInfo conditionField in eventCondition.GetType().GetFields())
                        {

                            //怪物数量触发的特殊处理
                            if(eventCondition.conditionType == EventConditionType.CurrentMonsterNum || eventCondition.conditionType == EventConditionType.TotalMonsterNum)
                            {
                                TotalMonster totalMonster = (TotalMonster)eventCondition;
                                //当怪物类型为All时，不序列化monsterId与pointId字段
                                if((totalMonster.monsterType == MonsterType.全部怪物 && conditionField.Name == "monsterId") || (totalMonster.monsterType == MonsterType.全部怪物 && conditionField.Name == "pointId"))
                                {
                                    continue;
                                }
                                //当怪物类型为Designated时，不序列化pointId字段
                                if(totalMonster.monsterType == MonsterType.指定怪物 && conditionField.Name == "pointId")
                                {
                                    continue;
                                }
                                //当怪物类型为SpawnPoint时，不序列化monsterId字段
                                if(totalMonster.monsterType == MonsterType.出怪点怪物 && conditionField.Name == "monsterId")
                                {
                                    continue;
                                }
                            }
                            
                            writer.WritePropertyName(conditionField.Name);
                            writer.WriteValue(conditionField.GetValue(eventCondition));
                        }
                        writer.WriteEndObject();
                    }
                    writer.WriteEndArray();
                }
                else if(fieldInfo.Name == "eventBehavior") //事件触发效果
                {
                    BaseEventBehavior eventBehavior = eventData.eventBehavior;
                    writer.WritePropertyName(fieldInfo.Name);
                    writer.WriteStartObject();
                    foreach(FieldInfo fieldInfo1 in eventBehavior.GetType().GetFields())
                    {
                        
                        //出怪的特殊处理
                        if(eventBehavior.behaviorType == EventBehaviorType.SpawnMonster)
                        {
                            SpawnMonster spawnMonster = (SpawnMonster)eventBehavior;
                            if(spawnMonster.spawnType == SpawnType.全部 && fieldInfo1.Name == "NumberPerOrder")
                            {
                                continue;
                            }
                        }
                        writer.WritePropertyName(fieldInfo1.Name);
                        writer.WriteValue(fieldInfo1.GetValue(eventBehavior));
                    }
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WritePropertyName(fieldInfo.Name);
                    writer.WriteValue(fieldInfo.GetValue(eventData));
                }
            }
        }
        writer.WriteEndObject();
    }
}

public class TagManagerConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jobj = serializer.Deserialize<JObject>(reader);
        if(jobj == null)
            return null;

        TagManager tagManager = new TagManager();

        List<Tag> tags = new List<Tag>();

        foreach(JToken temp in jobj.Value<JToken>("tags"))
        {
            Tag tag = new Tag(temp.Value<int>("Id"), temp.Value<string>("Des"));
            tags.Add(tag);
        }
        tagManager.tags = tags;
        return tagManager;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        TagManager obj = (TagManager)value;

        if(obj == null)
        {
            return;
        }

        writer.WriteStartObject();
        foreach(FieldInfo fieldInfo in obj.GetType().GetFields())
        {
            StageExportAttribute att = Attribute.GetCustomAttribute(fieldInfo, typeof(StageExportAttribute)) as StageExportAttribute;
            if(att == null)
                continue;

            if(fieldInfo.Name == "tags")
            {
                List<Tag> tags = fieldInfo.GetValue(obj) as List<Tag>;
                var jArray = WriteTags(tags);
                writer.WritePropertyName(fieldInfo.Name);
                jArray.WriteTo(writer);
            }
            else
            {
                writer.WritePropertyName(fieldInfo.Name);
                writer.WriteValue(fieldInfo.GetValue(obj));
            }
        }
        writer.WriteEndObject();
    }

    private static JArray WriteTags(List<Tag> tags)
    {
        JArray tagArray = new JArray();
        foreach(Tag tag in tags)
        {
            tagArray.Add(WriteTag(tag));
        }
        return tagArray;
    }

    private static JObject WriteTag(Tag tag)
    {
        JObject jobj = new JObject();
        foreach(FieldInfo fieldInfo in tag.GetType().GetFields())
        {
            jobj.Add(fieldInfo.Name, JToken.FromObject(fieldInfo.GetValue(tag)));
        }
        return jobj;
    }
}