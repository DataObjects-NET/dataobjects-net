// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.11.06

using System.Collections.Generic;

namespace Xtensive.Sql.Compiler
{
  internal class CycleNode : Node
  {
    public readonly object Id;

    public readonly IEnumerable<Node> Body;
    public readonly IEnumerable<Node> EmptyCase;

    public readonly string Delimiter;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    public CycleNode(object id, IEnumerable<Node> body, IEnumerable<Node> emptyCase, string delimiter)
    {
      Id = id;
      Body = body;
      EmptyCase = emptyCase;
      Delimiter = delimiter;
    }
  }
}