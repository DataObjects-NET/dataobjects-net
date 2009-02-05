// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.01.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq
{
  public class MemberPath : IEnumerable<MemberPathItem>
  {
    private readonly Deque<MemberPathItem> pathItems;

    public int Count
    {
      get { return pathItems != null ? pathItems.Count : 0; }
    }

    public bool IsValid
    {
      get { return pathItems!=null; }
    }

    private static MemberType GetMemberType(Type type)
    {
      if (typeof(Key).IsAssignableFrom(type))
        return MemberType.Key;
      if (typeof(IEntity).IsAssignableFrom(type))
        return MemberType.Entity;
      if (typeof(Structure).IsAssignableFrom(type))
        return MemberType.Structure;
      if (typeof(EntitySetBase).IsAssignableFrom(type))
        return MemberType.EntitySet;
      return MemberType.Unknown;
    }

    public static MemberPath Parse(Expression e, DomainModel model)
    {
      var result = new Deque<MemberPathItem>();
      MemberPathItem lastItem = null;
      Expression current = e;
      bool entityKeyAssociation = false;
      while (current.NodeType == ExpressionType.MemberAccess) {
        var memberAccess = (MemberExpression) current;
        var member = memberAccess.Member;
        current = memberAccess.Expression;
        MemberPathItem item;
        switch (GetMemberType(memberAccess.Type)) {
        case MemberType.Key:
          item = new MemberPathItem(member.Name, MemberType.Key, memberAccess);
          break;
        case MemberType.Structure:
          if (lastItem == null) {
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
          if (lastItem == null) {
            item = new MemberPathItem(member.Name, MemberType.Entity, memberAccess);
          }
          else {
            if (lastItem.Type == MemberType.Key) {
              if (!entityKeyAssociation) {
                item = new MemberPathItem (
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
              if(!type.Fields.TryGetValue(lastItem.Name, out field))
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
        default:
          if (lastItem != null)
            return new MemberPath();
          Type sourceType = memberAccess.Expression.Type;
          MemberType memberType = GetMemberType(sourceType);
          switch (memberType) {
          case MemberType.Key:
            return new MemberPath();
          case MemberType.Entity:
          case MemberType.Structure: {
            var sourceTypeInfo = model.Types[sourceType];
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
      if (current.NodeType == ExpressionType.Parameter) {
        if (lastItem != null)
          result.AddHead(lastItem);
      }
      else
        return new MemberPath();
      return new MemberPath(result);
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

    private MemberPath(Deque<MemberPathItem> pathItems)
    {
      this.pathItems = pathItems;
    }

    private MemberPath()
    {
    }
  }
}
