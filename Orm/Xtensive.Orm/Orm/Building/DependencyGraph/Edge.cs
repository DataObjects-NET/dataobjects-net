// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.24

using System;

namespace Xtensive.Orm.Building.DependencyGraph
{
  [Serializable]
  internal class Edge<TValue>
  {
    public Node<TValue> Tail { get; private set; }

    public Node<TValue> Head { get; private set; }

    public EdgeKind Kind { get; private set; }

    public EdgeWeight Weight { get; private set; }

    
    public override int GetHashCode()
    {
      unchecked {
        return (Tail.GetHashCode() * 397) ^ Head.GetHashCode();
      }
    }

    
    public bool Equals(Edge<TValue> obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj.Tail, Tail) && Equals(obj.Head, Head) && obj.Kind == Kind && obj.Weight == Weight;
    }

    
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Edge<TValue>))
        return false;
      return Equals((Edge<TValue>) obj);
    }

    
    public static bool operator ==(Edge<TValue> left, Edge<TValue> right)
    {
      return Equals(left, right);
    }

    
    public static bool operator !=(Edge<TValue> left, Edge<TValue> right)
    {
      return !Equals(left, right);
    }

    
    public override string ToString()
    {
      return string.Format("{0} -> {1}, ({2}, {3})", Tail, Head, Kind, Weight);
    }


    // Constructors

    public Edge(Node<TValue> tail, Node<TValue> head, EdgeKind kind, EdgeWeight weight)
    {
      Tail = tail;
      Head = head;
      Kind = kind;
      Weight = weight;
    }
  }
}