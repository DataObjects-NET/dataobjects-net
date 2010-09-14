// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// SQL compiler context.
  /// </summary>
  public class SqlCompilerContext
  {
    private SqlNode[] traversalPath;
    private readonly Stack<SqlNode> traversalStack;
    private readonly HashSet<SqlNode> traversalTable;

    public SqlTableNameProvider TableNameProvider { get; private set; }

    public SqlParameterNameProvider ParameterNameProvider { get; private set; }

    public ContainerNode Output { get; private set; }

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
      if (NamingOptions==options)
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

    public SqlCompilerOutputScope EnterMainVariantScope(object id)
    {
      var variant = new VariantNode(id) {
        Main = new ContainerNode(),
        Alternative = new ContainerNode()
      };
      Output.Add(variant);
      return OpenScope(ContextType.Collection, (ContainerNode) variant.Main);
    }

    public SqlCompilerOutputScope EnterAlternativeVariantScope(object id)
    {
      var variant = (VariantNode) Output.Current;
      if (variant.Id != id)
        throw new InvalidOperationException();
      return OpenScope(ContextType.Collection, (ContainerNode) variant.Alternative);
    }

    public SqlCompilerOutputScope EnterCycleBodyScope(object id, string delimiter)
    {
      var cycle = new CycleNode(id) {
        Body = new ContainerNode(),
        EmptyCase = new ContainerNode(),
        Delimiter = delimiter
      };
      Output.Add(cycle);
      return OpenScope(ContextType.Collection, (ContainerNode) cycle.Body);
    }

    public SqlCompilerOutputScope EnterCycleEmptyCaseScope(object id)
    {
      var cycle = (CycleNode) Output.Current;
      if (cycle.Id != id)
        throw new InvalidOperationException();
      return OpenScope(ContextType.Collection, (ContainerNode) cycle.EmptyCase);
    }

    private SqlCompilerOutputScope OpenScope(ContextType type)
    {
      var container = new ContainerNode();
      Output.Add(container);
      return OpenScope(type, container);
    }

    private SqlCompilerOutputScope OpenScope(ContextType type, ContainerNode container)
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

    internal SqlCompilerContext(SqlCompilerConfiguration configuration)
    {
      NamingOptions = SqlCompilerNamingOptions.TableQualifiedColumns;
      if (configuration.ForcedAliasing)
        NamingOptions |= SqlCompilerNamingOptions.TableAliasing;

      TableNameProvider = new SqlTableNameProvider(this);
      ParameterNameProvider = new SqlParameterNameProvider(configuration);
      traversalStack = new Stack<SqlNode>();
      traversalTable = new HashSet<SqlNode>();
      Output = new ContainerNode();
    }
  }
}
