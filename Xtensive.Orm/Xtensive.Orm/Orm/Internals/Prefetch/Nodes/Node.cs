// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.01.13

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal abstract class Node : IEquatable<Node>
  {
    public string Path { get; private set; }

    protected internal abstract Node Accept(NodeVisitor visitor);

    public virtual bool Equals(Node other)
    {
      if (ReferenceEquals(null, other)) 
        return false;
      if (ReferenceEquals(this, other)) 
        return true;
      return Equals(other.Path, Path);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as Node);
    }

    public override int GetHashCode()
    {
      return Path.GetHashCode();
    }

    protected Node(string path)
    {
      Path = path;
    }
  }
}