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
    private NodeContainer output;
    private SqlNode[] traversalPath;

    private readonly NodeContainer root;
    private readonly Stack<SqlNode> traversalStack;
    private readonly HashSet<SqlNode> traversalTable;
    private readonly AliasProvider aliasProvider = new AliasProvider();
    private readonly Dictionary<object, string> parameterNames = new Dictionary<object, string>();
 
    public bool IsEmpty { get { return output.IsEmpty; } }
    public AliasProvider AliasProvider { get { return aliasProvider; } }

    internal Dictionary<object, string> ParameterNames { get { return parameterNames; } }
    internal NodeContainer Output { get { return output; } }

    public void AppendText(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      output.Add(new TextNode(text));
    }

    public void AppendDelimiter(string text)
    {
      output.Add(new NodeDelimiter(DelimiterType.Row, text));
    }

    public void AppendDelimiter(string text, DelimiterType type)
    {
      output.Add(new NodeDelimiter(type, text));
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
      output.Add(variant);
      return CreateScope(ContextType.Collection, (NodeContainer) variant.Main);
    }

    public SqlCompilerScope EnterAlternativeVariant(SqlNode node, object key)
    {
      var variant = (VariantNode) output.Current;
      if (variant.Key != key)
        throw new InvalidOperationException();
      return CreateScope(ContextType.Collection, (NodeContainer) variant.Alternative);
    }

    public string GetParameterName(SqlParameterRef parameterRef)
    {
      if (!string.IsNullOrEmpty(parameterRef.Name))
        return parameterRef.Name;
      string name;
      if (!parameterNames.TryGetValue(parameterRef.Parameter, out name)) {
        name = "p" + nextParameter++;
        parameterNames.Add(parameterRef.Parameter, name);
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
      output = scope.OriginalOutput;
      if (scope.Type == ContextType.Node)
        traversalTable.Remove(traversalStack.Pop());
    }

    #region Private methods

    private SqlCompilerScope CreateScope(ContextType type)
    {
      var newContainer = new NodeContainer();
      output.Add(newContainer);
      return CreateScope(type, newContainer);
    }

    private SqlCompilerScope CreateScope(ContextType type, NodeContainer newContainer)
    {
      traversalPath = null;
      var scope = new SqlCompilerScope(this, output, type);
      output = newContainer;
      return scope;
    }

    #endregion

    // Constructor

    internal SqlCompilerContext()
    {
      traversalStack = new Stack<SqlNode>();
      traversalTable = new HashSet<SqlNode>();
      root = new NodeContainer();
      output = root;
    }
  }
}
