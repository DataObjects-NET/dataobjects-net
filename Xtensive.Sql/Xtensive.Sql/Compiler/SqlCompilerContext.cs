// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Sql.Compiler.Internals;
using Xtensive.Sql.Exceptions;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql.Compiler
{
  public class SqlCompilerContext
  {
    private SqlNode[] traversalPath;
    private readonly Stack<SqlNode> traversalStack;
    private readonly HashSet<SqlNode> traversalTable;

    public SqlTableNameProvider TableNameProvider { get; private set; }

    public SqlParameterNameProvider ParameterNameProvider { get; set; }

    public NodeContainer Output { get; private set; }

    public SqlCompilerNamingOptions NamingOptions { get; private set; }

    public bool IsEmpty
    {
      get { return Output.IsEmpty; }
    }

    public SqlNode[] GetTraversalPath()
    {
      if (traversalPath == null)
        traversalPath = traversalStack.ToArray();
      return traversalPath;
    }

    public SqlCompilerNamingScope EnterScope(SqlCompilerNamingOptions options)
    {
      if (NamingOptions == options)
        return null;

      var scope = new SqlCompilerNamingScope(this, NamingOptions);
      NamingOptions = options;
      return scope;
    }

    internal void CloseScope(SqlCompilerNamingScope scope)
    {
      NamingOptions = scope.ParentOptions;
    }

    #region SqlCompilerOutputScope members

    public SqlCompilerOutputScope EnterScope(SqlNode node)
    {
      if (traversalTable.Contains(node))
        throw new SqlCompilerException(Strings.ExCircularReferenceDetected);
      traversalStack.Push(node);
      traversalTable.Add(node);
      return OpenScope(ContextType.Node);
    }

    public SqlCompilerOutputScope EnterCollectionScope()
    {
      return OpenScope(ContextType.Collection);
    }

    public SqlCompilerOutputScope EnterMainVariantScope(SqlNode node, object key)
    {
      var variant = new VariantNode(key) {Main = new NodeContainer(), Alternative = new NodeContainer()};
      Output.Add(variant);
      return OpenScope(ContextType.Collection, (NodeContainer) variant.Main);
    }

    public SqlCompilerOutputScope EnterAlternativeVariantScope(SqlNode node, object key)
    {
      var variant = (VariantNode) Output.Current;
      if (variant.Key != key)
        throw new InvalidOperationException();
      return OpenScope(ContextType.Collection, (NodeContainer) variant.Alternative);
    }

    private SqlCompilerOutputScope OpenScope(ContextType type)
    {
      var container = new NodeContainer();
      Output.Add(container);
      return OpenScope(type, container);
    }

    private SqlCompilerOutputScope OpenScope(ContextType type, NodeContainer container)
    {
      traversalPath = null;
      var scope = new SqlCompilerOutputScope(this, Output, type);
      Output = container;
      return scope;
    }

    internal void CloseScope(SqlCompilerOutputScope scope)
    {
      traversalPath = null;
      Output = scope.ParentContainer;
      if (scope.Type == ContextType.Node)
        traversalTable.Remove(traversalStack.Pop());
    }

    #endregion



    // Constructor

    internal SqlCompilerContext(SqlCompilerOptions options)
    {
      NamingOptions = SqlCompilerNamingOptions.TableQualifiedColumns;
      if (options.ForcedAliasing)
        NamingOptions |= SqlCompilerNamingOptions.TableAliasing;

      TableNameProvider = new SqlTableNameProvider(this);
      ParameterNameProvider = new SqlParameterNameProvider(options);
      traversalStack = new Stack<SqlNode>();
      traversalTable = new HashSet<SqlNode>();
      Output = new NodeContainer();
    }
  }
}
