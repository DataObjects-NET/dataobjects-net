// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.15

namespace Xtensive.Sql.Compiler.Internals
{
  internal class HoleNode : Node
  {
    public readonly string Prefix;
    public readonly object Key;

    public override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public HoleNode(string prefix, object key)
    {
      Prefix = prefix;
      Key = key;
    }
  }
}