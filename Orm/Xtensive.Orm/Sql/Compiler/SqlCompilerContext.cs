// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// SQL compiler context.
  /// </summary>
  public class SqlCompilerContext
  {
    private SqlNode[] traversalPath;
    private readonly Stack<SqlNode> traversalStack = new Stack<SqlNode>();
    private readonly HashSet<SqlNode> traversalTable = new HashSet<SqlNode>();

    public bool ParametrizeSchemaNames { get; set; }

    public SqlTableNameProvider TableNameProvider { get; private set; }

    public SqlParameterNameProvider ParameterNameProvider { get; private set; }

    public ContainerNode Output { get; private set; }

    public SqlCompilerNamingOptions NamingOptions { get; private set; }

    public SqlNodeActualizer SqlNodeActualizer { get; private set; }

    public SqlNode[] GetTraversalPath() =>
      traversalPath ??= traversalStack.ToArray();

    public bool HasOptions(SqlCompilerNamingOptions requiredOptions)
    {
      return (NamingOptions & requiredOptions)==requiredOptions;
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
      if (!traversalTable.Add(node))
        throw new SqlCompilerException(Strings.ExCircularReferenceDetected);
      traversalStack.Push(node);
      return OpenScope(ContextType.Node);
    }

    public SqlCompilerOutputScope EnterCollectionScope()
    {
      return OpenScope(ContextType.Collection);
    }

    public SqlCompilerOutputScope EnterMainVariantScope(object id)
    {
      var mainContainerNode = new ContainerNode();
      Output.Add(new VariantNode(id, mainContainerNode, new ContainerNode()));
      return OpenScope(ContextType.Collection, mainContainerNode);
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
      var body = new ContainerNode();
      Output.Add(new CycleNode(id, body, new ContainerNode(), delimiter));
      return OpenScope(ContextType.Collection, body);
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
      return OpenScope(type, Output);
    }

    private SqlCompilerOutputScope OpenScope(ContextType type, ContainerNode container)
    {
      traversalPath = null;
      var scope = new SqlCompilerOutputScope(this, type);
      if (Output != container) {
        Output = container;
      }
      else {
        Output.StartOfCollection = true;
        if (Output.RequireIndent) {
          Output.Indent++;
        }
      }
      return scope;
    }

    internal void CloseScope(SqlCompilerOutputScope scope)
    {
      traversalPath = null;
      if (Output != scope.ParentContainer) {
        Output.FlushBuffer();
        Output = scope.ParentContainer;
      }
      else {
        Output.StartOfCollection = scope.StartOfCollection;
        if (Output.RequireIndent) {
          Output.Indent--;
        }
      }
      if (scope.Type == ContextType.Node)
        traversalTable.Remove(traversalStack.Pop());
    }

    #endregion


    // Constructor

    internal SqlCompilerContext(SqlCompilerConfiguration configuration)
    {
      NamingOptions = SqlCompilerNamingOptions.TableQualifiedColumns | SqlCompilerNamingOptions.TableAliasing;
      if (configuration.DatabaseQualifiedObjects)
        NamingOptions |= SqlCompilerNamingOptions.DatabaseQualifiedObjects;

      TableNameProvider = new SqlTableNameProvider(this);
      ParameterNameProvider = new SqlParameterNameProvider(configuration);
      Output = new ContainerNode();
      SqlNodeActualizer = new SqlNodeActualizer(configuration.DatabaseMapping, configuration.SchemaMapping);
      ParametrizeSchemaNames = configuration.ParametrizeSchemaNames;
    }
  }
}
