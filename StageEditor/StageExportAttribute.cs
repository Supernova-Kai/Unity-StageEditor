//
// 自定义序列化特性
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

[System.AttributeUsage(AttributeTargets.Field)]
public class StageExportAttribute : System.Attribute
{

}