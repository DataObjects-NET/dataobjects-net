// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.01.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq
{
  public class AccessPath : IEnumerable<AccessPathItem>
  {
    private Deque<AccessPathItem> pathItems;

    public int Count
    {
      get
      {
        return pathItems.Count;
      }
    }

    public static AccessPath Parse(Expression e, DomainModel model)
    {
      var result = new Deque<AccessPathItem>();
      AccessPathItem lastItem = null;
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
        AccessPathItem item;
        if (isKey) {
          item = new AccessPathItem(member.Name, AccessType.Key, memberAccess);
        }
        else if(isStructure) {
          if (lastItem == null) {
            item = new AccessPathItem(member.Name, AccessType.Struct, memberAccess);
          }
          else {
            item = new AccessPathItem(
              member.Name + "." + lastItem.Name, 
              lastItem.Type, 
              lastItem.Expression);
          }
        }
        else if (isEntity) {
          if (lastItem == null) {
            item = new AccessPathItem(member.Name, AccessType.Entity, memberAccess);
          }
          else {
            if (lastItem.Type == AccessType.Key) {
              if (!entityKeyAssociation) {
                item = new AccessPathItem (
                  member.Name + "." + lastItem.Name,
                  lastItem.Type,
                  lastItem.Expression);
                entityKeyAssociation = true;
              }
              else {
                item = new AccessPathItem(
                  member.Name,
                  AccessType.Entity, 
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
                item = new AccessPathItem(
                  member.Name + "." + lastItem.Name,
                  lastItem.Type,
                  lastItem.Expression);
              }
              else {
                item = new AccessPathItem(
                  member.Name,
                  AccessType.Entity,
                  memberAccess);
                result.AddHead(lastItem);
              }
            }
          }
        }
        else if (isEntitySet) {
          item = new AccessPathItem(member.Name, AccessType.EntitySet, memberAccess);
        }
        else {
          item = new AccessPathItem(member.Name, AccessType.Field, memberAccess);
        }
        lastItem = item;
      }
      if (current.NodeType == ExpressionType.Parameter) {
        if (lastItem != null)
          result.AddHead(lastItem);
      }
      else
        throw Exceptions.InternalError(string.Format(
          Strings.ExUnsupportedExpressionType, current.NodeType), Log.Instance);
      return new AccessPath(result);
    }

    /// <inheritdoc/>
    public IEnumerator<AccessPathItem> GetEnumerator()
    {
      return pathItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructor

    private AccessPath(Deque<AccessPathItem> pathItems)
    {
      this.pathItems = pathItems;
    }
  }
}