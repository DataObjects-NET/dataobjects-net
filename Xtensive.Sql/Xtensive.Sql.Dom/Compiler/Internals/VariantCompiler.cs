// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class VariantCompiler : NodeVisitor
  {
    private readonly StringBuilder result = new StringBuilder();
    private readonly HashSet<object> activeKeys;
    private readonly Node root;

    public override void Visit(TextNode node)
    {
      result.Append(node.Text);
    }

    public override void Visit(VariantNode node)
    {
      if (activeKeys != null && activeKeys.Contains(node.Key))
        VisitNodeSequence(node.Alternative);
      else
        VisitNodeSequence(node.Main);
    }

    public string Compile()
    {
      VisitNodeSequence(root);
      return result.ToString();
    }

    // Constructor

    public VariantCompiler(Node root)
    {
      this.root = root;
    }

    public VariantCompiler(Node root, IEnumerable<object> activeKeys)
    {
      this.root = root;
      this.activeKeys = activeKeys.ToHashSet();
    }
  }
}