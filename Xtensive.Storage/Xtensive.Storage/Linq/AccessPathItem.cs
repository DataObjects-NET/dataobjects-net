// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.26

using System.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  public class AccessPathItem
  {
    public string Name { get; private set; }
    public AccessType Type { get; set; }
    public Expression Expression { get; set; }


    // Constructor

    public AccessPathItem(string name, AccessType type, Expression expression)
    {
      Name = name;
      Type = type;
      Expression = expression;
    }
  }
}