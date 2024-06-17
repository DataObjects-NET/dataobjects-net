// Copyright (C) 2009-2024 Xtensive LLC.
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
    private readonly bool canActualizeQuery;

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
      _ = result.Append(node.Text);
    }

    public override void Visit(VariantNode node)
    {
      VisitNodes(configuration.AlternativeBranches.Contains(node.Id) ? node.Alternative : node.Main);
    }

    public override void Visit(PlaceholderNode node)
    {
      if (node is SchemaNodePlaceholderNode schemaPlaceHolder) {
        Visit(schemaPlaceHolder);
      }
      else {
        if (!configuration.PlaceholderValues.TryGetValue(node.Id, out var value))
          throw new InvalidOperationException(string.Format(Strings.ExValueForPlaceholderXIsNotSet, node.Id));
        _ = result.Append(value);
      }
    }

    private void Visit(SchemaNodePlaceholderNode node)
    {
      EnsureActualizationPossible();

      var schema = node.SchemaNode.Schema;

      var names = (node.DbQualified)
        ? new string[] { schema.Catalog.GetActualDbName(configuration.DatabaseMapping), schema.GetActualDbName(configuration.SchemaMapping), node.SchemaNode.DbName }
        : new string[] { schema.GetActualDbName(configuration.SchemaMapping), node.SchemaNode.DbName };

      _ = result.Append(SqlHelper.Quote(node.EscapeSetup, names));
    }

    public override void Visit(CycleItemNode node)
    {
      _ = result.Append(currentCycleItem[node.Index]);
    }

    public override void Visit(CycleNode node)
    {
      if (!configuration.DynamicFilterValues.TryGetValue(node.Id, out var items))
        throw new InvalidOperationException(string.Format(Strings.ExItemsForCycleXAreNotSpecified, node.Id));
      if (items==null || items.Count==0) {
        VisitNodes(node.EmptyCase);
        return;
      }
      for (int i = 0, count = items.Count; i < count - 1; i++) {
        currentCycleItem = items[i];
        VisitNodes(node.Body);
        _ = result.Append(node.Delimiter);
      }
      currentCycleItem = items[items.Count - 1];
      VisitNodes(node.Body);
    }

    #endregion

    private void EnsureActualizationPossible()
    {
      if (!canActualizeQuery) {
        throw new InvalidOperationException(Strings.ExUnableToActualizeSchemaNodeInQuery);
      }
    }

    // Constructors

    private PostCompiler(SqlPostCompilerConfiguration configuration, int estimatedResultLength)
    {
      int capacity = estimatedResultLength + ResultCapacityMargin;
      result = new StringBuilder(capacity < MinimalResultCapacity ? MinimalResultCapacity : capacity);
      this.configuration = configuration;
      canActualizeQuery = configuration.DatabaseMapping != null && configuration.SchemaMapping != null;
    }
  }
}