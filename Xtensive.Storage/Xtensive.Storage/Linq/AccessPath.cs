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
using Xtensive.Storage.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Linq
{
  public class AccessPath : IEnumerable<AccessPathItem>
  {

//    public static AccessPath Parse(Expression e)
//    {
//      var result = new Deque<AccessPathItem>();
//      string fieldName = null;
//      bool isJoined = false;
//      Expression current = e;
//      while (current.NodeType == ExpressionType.MemberAccess) {
//        var memberAccess = (MemberExpression) current;
//        var member = memberAccess.Member;
//        current = memberAccess.Expression;
//        if (typeof (IEntity).IsAssignableFrom(memberAccess.Type)) {
//          var type = translator.Model.Types[memberAccess.Type];
//          if (fieldName == null) {
//            fieldName = member.Name;
//            isJoined = true;
//          }
//          else {
//            if (fieldName == "Key" || type.Fields[fieldName].IsPrimaryKey)
//              fieldName = member.Name + "." + fieldName;
//            else {
//              var pathItem = new AccessPathItem(isJoined ? null : fieldName, 
//                isJoined ? fieldName : null);
//              result.AddHead(pathItem);
//              fieldName = member.Name;
//              isJoined = true;
//            }
//          }
//        }
//        else
//          fieldName = fieldName==null ? member.Name : member.Name + "." + fieldName;
//      }
//      if (current.NodeType == ExpressionType.Parameter) {
//        if (fieldName != null) {
//          var pathItem = new AccessPathItem(isJoined ? null : fieldName,
//            isJoined ? fieldName : null);
//          result.AddHead(pathItem);
//        }
//      }
//      else
//        throw Exceptions.InternalError(string.Format(
//          Strings.ExUnsupportedExpressionType, current.NodeType), Log.Instance);
//      return result;
//      throw new NotImplementedException();
//    }

    /// <inheritdoc/>
    public IEnumerator<AccessPathItem> GetEnumerator()
    {
      throw new System.NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructor

    private AccessPath(Expression e)
    {

    }
  }
}