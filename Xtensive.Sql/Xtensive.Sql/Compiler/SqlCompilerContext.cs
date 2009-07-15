// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Sql.Compiler.Internals;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Exceptions;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql.Compiler
{
  public class SqlCompilerContext
  {
    private int nextParameter;
    private SqlNode[] traversalPath;

    private readonly NodeContainer root;
    private readonly Stack<SqlNode> traversalStack;
    private readonly HashSet<SqlNode> traversalTable;

    public bool IsEmpty { get { return Output.IsEmpty; } }
    public AliasProvider AliasProvider { get; private set; }

    internal string ParameterPrefix { get; set; }
    internal Dictionary<object, string> ParameterNames { get; private set; }
    internal NodeContainer Output { get; private set; }

    public void AppendHole(string prefix, object key)
    {
      Output.Add(new HoleNode(prefix, key));
    }

    public void AppendText(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      Output.Add(new TextNode(text));
    }

    public void AppendDelimiter(string text)
    {
      Output.Add(new NodeDelimiter(DelimiterType.Row, text));
    }

    public void AppendDelimiter(string text, DelimiterType type)
    {
      Output.Add(new NodeDelimiter(type, text));
    }

    public SqlCompilerScope EnterNode(SqlNode node)
    {
      if (traversalTable.Contains(node))
        throw new SqlCompilerException(Strings.ExCircularReferenceDetected);
      traversalStack.Push(node);
      traversalTable.Add(node);
      return CreateScope(ContextType.Node);
    }

    public SqlCompilerScope EnterCollection()
    {
      return CreateScope(ContextType.Collection);
    }
    
    public SqlCompilerScope EnterMainVariant(SqlNode node, object key)
    {
      var variant = new VariantNode(key) {Main = new NodeContainer(), Alternative = new NodeContainer()};
      Output.Add(variant);
      return CreateScope(ContextType.Collection, (NodeContainer) variant.Main);
    }

    public SqlCompilerScope EnterAlternativeVariant(SqlNode node, object key)
    {
      var variant = (VariantNode) Output.Current;
      if (variant.Key != key)
        throw new InvalidOperationException();
      return CreateScope(ContextType.Collection, (NodeContainer) variant.Alternative);
    }

    public string GetParameterName(object parameter)
    {
      string name;
      if (!ParameterNames.TryGetValue(parameter, out name)) {
        name = ParameterPrefix + nextParameter++;
        ParameterNames.Add(parameter, name);
      }
      return name;
    }

    /// <summary>
    /// Gets the traversal path.
    /// </summary>
    /// <value>The traversal path.</value>
    public SqlNode[] GetTraversalPath()
    {
      if (traversalPath == null)
        traversalPath = traversalStack.ToArray();
      return traversalPath;
    }

    internal void DisposeScope(SqlCompilerScope scope)
    {
      traversalPath = null;
      Output = scope.OriginalOutput;
      if (scope.Type == ContextType.Node)
        traversalTable.Remove(traversalStack.Pop());
    }

    #region Private methods

    private SqlCompilerScope CreateScope(ContextType type)
    {
      var newContainer = new NodeContainer();
      Output.Add(newContainer);
      return CreateScope(type, newContainer);
    }

    private SqlCompilerScope CreateScope(ContextType type, NodeContainer newContainer)
    {
      traversalPath = null;
      var scope = new SqlCompilerScope(this, Output, type);
      Output = newContainer;
      return scope;
    }

    #endregion

    // Constructor

    internal SqlCompilerContext()
    {
      ParameterNames = new Dictionary<object, string>();
      AliasProvider = new AliasProvider();
      traversalStack = new Stack<SqlNode>();
      traversalTable = new HashSet<SqlNode>();
      root = new NodeContainer();
      Output = root;
    }
  }
}
