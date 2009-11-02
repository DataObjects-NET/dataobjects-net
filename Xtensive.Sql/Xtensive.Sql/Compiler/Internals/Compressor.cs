// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Text;

namespace Xtensive.Sql.Compiler.Internals
{
  internal class Compressor : NodeVisitor
  {
    private char last = '\n';
    private byte indent;
    private StringBuilder buffer;
    private Node root;
    private Node current;

    public Node Compress(NodeContainer node)
    {
      CreateBuffer();
      VisitNodeSequence(node);
      FlushBuffer();
      return root;
    }

    #region Private methods

    private void CreateBuffer()
    {
      buffer = new StringBuilder();
    }

    private void FlushBuffer()
    {
      if (buffer == null)
        return;
      string text = buffer.ToString();
      buffer = null;
      if (string.IsNullOrEmpty(text))
        return;
      AppendNode(new TextNode(text));
    }
   
    private void AppendNode(Node node)
    {
      if (current == null) {
        current = node;
        root = node;
      }
      else {
        current.Next = node;
        current = node;
      }
    }

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

    private Node VisitVariantNode(Node node)
    {
      var originalCurrent = current;
      var originalRoot = root;
      root = null;
      current = null;
      try {
        CreateBuffer();
        VisitNodeSequence(node);
        FlushBuffer();
        return root;
      }
      finally {
        buffer = null;
        root = originalRoot;
        current = originalCurrent;
      }
    }

    #endregion

    #region NodeVisitor Members

    public override void Visit(TextNode node)
    {
      AppendSpace();
      Append(node.Text);
    }

    public override void Visit(NodeContainer node)
    {
      if (node.RequireIndent) {
        indent++;
        buffer.AppendLine();
        AppendIndent();
      }
      VisitNodeSequence(node.Child);
      if (node.RequireIndent)
        indent--;
    }

    public override void Visit(NodeDelimiter node)
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

    public override void Visit(VariantNode node)
    {
      FlushBuffer();
      var variant = new VariantNode(node.Key);
      variant.Main = VisitVariantNode(node.Main);
      variant.Alternative = VisitVariantNode(node.Alternative);
      AppendNode(variant);
      CreateBuffer();
    }

    #endregion
  }
}