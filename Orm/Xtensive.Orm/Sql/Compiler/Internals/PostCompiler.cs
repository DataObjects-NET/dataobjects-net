// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xtensive.Sql.Compiler
{
  internal class PostCompiler : NodeVisitor
  {
    private const int MinimalResultCapacity = 64;
    private const int ResultCapacityMargin = 16;

    private readonly StringBuilder result;
    private readonly SqlPostCompilerConfiguration configuration;

    private string[] currentCycleItem;

    public static string Process(IReadOnlyList<Node> nodes, SqlPostCompilerConfiguration configuration, int estimatedResultLength)
    {
      var textNodesLength = nodes.OfType<TextNode>().Sum(o => o.Text.Length);
      var compiler = new PostCompiler(configuration, Math.Max(textNodesLength, estimatedResultLength));
      compiler.VisitNodes(nodes);
      return compiler.result.ToString();
    }
    
    #region NodeVisitor members

    public override void Visit(TextNode node)
    {
      result.Append(node.Text);
    }

    public override void Visit(VariantNode node)
    {
      if (configuration.AlternativeBranches.Contains(node.Id))
        VisitNodes(node.Alternative);
      else
        VisitNodes(node.Main);
    }

    public override void Visit(PlaceholderNode node)
    {
      string value;
      if (!configuration.PlaceholderValues.TryGetValue(node.Id, out value))
        throw new InvalidOperationException(string.Format(Strings.ExValueForPlaceholderXIsNotSet, node.Id));
      result.Append(value);
    }

    public override void Visit(CycleItemNode node)
    {
      result.Append(currentCycleItem[node.Index]);
    }

    public override void Visit(CycleNode node)
    {
      List<string[]> items;
      if (!configuration.DynamicFilterValues.TryGetValue(node.Id, out items))
        throw new InvalidOperationException(string.Format(Strings.ExItemsForCycleXAreNotSpecified, node.Id));
      if (items==null || items.Count==0) {
        VisitNodes(node.EmptyCase);
        return;
      }
      for (int i = 0; i < items.Count - 1; i++) {
        currentCycleItem = items[i];
        VisitNodes(node.Body);
        result.Append(node.Delimiter);
      }
      currentCycleItem = items[items.Count - 1];
      VisitNodes(node.Body);
    }

    #endregion


    // Constructors

    private PostCompiler(SqlPostCompilerConfiguration configuration, int estimatedResultLength)
    {
      int capacity = estimatedResultLength + ResultCapacityMargin;
      result = new StringBuilder(capacity < MinimalResultCapacity ? MinimalResultCapacity : capacity);
      this.configuration = configuration;
    }
  }
}