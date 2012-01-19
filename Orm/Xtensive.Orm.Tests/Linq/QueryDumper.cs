// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.03.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm.Tests.Linq
{
  public class QueryDumper
  {
    private int treeDepth;
    private int fillStringLength;
    private bool containsEnumerable;

    private void DumpInternal(IEnumerable query, bool showResults)
    {
      EnumerateAll(query);

      if (showResults) {

      treeDepth = 1;
      fillStringLength = 0;
      containsEnumerable = false;

      var listOfElements = new List<object>();
      foreach (var o in query)
        listOfElements.Add(o);
      if (listOfElements.Count==0) {
        Log.Info("NULL");
        return;
      }

      try {
        var document = new XmlDocument();
        CreateNodeTree(listOfElements, ref document, "Root", document);

        if (!containsEnumerable) {
          CorrectNodeTree(ref document);
          var str = new StringBuilder("|");
          foreach (XmlNode o in document.DocumentElement.ChildNodes[0].ChildNodes) {
            var value = Int32.Parse(o.Attributes["length"].Value);
            fillStringLength = fillStringLength + value;
            str.Append("  ").Append(o.Name).Append(CreateFillString(value - o.Name.Length - 3, ' ')).Append("|");
          }

          var groupHeader = new List<string>();
          groupHeader.Add(CreateFillString(fillStringLength + 1, '*'));
          groupHeader.Add(str.ToString());
          groupHeader.Add(CreateFillString(fillStringLength + 1, '*'));

          if (document.DocumentElement.ChildNodes[0].Attributes["group"]==null) {
            foreach (var s in groupHeader)
              Log.Info(s);
            OutputLog(document, null);
          }
          else
            OutputLog(document, groupHeader);
        }
        else {
          Log.Info("Correct output is impossible.");
//          EnumerateAll(query);
        }
      }
      catch {
        Log.Info("Errors occurred during execution.");
//        EnumerateAll(query);
      }
      }
    }

    public static void Dump(object value)
    {
      Dump(value, false);
    }

    public static void Dump(object value, bool showResults)
    {
      if (value is IEnumerable)
        Dump((IEnumerable) value, showResults);
      else {
        try {
          Dump(new[] {value}, showResults);
        }
        catch {
          Log.Info(value==null ? "NULL" : value.ToString());
        }
      }
    }

    public static void Dump(IEnumerable value)
    {
      Dump(value, false);
    }

    public static void Dump(IEnumerable value, bool showResults)
    {
      var dumper = new QueryDumper();
      dumper.DumpInternal(value, showResults);
    }

    #region Private Length related methods

    private void FillLength(ref XmlDocument document, XmlNode rootNode)
    {
      foreach (XmlNode node in rootNode.ChildNodes) {
        if (!node.HasChildNodes) {
          if (node.Attributes["value"]!=null) {
            var l = node.Attributes["value"].Value.Length + 5;
            if (l > Int32.Parse(node.Attributes["length"].Value) + 5)
              node.Attributes["length"].Value = l.ToString();
            else
              node.Attributes["length"].Value = (Int32.Parse(node.Attributes["length"].Value) + 5).ToString();
          }
        }
        else
          FillLength(ref document, node);
      }
    }

    private void FillLengths(ref XmlDocument document, XmlNodeList nodes)
    {
      foreach (XmlNode node in nodes)
        FillLength(ref document, node);
    }

    private void CorrectMaxLengths(ref XmlDocument document, XmlNodeList nodes)
    {
      var firstNodes = nodes;
      if (nodes[0].HasChildNodes && nodes[0].ParentNode==document.DocumentElement)
        firstNodes = nodes[0].ChildNodes;

      foreach (XmlNode node in firstNodes) {
        if (!node.HasChildNodes) {
          if (node.Attributes["length"]!=null) {
            int currentLength = Int32.Parse(node.Attributes["length"].Value);
            var resultNodes = document.SelectNodes("//" + node.Name);
            foreach (XmlNode o in resultNodes) {
              var attribute = Int32.Parse(o.Attributes["length"].Value);
              if (attribute > currentLength)
                currentLength = attribute;
            }
            foreach (XmlNode o in resultNodes)
              o.Attributes["length"].Value = currentLength.ToString();
          }
        }
        else {
          CorrectMaxLengths(ref document, node.ChildNodes);
        }
      }
    }

    private int FillMaxLengths(ref XmlDocument document, XmlNodeList nodes)
    {
      var sum = 0;
      var correctNodes = nodes;
      if (nodes[0].HasChildNodes && nodes[0].ParentNode==document.DocumentElement)
        correctNodes = nodes[0].ChildNodes;
      foreach (XmlNode o in correctNodes) {
        if (!o.HasChildNodes)
          sum += Int32.Parse(o.Attributes["length"].Value);
        else
          sum += FillMaxLengths(ref document, o.ChildNodes);
      }
      if (correctNodes[0].ParentNode.ParentNode!=document.DocumentElement) {
        var maxLength = Int32.Parse(correctNodes[0].ParentNode.Attributes["length"].Value);
        if (maxLength < sum)
          correctNodes[0].ParentNode.Attributes["length"].Value = sum.ToString();
      }
      return sum;
    }

    private void FillCorrectMaxLengths(ref XmlDocument document, XmlNodeList nodes)
    {
      var correctNodes = nodes;
      if (nodes[0].HasChildNodes && nodes[0].ParentNode==document.DocumentElement)
        correctNodes = nodes[0].ChildNodes;
      foreach (XmlNode node in correctNodes) {
        var allNodes = document.SelectNodes("//" + node.Name);
        foreach (XmlNode o in allNodes)
          o.Attributes["length"].Value = node.Attributes["length"].Value;
        if (node.HasChildNodes)
          FillCorrectMaxLengths(ref document, node.ChildNodes);
      }
    }

    #endregion

    private void CreateNodeTree(List<object> values, ref XmlDocument document, string rootElement, XmlNode parentElement)
    {
      var parentNode = document.CreateElement(rootElement);
      parentElement.AppendChild(parentNode);

      if (rootElement!="Root") {
        containsEnumerable = true;
        var lengthAttribute = document.CreateAttribute("length");
        var depthAttribute = document.CreateAttribute("depth");
        var valueAttribute = document.CreateAttribute("value");
        var enumerableAttribute = document.CreateAttribute("enumerable");
        lengthAttribute.Value = rootElement.Length.ToString();
        depthAttribute.Value = treeDepth.ToString();
        valueAttribute.Value = "NULL";
        enumerableAttribute.Value = "TRUE";
        parentNode.Attributes.Append(lengthAttribute);
        parentNode.Attributes.Append(depthAttribute);
        parentNode.Attributes.Append(valueAttribute);
        parentNode.Attributes.Append(enumerableAttribute);
      }

      int groupIndex = 1;
      int itemIndex = 0;

      foreach (var value in values) {
        treeDepth = 1;
        var depth = 1;
        XmlNode itemNode = document.CreateElement("Item" + itemIndex);

        if (value==null || !value.GetType().IsGenericType || (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition()!=typeof (Grouping<,>))) {
          itemNode = document.CreateElement("Item" + itemIndex);
          itemIndex++;
          parentNode.AppendChild(itemNode);
        }

        if (value==null || ((GetMemberType(value.GetType())==MemberType.Primitive
          || GetMemberType(value.GetType())==MemberType.Unknown) && !value.GetType().IsGenericType)
            || (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition()!=typeof (Grouping<,>))
              && (GetMemberType(value.GetType())==MemberType.Primitive
                || GetMemberType(value.GetType())==MemberType.Unknown))
          depth = AddNode(value, null, ref document, itemNode, depth);

        else if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition()==typeof (Grouping<,>)) {
          var exactValue = (IEnumerable) value;
          foreach (var val in exactValue) {
            itemNode = document.CreateElement("Item" + itemIndex);
            parentNode.AppendChild(itemNode);
            itemIndex++;
            var groupAttribute = document.CreateAttribute("group");
            groupAttribute.Value = groupIndex.ToString();
            var keyAttribute = document.CreateAttribute("key");
            keyAttribute.Value = KeyToString(value.GetType().GetProperty(Orm.WellKnown.KeyFieldName).GetValue(value, null));
            itemNode.Attributes.Append(groupAttribute);
            itemNode.Attributes.Append(keyAttribute);
            var properties = val.GetType().GetProperties();
            foreach (var info in properties)
              depth = AddNode(val, info, ref document, itemNode, depth);
          }
          groupIndex++;
        }

        else {
          var properties = value.GetType().GetProperties();
          foreach (var info in properties)
            depth = AddNode(value, info, ref document, itemNode, depth);
        }
      }
    }

    private void CorrectNodeTree(ref XmlDocument document)
    {
      FillLengths(ref document, document.DocumentElement.ChildNodes);
      CorrectMaxLengths(ref document, document.DocumentElement.ChildNodes);
      FillMaxLengths(ref document, document.DocumentElement.ChildNodes);
      FillCorrectMaxLengths(ref document, document.DocumentElement.ChildNodes);
    }

    private int AddNode(object value, PropertyInfo property, ref XmlDocument document, XmlNode parentNode, int depthValue)
    {
      var depth = depthValue;

      //One column of primitive type

      if (property==null) {
        var propertyName = "CompilationResult";
        XmlElement node = document.CreateElement(propertyName);
        var valueAttribute = document.CreateAttribute("value");
        var lengthAttribute = document.CreateAttribute("length");
        var depthAttribute = document.CreateAttribute("depth");
        lengthAttribute.Value = propertyName.Length.ToString();
        depthAttribute.Value = depth.ToString();
        node.Attributes.Append(lengthAttribute);
        node.Attributes.Append(depthAttribute);
        parentNode.AppendChild(node);
        valueAttribute.Value = ReplaceTabs(value);
        node.Attributes.Append(valueAttribute);
      }

      else {
        if (property.PropertyType.IsGenericType &&
          (property.PropertyType.GetGenericTypeDefinition()==typeof (IQueryable<>)
            || (property.PropertyType.GetGenericTypeDefinition()==typeof (IEnumerable<>)))) {
          var enumerable = (IEnumerable) property.GetValue(value, property.GetIndexParameters());
          var list = new List<object>();
          foreach (var o in enumerable)
            list.Add(o);
          CreateNodeTree(list, ref document, property.Name, parentNode);
        }
        else {
          var memberType = GetMemberType(property.PropertyType);
          XmlElement node = document.CreateElement(property.Name);
          var valueAttribute = document.CreateAttribute("value");
          var lengthAttribute = document.CreateAttribute("length");
          var depthAttribute = document.CreateAttribute("depth");
          lengthAttribute.Value = property.Name.Length.ToString();
          depthAttribute.Value = depth.ToString();
          node.Attributes.Append(lengthAttribute);
          node.Attributes.Append(depthAttribute);

          switch (memberType) {
          case MemberType.Unknown:
            if (ValueIsForOutput(property)) {
              parentNode.AppendChild(node);
              valueAttribute.Value = ReplaceTabs(property.GetValue(value, null));
              node.Attributes.Append(valueAttribute);
              node.Attributes.Append(valueAttribute);
            }
            break;
          case MemberType.Entity:
            parentNode.AppendChild(node);
            var entityValue = property.GetValue(value, null);
            valueAttribute.Value = (entityValue!=null) ? entityValue.GetType().GetProperty(Orm.WellKnown.KeyFieldName).GetValue(entityValue, null).ToString() : "NULL";
            node.Attributes.Append(valueAttribute);
            break;
          case MemberType.Structure:
          case MemberType.Anonymous:
            if (depth==treeDepth)
              treeDepth += 1;
            depth += 1;
            parentNode.AppendChild(node);
            var itemValue = property.GetValue(value, null);
            var itemProperties = itemValue.GetType().GetProperties();
            foreach (var itemProperty in itemProperties)
              AddNode(itemValue, itemProperty, ref document, node, depth);
            depth -= 1;
            break;
          }
        }
      }
      return depth;
    }

    private void OutputLog(XmlDocument document, List<string> groupHeader)
    {
      int currentGroup = 1;
      if (groupHeader!=null) {
        Log.Info(String.Empty);
        Log.Info(CreateFillString(fillStringLength + 1, '*'));
        Log.Info("|  Key = " + document.DocumentElement.ChildNodes[0].Attributes["key"].Value);
        foreach (var s in groupHeader)
          Log.Info(s);
      }

      foreach (XmlNode node in document.DocumentElement.ChildNodes) {
        if (groupHeader!=null && Int32.Parse(node.Attributes["group"].Value) > currentGroup) {
          Log.Info(String.Empty);
          Log.Info(CreateFillString(fillStringLength + 1, '*'));
          Log.Info("|  Key = " + node.Attributes["key"].Value);
          foreach (var s in groupHeader)
            Log.Info(s);
          currentGroup++;
        }

        var nodes = node.Clone().SelectNodes("//*[@depth=1]");
        var helpList = new List<Pair<XmlNode, int>>();
        var drawList = new List<Pair<XmlNode, int>>();

        for (int i = 0; i < nodes.Count; i++) {
          if (!nodes[i].HasChildNodes)
            helpList.Add(new Pair<XmlNode, int>(nodes[i], 0));
          else {
            var length = 0;
            for (int j = i - 1; j >= 0; j = j - 1) {
              if (nodes[j].HasChildNodes)
                break;
              length += Int32.Parse(nodes[j].Attributes["length"].Value);
            }
            helpList.Add(new Pair<XmlNode, int>(nodes[i], length));
          }
          drawList.Add(new Pair<XmlNode, int>(nodes[i], 0));
        }
        DrawSingleLine(drawList);

        for (int i = 2; i <= treeDepth; i++) {
          drawList.Clear();
          nodes = node.Clone().SelectNodes("//*[@depth=" + i + "]");
          for (int j = 0; j < nodes.Count; j++) {
            var val = helpList.Where(v => v.First.InnerXml==nodes[j].ParentNode.InnerXml);
            var value = val!=null ? val.First() : new Pair<XmlNode, int>(null, 0);

            if (((j > 0) && nodes[j - 1].ParentNode.InnerXml!=value.First.InnerXml) || j==0)
              drawList.Add(new Pair<XmlNode, int>(nodes[j], value.Second));
            else if ((j > 0) && nodes[j - 1].ParentNode.InnerXml==value.First.InnerXml)
              drawList.Add(new Pair<XmlNode, int>(nodes[j], 0));
          }
          DrawSingleLine(drawList);
          if (i==treeDepth)
            break;
          helpList.Clear();
          for (int l = 0; l < nodes.Count; l++) {
            if (!nodes[l].HasChildNodes)
              helpList.Add(new Pair<XmlNode, int>(nodes[l], drawList[l].Second));
            else {
              var length = 0;
              for (int j = l - 1; j >= 0; j = j - 1) {
                if (nodes[j].HasChildNodes)
                  break;
                length += drawList[j].Second + Int32.Parse(drawList[j].First.Attributes["length"].Value);
              }
              helpList.Add(new Pair<XmlNode, int>(nodes[l], length + drawList[l].Second));
            }
          }
        }
        Log.Info(CreateFillString(fillStringLength + 1, '='));
      }
    }

    private void DrawSingleLine(List<Pair<XmlNode, int>> listOfNodes)
    {
      var str = new StringBuilder("|");
      var separateLine = new StringBuilder("|");
      bool drawSeporateLine = false;
      foreach (var node in listOfNodes) {
        if (!node.First.HasChildNodes) {
          var partStr = new StringBuilder();
          partStr.Append(CreateFillString(node.Second!=0 ? node.Second - 1 : 0, ' '))
            .Append(node.Second!=0 ? "|  " : "  ")
            .Append(node.First.Attributes["value"].Value).Append(CreateFillString(Int32.Parse
              (node.First.Attributes["length"].Value) - node.First.Attributes["value"].Value.Length - 3, ' ')).Append("|");
          separateLine.Append(CreateFillString(partStr.Length, ' '));
          str.Append(partStr);
        }
        else {
          var partStr = new StringBuilder();
          drawSeporateLine = true;
          partStr.Append(CreateFillString(node.Second, ' '));
          foreach (XmlNode o in node.First.ChildNodes) {
            partStr.Append("  ").Append(o.Name).Append(CreateFillString(Int32.Parse
              (o.Attributes["length"].Value) - o.Name.Length - 3, ' ')).Append("|");
          }
          separateLine.Append(CreateFillString(partStr.Length, '='));
          str.Append(partStr);
        }
      }

      if (str.Length < fillStringLength + 1)
        str.Append(CreateFillString(fillStringLength - str.Length, ' ')).Append("|");
      if (separateLine.Length < fillStringLength + 1)
        separateLine.Append(CreateFillString(fillStringLength - separateLine.Length, ' ')).Append("|");

      if (str.ToString().EndsWith(" "))
        str.Replace(" ", "|", str.Length - 1, 1);
      if (separateLine.ToString().EndsWith(" "))
        separateLine.Replace(" ", "|", str.Length - 1, 1);

      Log.Info(str.ToString());
      if (drawSeporateLine)
        Log.Info(separateLine.ToString());
    }

    private MemberType GetMemberType(Type type)
    {
      if (typeof (Key).IsAssignableFrom(type))
        return MemberType.Key;
      if (typeof (IEntity).IsAssignableFrom(type))
        return MemberType.Entity;
      if (typeof (Structure).IsAssignableFrom(type))
        return MemberType.Structure;
      if (typeof (EntitySetBase).IsAssignableFrom(type))
        return MemberType.EntitySet;
      if (Attribute.IsDefined(type, typeof (CompilerGeneratedAttribute), false)
        && type.BaseType==typeof (object)
          && type.Name.Contains("AnonymousType")
            && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
              && (type.Attributes & TypeAttributes.NotPublic)==TypeAttributes.NotPublic)
        return MemberType.Anonymous;
      return MemberType.Unknown;
    }

    private string ReplaceTabs(object value)
    {
      if (value==null)
        return "NULL";
      if (value.GetType()==typeof (byte[]))
        return "ByteArray";
      return value.ToString().Replace("-", " ").Replace("\n", string.Empty)
        .Replace("\t", string.Empty).Replace("\\", string.Empty)
        .Replace("\"", " ").Replace("\r", " ").Trim();
    }

    private string CreateFillString(int count, char element)
    {
      var list = Enumerable.Repeat(element, count);
      string result = String.Empty;
      foreach (var item in list)
        result = result + item;
      return result;
    }

    private bool ValueIsForOutput(PropertyInfo info)
    {
      var memberType = GetMemberType(info.PropertyType);
      return ((!info.CanWrite && info.PropertyType!=typeof (bool)
        && memberType!=MemberType.Key
          && info.PropertyType!=typeof (PersistenceState))
            || (info.CanWrite)) && info.DeclaringType!=typeof (Persistent)
              && info.DeclaringType!=typeof (SessionBound)
                && memberType!=MemberType.EntitySet;
    }

    private string KeyToString(object key)
    {
      var properties = key.GetType().GetProperties();
      var str = new StringBuilder("{ ");
      switch (GetMemberType(key.GetType())) {
      case MemberType.Structure:
      case MemberType.Anonymous:
        foreach (var info in properties) {
          if (ValueIsForOutput(info)) {
            str.Append(info.Name).Append(" = ");
            var propertyValue = info.GetValue(key, null);
            if (GetMemberType(info.PropertyType)==MemberType.Entity)
              str.Append((propertyValue!=null) ? info.PropertyType.GetProperty(Orm.WellKnown.KeyFieldName).GetValue(propertyValue, null).ToString() : "NULL");
            if (GetMemberType(info.PropertyType)==MemberType.Structure || GetMemberType(info.PropertyType)==MemberType.Anonymous)
              str.Append(ReplaceTabs(KeyToString(info.GetValue(key, null))));
            else
              str.Append(ReplaceTabs(propertyValue))
                .Append(" , ");
          }
        }
        break;
      case MemberType.Entity:
        str.Append(key.GetType().GetProperty(Orm.WellKnown.KeyFieldName).GetValue(key, null).ToString());
        break;

      default:
        str.Append(ReplaceTabs(key));
        break;
      }
      if (str[str.Length - 2]==',')
        str.Remove(str.Length - 2, 2);
      return str.Append(" }").ToString();
    }

    private static void EnumerateAll(IEnumerable enumerable)
    {
      foreach (var o in enumerable)
        if (o!=null) {
          if (o.GetType().IsGenericType && (o.GetType().GetGenericTypeDefinition()==typeof (IQueryable<>)
            || o.GetType().GetGenericTypeDefinition()==typeof (IEnumerable<>)
            || o.GetType().GetGenericTypeDefinition()==typeof (SubQuery<>)
            || o.GetType().GetGenericTypeDefinition()==typeof (Grouping<,>)))
            EnumerateAll((IEnumerable) o);
          var properties = o.GetType().GetProperties();
          foreach (var info in properties) {
            if (info.PropertyType.IsGenericType && 
              (info.PropertyType.GetGenericTypeDefinition()==typeof (IQueryable<>)
              || info.PropertyType.GetGenericTypeDefinition()==typeof (IEnumerable<>)
              || info.PropertyType.GetGenericTypeDefinition()==typeof (SubQuery<>)
              || info.PropertyType.GetGenericTypeDefinition()==typeof (Grouping<,>)
              ))
              EnumerateAll((IEnumerable) info.GetValue(o, null));
          }
        }
    }
  }
}