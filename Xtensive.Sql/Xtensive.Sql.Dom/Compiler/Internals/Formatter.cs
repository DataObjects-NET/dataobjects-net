// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Text;

namespace Xtensive.Sql.Dom.Compiler.Internals
{
  internal class Formatter : INodeVisitor
  {
    private readonly StringBuilder buffer = new StringBuilder(1024);
    private char last;
    private byte indent;

    public string Format(NodeContainer node)
    {
      buffer.Length = 0;
      last = '\n';
      node.AcceptVisitor(this);
      return buffer.ToString();
    }

    #region Private methods

    private void Append(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      char first = text[0];
      if (first==')' && last==' ')
        buffer.Length--;
      last = text[text.Length-1];
      buffer.Append(text);
    }

    private void AppendLine(string text)
    {
      buffer.AppendLine(text);
      last = '\n';
    }

    private void AppendSpace()
    {
      if (!(last==' ' || last=='\n' || last=='(')) {
        buffer.Append(' ');
        last = ' ';
      }
    }

    private void AppendIndent()
    {
      if (indent>0) {
        buffer.Append(new string(' ', indent*2));
        last = ' ';
      }
    }

    #endregion

    #region INodeVisitor Members

    public void Visit(TextNode node)
    {
      AppendSpace();
      Append(node.Text);
    }

    public void Visit(NodeContainer node)
    {
      if (node.RequireIndent) {
        indent++;
        buffer.AppendLine();
        AppendIndent();
      }
      Node n = node.Child;
      while (n!=null) {
        n.AcceptVisitor(this);
        n = n.Next;
      }
      if (node.RequireIndent)
        indent--;
    }

    public void Visit(NodeDelimiter node)
    {
      switch (node.Type) {
        case DelimiterType.Column:
          AppendLine(node.Text);
          AppendIndent();
          break;
        default:
          Append(node.Text);
          break;
      }
    }

    public void Visit(VariantNode node)
    {
      
    }

    #endregion
  }
}