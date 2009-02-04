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
    private Deque<MemberPathItem> pathItems;

    public int Count
    {
      get { return pathItems != null ? pathItems.Count : 0; }
    }

    public bool IsValid
    {
      get { return pathItems!=null; }
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
        bool isKey = typeof(Key).IsAssignableFrom(memberAccess.Type);
        bool isEntity = typeof(IEntity).IsAssignableFrom(memberAccess.Type);
        bool isEntitySet = typeof(EntitySetBase).IsAssignableFrom(memberAccess.Type);
        bool isStructure = typeof(Structure).IsAssignableFrom(memberAccess.Type);
        MemberPathItem item;
        if (isKey) {
          item = new MemberPathItem(member.Name, MemberType.Key, memberAccess);
        }
        else if(isStructure) {
          if (lastItem == null) {
            item = new MemberPathItem(member.Name, MemberType.Struct, memberAccess);
          }
          else {
            item = new MemberPathItem(
              member.Name + "." + lastItem.Name, 
              lastItem.Type, 
              lastItem.Expression);
          }
        }
        else if (isEntity) {
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
        }
        else if (isEntitySet) {
          item = new MemberPathItem(member.Name, MemberType.EntitySet, memberAccess);
        }
        else {
          if (lastItem != null)
            return new MemberPath();
          var sourceType = memberAccess.Expression.Type;
          var sourceIsEntity = typeof(IEntity).IsAssignableFrom(sourceType);
          var sourceIsStructure = typeof(Structure).IsAssignableFrom(sourceType);
          var sourceIsKey = typeof(Key).IsAssignableFrom(sourceType);
          if (sourceIsKey)
            return new MemberPath();
          if (sourceIsStructure || sourceIsEntity) {
            var sourceTypeInfo = model.Types[sourceType];
            if (!sourceTypeInfo.Fields.Contains(member.Name))
              return new MemberPath();
          }
          item = new MemberPathItem(member.Name, MemberType.Field, memberAccess);
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