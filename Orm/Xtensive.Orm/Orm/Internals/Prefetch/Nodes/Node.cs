// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.13

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal abstract class Node : IEquatable<Node>
  {
    public string Path { get; private set; }

    public abstract Node Accept(NodeVisitor visitor);

    public bool Equals(Node other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other)) 
        return true;
      return Equals(other.Path, Path);
    }

    public override bool Equals(object obj)
    {
      if (obj is null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as Node);
    }

    public override int GetHashCode()
    {
      return Path.GetHashCode();
    }

    public override string ToString()
    {
      var builder = new StringBuilder();
      AppendDescription(builder, 0);
      return builder.ToString();
    }

    private void AppendDescription(StringBuilder output, int indent)
    {
      output.AppendIndented(indent, GetDescription());
      output.AppendLine();
      var hasNestedNodes = this as IHasNestedNodes;
      if (hasNestedNodes!=null)
        foreach (var node in hasNestedNodes.NestedNodes)
          node.AppendDescription(output, indent + 2);
    }

    protected virtual string GetDescription()
    {
      var nodeName = GetType().Name.TryCutSuffix(typeof (Node).Name);
      return $"{nodeName}({Path})";
    }

    // Constructors

    protected Node(string path)
    {
      Path = path;
    }
  }
}