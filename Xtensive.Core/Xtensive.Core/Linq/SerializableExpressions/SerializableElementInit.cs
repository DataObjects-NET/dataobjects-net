// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.05.14

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Xtensive.Core.Linq.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ElementInit"/>
  /// </summary>
  [Serializable]
  public class SerializableElementInit
  {
    public MethodInfo AddMethod;
    public SerializableExpression[] Arguments;
  }
}