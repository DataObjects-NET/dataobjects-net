// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

using System.Collections.Generic;

namespace Xtensive.Sql.Compiler
{
  internal class VariantNode : Node
  {
    public readonly object Id;

    public readonly IEnumerable<Node> Main;
    public readonly IEnumerable<Node> Alternative;

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructor

    public VariantNode(object id, IEnumerable<Node> main, IEnumerable<Node> alternative)
    {
      Id = id;
      Main = main;
      Alternative = alternative;
    }
  }
}