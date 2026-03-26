// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.02.24

using System;

namespace Xtensive.Orm.Building.DependencyGraph
{
  [Serializable]
  internal readonly struct Edge<TValue> : IEquatable<Edge<TValue>>
  {
    public Node<TValue> Tail { get; }

    public Node<TValue> Head { get; }

    public EdgeKind Kind { get; }

    public EdgeWeight Weight { get; }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return (Tail.GetHashCode() * 397) ^ Head.GetHashCode();
      }
    }

    /// <inheritdoc/>
    public bool Equals(Edge<TValue> obj) =>
      Equals(obj.Tail, Tail) && Equals(obj.Head, Head) && obj.Kind == Kind && obj.Weight == Weight;

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is Edge<TValue> other && Equals(other);

    /// <inheritdoc/>
    public static bool operator ==(Edge<TValue> left, Edge<TValue> right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(Edge<TValue> left, Edge<TValue> right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"{Tail} -> {Head}, ({Kind}, {Weight})";
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
