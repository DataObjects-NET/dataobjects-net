// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.01.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq
{
  [DebuggerDisplay("PathType = {PathType}, IsValid = {IsValid}, Count = {Count}")]
  internal class MemberPath : IEnumerable<MemberPathItem>
  {
    private readonly Deque<MemberPathItem> pathItems;
    private readonly MemberType pathType;

    public MemberType PathType
    {
      get { return pathType; }
    }

    public int Count
    {
      get { return pathItems!=null ? pathItems.Count : 0; }
    }

    public bool IsValid
    {
      get { return pathItems!=null; }
    }

    public ParameterExpression Parameter { get; private set; }

    public Expression TranslateParameter(Expression parameterSubstitution)
    {
      if (!IsValid)
        throw new InvalidOperationException();
      var result = parameterSubstitution;
      if (Count > 0)
        result = ExpressionReplacer.Replace(pathItems.Tail.Expression, Parameter, parameterSubstitution);
      return result;
    }

    public static MemberPath Parse(Expression e, DomainModel model)
    {
      var result = new Deque<MemberPathItem>();
      MemberPathItem lastItem = null;
      Expression current = e;
      bool entityKeyAssociation = false;
      while (current.NodeType==ExpressionType.MemberAccess || current.NodeType==ExpressionType.Convert) {
        if (current.NodeType==ExpressionType.Convert) {
          current = ((UnaryExpression) current).Operand;
          continue;
        }
        var memberAccess = (MemberExpression) current;
        var member = memberAccess.Member;
        current = memberAccess.Expression;
        MemberPathItem item;
        var memberAccessType = memberAccess.GetMemberType();
        switch (memberAccessType) {
          case MemberType.Key:
            item = new MemberPathItem(member.Name, MemberType.Key, memberAccess);
            break;
          case MemberType.Structure:
            if (lastItem==null) {
              item = new MemberPathItem(member.Name, MemberType.Structure, memberAccess);
            }
            else {
              item = new MemberPathItem(
                member.Name + "." + lastItem.Name,
                lastItem.Type,
                lastItem.Expression);
            }
            break;
          case MemberType.Entity:
            if (lastItem==null) {
              item = new MemberPathItem(member.Name, MemberType.Entity, memberAccess);
            }
            else {
              var expressionType = current.GetMemberType();
              if (lastItem.Type==MemberType.Key) {
                if (!entityKeyAssociation && expressionType != MemberType.Anonymous) {
                  item = new MemberPathItem(
                    member.Name + "." + lastItem.Name,
                    lastItem.Type,
                    lastItem.Expression);
                  entityKeyAssociation = true;
                }
                else {
                  item = new MemberPathItem(
                    member.Name,
                    MemberType.Entity,
                    memberAccess);
                  result.AddHead(lastItem);
                }
              }
              else {
                var type = model.Types[memberAccess.Type];
                FieldInfo field;
                if (!type.Fields.TryGetValue(lastItem.Name, out field))
                  throw new InvalidOperationException(string.Format(Strings.ExFieldNotFoundInModel, lastItem.Name));
                if (field.IsPrimaryKey) {
                  item = new MemberPathItem(
                    member.Name + "." + lastItem.Name,
                    lastItem.Type,
                    lastItem.Expression);
                }
                else {
                  item = new MemberPathItem(
                    member.Name,
                    MemberType.Entity,
                    memberAccess);
                  result.AddHead(lastItem);
                }
              }
            }
            break;
          case MemberType.EntitySet:
            item = new MemberPathItem(member.Name, MemberType.EntitySet, memberAccess);
            break;
          case MemberType.Anonymous:
            if (lastItem == null)
              item = new MemberPathItem(member.Name, MemberType.Anonymous, memberAccess);
            else {
              item = new MemberPathItem(
                member.Name,
                MemberType.Anonymous,
                memberAccess);
              result.AddHead(lastItem);
            }
            break;
//          case MemberType.Grouping:
//            break;
          default:
            if (lastItem!=null)
              return new MemberPath();
            MemberType memberType = memberAccess.Expression.GetMemberType();
            switch (memberType) {
              case MemberType.Key:
                return new MemberPath();
              case MemberType.Entity:
              case MemberType.Structure: {
                var sourceTypeInfo = model.Types[memberAccess.Expression.Type];
                if (!sourceTypeInfo.Fields.Contains(member.Name))
                  return new MemberPath();
              }
                break;
            }
            item = new MemberPathItem(member.Name, MemberType.Primitive, memberAccess);
            break;
        }
        lastItem = item;
      }
      if (current.NodeType==ExpressionType.Parameter) {
        if (lastItem!=null)
          result.AddHead(lastItem);
        return new MemberPath(result, (ParameterExpression) current);
      }
      else
        return new MemberPath();
    }

    /// <inheritdoc/>
    public IEnumerator<MemberPathItem> GetEnumerator()
    {
      if (!IsValid)
        throw new InvalidOperationException();
      return pathItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructor

    private MemberPath(Deque<MemberPathItem> pathItems, ParameterExpression parameter)
    {
      Parameter = parameter;
      this.pathItems = pathItems;
      pathType = pathItems.Count > 0
        ? pathItems.Tail.Type
        : parameter.GetMemberType();
    }

    private MemberPath()
    {
    }
  }
}