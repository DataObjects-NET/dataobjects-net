// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.24

using System;
using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage.Building.DependencyGraph
{
  [Serializable]
  internal class Edge
  {
    public TypeDef Tail { get; private set; }

    public TypeDef Head { get; private set; }

    public EdgeKind Kind { get; private set; }

    public EdgeWeight Weight { get; private set; }

    public override int GetHashCode()
    {
      unchecked {
        return (Tail.GetHashCode() * 397) ^ Head.GetHashCode();
      }
    }

    public bool Equals(Edge obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj.Tail, Tail) && Equals(obj.Head, Head);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Edge))
        return false;
      return Equals((Edge) obj);
    }

    public static bool operator ==(Edge left, Edge right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Edge left, Edge right)
    {
      return !Equals(left, right);
    }

    public Edge(TypeDef tail, TypeDef head, EdgeKind kind, EdgeWeight weight)
    {
      Tail = tail;
      Head = head;
      Kind = kind;
      Weight = weight;
    }
  }
}