// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections;
using System.Collections.Generic;
using Xtensive.Sql.Dom.Compiler.Internals;
using Xtensive.Sql.Dom.Exceptions;
using Xtensive.Sql.Dom.Resources;

namespace Xtensive.Sql.Dom.Compiler
{
  public class SqlCompilerContext
  {
    private readonly NodeContainer root;
    private NodeContainer output;
    private Stack<SqlNode> traversalStack;
    private SqlNode[] traversalPath;
    private Hashtable traversalTable;
    private AliasProvider aliasProvider = new AliasProvider();

    /// <summary>
    /// Gets the traversal path.
    /// </summary>
    /// <value>The traversal path.</value>
    public SqlNode[] GetTraversalPath()
    {
      if (traversalPath==null)
        traversalPath = traversalStack.ToArray();
      return traversalPath;
    }

    public bool IsEmpty
    {
      get { return output.IsEmpty; }
    }

    internal NodeContainer Output
    {
      get { return output; }
    }

    public AliasProvider AliasProvider
    {
      get { return aliasProvider; }
    }

    public void AppendText(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      output.Add(new TextNode(text));
    }

    public void AppendDelimiter(string text)
    {
      output.AppendDelimiter(text, DelimiterType.Row);
    }

    public void AppendDelimiter(string text, DelimiterType type)
    {
      output.Add(new NodeDelimiter(type, text));
    }

    public SqlCompilerScope EnterNode(SqlNode node)
    {
      traversalStack.Push(node);
      OnChangeContext();
      if (traversalTable.ContainsKey(node))
        throw new SqlCompilerException(Strings.ExCircularReferenceDetected);
      traversalTable.Add(node, traversalTable);
      return CreateScope(ContextType.Node);
    }

    public SqlCompilerScope EnterCollection()
    {
      OnChangeContext();
      return CreateScope(ContextType.Collection);
    }

    private void OnChangeContext()
    {
      if (traversalPath!=null)
        traversalPath = null;
    }

    private SqlCompilerScope CreateScope(ContextType type)
    {
      SqlCompilerScope scope = new SqlCompilerScope(this, output, type);
      NodeContainer nc = new NodeContainer();
      output.Add(nc);
      output = nc;
      return scope;
    }

    internal void DisposeScope(SqlCompilerScope scope)
    {
      output = scope.Output;
      OnChangeContext();
      if (scope.Type==ContextType.Node)
        traversalTable.Remove(traversalStack.Pop());
    }

    internal SqlCompilerContext()
    {
      traversalStack = new Stack<SqlNode>(128);
      traversalTable = new Hashtable(128);
      root = new NodeContainer();
      output = root;
    }
  }
}
