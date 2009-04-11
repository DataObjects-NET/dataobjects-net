// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.26

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  [DebuggerDisplay("Name = {Name}, Type = {Type}")]
  internal class MemberPathItem
  {
    public string Name { get; private set; }
    public MemberType Type { get; set; }
    public MemberExpression Expression { get; set; }


    // Constructor

    public MemberPathItem(string name, MemberType type, MemberExpression expression)
    {
      Name = name;
      Type = type;
      Expression = expression;
    }
  }
}