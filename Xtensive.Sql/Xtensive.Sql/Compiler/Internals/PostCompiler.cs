// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Compiler.Internals
{
  internal class PostCompiler : NodeVisitor
  {
    private const int MinimalResultCapacity = 64;
    private const int ResultCapacityMargin = 16;
    private readonly StringBuilder result;

    private HashSet<object> activeVariantKeys;
    private IDictionary<object, string> holeNodeValues;

    public override void Visit(TextNode node)
    {
      result.Append(node.Text);
    }

    public override void Visit(VariantNode node)
    {
      if (activeVariantKeys!=null && activeVariantKeys.Contains(node.Key))
        VisitNodeSequence(node.Alternative);
      else
        VisitNodeSequence(node.Main);
    }

    public override void Visit(HoleNode node)
    {
      if (holeNodeValues==null)
        throw new InvalidOperationException();
      string value;
      if (!holeNodeValues.TryGetValue(node.Id, out value))
        throw new InvalidOperationException();
      result.Append(value);
    }

    public static string Compile(Node root, IEnumerable<object> activeVariantKeys, IDictionary<object, string> holeNodeValues, int estimatedResultLength)
    {
      var compiler = new PostCompiler(estimatedResultLength) {
        activeVariantKeys = activeVariantKeys!=null
          ? activeVariantKeys.ToHashSet()
          : null, holeNodeValues = holeNodeValues
      };
      compiler.VisitNodeSequence(root);
      return compiler.result.ToString();
    }


    // Constructor

    private PostCompiler(int estimatedResultLength)
    {
      int capacity = estimatedResultLength + ResultCapacityMargin;
      result = new StringBuilder(capacity < MinimalResultCapacity ? MinimalResultCapacity : capacity);
    }
  }
}