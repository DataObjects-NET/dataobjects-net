// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Sql.Compiler
{
  internal sealed class Compressor : NodeVisitor
  {
    private readonly char newLineEnd;
    private char last;
    private byte indent;
    private readonly StringBuilder buffer = new StringBuilder();

    private List<Node> children = new List<Node>();

    public IReadOnlyList<Node> Children => children;

    public static IReadOnlyList<Node> Process(SqlTranslator translator, ContainerNode node)
    {
      var compressor = new Compressor(translator);
      compressor.Visit(node);
      compressor.FlushBuffer();
      return compressor.Children;
    }

    #region Private / internal methods

    private void FlushBuffer()
    {
      if (buffer.Length > 0) {
        AppendNode(new TextNode(buffer.ToString()));
        buffer.Clear();
      }
    }

    private void ResetLast()
    {
      last = '\0';
    }

    private void AppendNode(Node node)
    {
      children.Add(node);
    }

    private void Append(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      char first = text[0];
      if (first == ')' && last == ' ')
        buffer.Length--;
      last = text[text.Length - 1];
      buffer.Append(text);
    }

    private void AppendLine(string text)
    {
      buffer.AppendLine(text);
      last = newLineEnd;
    }

    private void AppendSpace()
    {
      if (!(last == ' ' || last == newLineEnd || last == '(')) {
        buffer.Append(' ');
        last = ' ';
      }
    }

    private void AppendIndent()
    {
      if (indent > 0) {
        for (int i = indent; i-- > 0;) {
          buffer.Append("  ");
        }
        last = ' ';
      }
    }

    private IEnumerable<Node> VisitBranch(IEnumerable<Node> nodes)
    {
      var originalChildren = children;
      children = new List<Node>();
      try {
        VisitNodes(nodes);
        FlushBuffer();
        return Children;
      }
      finally {
        children = originalChildren;
      }
    }

    private void BeginNonTextNode()
    {
      AppendSpace();
      FlushBuffer();
    }

    private void EndNonTextNode()
    {
      ResetLast();
    }

    #endregion

    #region NodeVisitor Members

    public override void Visit(TextNode node)
    {
      AppendSpace();
      if (buffer?.Length > 0) {
        FlushBuffer();
        ResetLast();
      }
      AppendNode(node); // Append node instead of string copy to minimize amount of work and allocations.
    }

    public override void Visit(ContainerNode node)
    {
      if (node.RequireIndent) {
        indent++;
        buffer.AppendLine();
        AppendIndent();
      }
      VisitNodes(node.Children);
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
      AppendNode(new VariantNode(node.Id, VisitBranch(node.Main), VisitBranch(node.Alternative)));
      EndNonTextNode();
    }

    public override void Visit(PlaceholderNode node)
    {
      BeginNonTextNode();
      AppendNode(node);
      EndNonTextNode();
    }

    public override void Visit(CycleItemNode node)
    {
      BeginNonTextNode();
      AppendNode(node);
      EndNonTextNode();
    }

    public override void Visit(CycleNode node)
    {
      BeginNonTextNode();
      AppendNode(new CycleNode(node.Id, VisitBranch(node.Body), VisitBranch(node.EmptyCase), node.Delimiter));
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