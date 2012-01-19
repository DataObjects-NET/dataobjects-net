// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Text;

namespace Xtensive.Sql.Compiler
{
  internal sealed class Compressor : NodeVisitor
  {
    private readonly char newLineEnd;
    private char last;
    private byte indent;
    private StringBuilder buffer;
    private Node root;
    private Node current;

    public static Node Process(SqlTranslator translator, ContainerNode node)
    {
      var compressor = new Compressor(translator);
      compressor.CreateBuffer();
      compressor.VisitNodeSequence(node);
      compressor.FlushBuffer();
      return compressor.root;
    }

    #region Private / internal methods

    private void CreateBuffer()
    {
      buffer = new StringBuilder();
    }

    private void FlushBuffer()
    {
      if (buffer==null)
        return;
      string text = buffer.ToString();
      buffer = null;
      if (string.IsNullOrEmpty(text))
        return;
      AppendNode(new TextNode(text));
    }

    private void ResetLast()
    {
      last = '\0';
    }
   
    private void AppendNode(Node node)
    {
      if (current==null) {
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
      last = newLineEnd;
    }

    private void AppendSpace()
    {
      if (!(last==' ' || last==newLineEnd || last=='(')) {
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

    private Node VisitBranch(Node node)
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

    private void BeginNonTextNode()
    {
      AppendSpace();
      FlushBuffer(); 
    }

    private void EndNonTextNode()
    {
      CreateBuffer();
      ResetLast(); 
    }

    #endregion

    #region NodeVisitor Members

    public override void Visit(TextNode node)
    {
      AppendSpace();
      Append(node.Text);
    }

    public override void Visit(ContainerNode node)
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

    public override void Visit(DelimiterNode node)
    {
      switch (node.Type) {
      case SqlDelimiterType.Column:
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
      BeginNonTextNode();
      var variant = new VariantNode(node.Id);
      variant.Main = VisitBranch(node.Main);
      variant.Alternative = VisitBranch(node.Alternative);
      AppendNode(variant);
      EndNonTextNode();
    }

    public override void Visit(PlaceholderNode node)
    {
      BeginNonTextNode();
      AppendNode(new PlaceholderNode(node.Id));
      EndNonTextNode();
    }

    public override void Visit(CycleItemNode node)
    {
      BeginNonTextNode();
      AppendNode(new CycleItemNode(node.Index));
      EndNonTextNode();
    }

    public override void Visit(CycleNode node)
    {
      BeginNonTextNode();
      var cycle = new CycleNode(node.Id);
      cycle.Body = VisitBranch(node.Body);
      cycle.EmptyCase = VisitBranch(node.EmptyCase);
      cycle.Delimiter = node.Delimiter;
      AppendNode(cycle);
      EndNonTextNode();
    }

    #endregion


    // Constructors

    private Compressor(SqlTranslator translator)
    {
      newLineEnd = translator.NewLine[translator.NewLine.Length - 1];
      last = newLineEnd;
    }
  }
}