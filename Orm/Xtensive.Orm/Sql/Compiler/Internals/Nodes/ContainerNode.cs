// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Xtensive.Sql.Compiler
{
  public interface IOutput
  {
    /// <summary>
    /// The <see cref="System.Text.StringBuilder"/> the output writes to.
    /// </summary>
    StringBuilder StringBuilder { get; }

    /// <summary>
    /// Appends given text.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>The <see cref="IOutput"/> instance with appended text.</returns>
    IOutput Append(string text);

    /// <summary>
    /// Appends given character.
    /// </summary>
    /// <param name="v">The character to append.</param>
    /// <returns>The output instance with appended character.</returns>
    IOutput Append(char v);

    /// <summary>
    /// Appends given <see cref="long"/> value.
    /// </summary>
    /// <param name="v">The value to append.</param>
    /// <returns>The <see cref="IOutput"/> instance with appended value.</returns>
    IOutput Append(long v);

    /// <summary>
    /// Appends given literal text.
    /// </summary>
    /// <param name="text">The literal text to append.</param>
    /// <returns>The <see cref="IOutput"/> instance with appended literal text.</returns>
    IOutput AppendLiteral(string text);

    /// <summary>
    /// Appends given literal character.
    /// </summary>
    /// <param name="v">The literal character to append.</param>
    /// <returns>The <see cref="IOutput"/> instance with appended literal character.</returns>
    IOutput AppendLiteral(char v);

    /// <summary>
    /// Adds text to the output as an opening punctuation, like method and function name with opening parenthesis,
    /// e.g 'MAX('. Should be used with <see cref="IOutput.AppendClosingPunctuation(string)"/>
    /// Some optimizations of inner state may happen.
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>The <see cref="IOutput"/> instance with appended text.</returns>
    IOutput AppendOpeningPunctuation(string text);

    /// <summary>
    /// Adds text to the output as a closing punctuation. Should be used in pair with <see cref="IOutput.AppendOpeningPunctuation(string)"/>
    /// Some optimizations of inner state may happen, e.g cut off last space. Use it with closing parenthesis like ")".
    /// </summary>
    /// <param name="text">The text to append.</param>
    /// <returns>The <see cref="IOutput"/> instance with given text</returns>
    IOutput AppendClosingPunctuation(string text);

    /// <summary>
    /// Appends space to the output.
    /// </summary>
    /// <returns>The <see cref="IOutput"/> instance with appended space.</returns>
    IOutput AppendSpace();

    /// <summary>
    /// Appends space unless current output state ends with space or other character
    /// that desqualfy output from appending space.
    /// </summary>
    /// <returns>The <see cref="IOutput"/> instance with appended space if in was nessesary.</returns>
    IOutput AppendSpaceIfNecessary();

    /// <summary>
    /// Appends new line text.
    /// </summary>
    /// <param name="text">The new line text.</param>
    /// <returns>The <see cref="IOutput"/> instance with appended new line text.</returns>
    IOutput AppendNewLine(string text);
  }

  /// <summary>
  /// Container node in SQL DOM query model.
  /// </summary>
  public class ContainerNode : Node, IOutput, IEnumerable<Node>
  {
    private static readonly IFormatProvider invarianCulture = CultureInfo.InvariantCulture;

    public readonly bool RequireIndent; // Never set

    private readonly StringBuilder stringBuilder = new();
    private readonly List<Node> children = new();

    private char? lastChar;
    private bool lastCharIsPunctuation;
    private bool lastNodeIsText = true;

    public IReadOnlyList<Node> Children
    {
      get {
        FlushBuffer();
        return children;
      }
    }

    public int Indent { get; set; }

    public bool StartOfCollection { get; set; } = true;

    public Node Current => Children.Count == 0 ? null : children[^1]; // logically children can be used here after Children flushed buffer

    /// <inheritdoc/>
    public StringBuilder StringBuilder
    {
      get {
        lastCharIsPunctuation = false;
        lastChar = null;
        return stringBuilder;
      }
    }

    public void AppendCycleItem(int index) => Add(new CycleItemNode(index));

    public void AppendPlaceholderWithId(object id) => Add(new PlaceholderNode(id));

    public void AppendIndent()
    {
      if (Indent > 0) {
        for (var i = Indent; i-- > 0;) {
          _ = Append("  ");
        }
        lastCharIsPunctuation = true;
      }
    }

    public void Add(Node node)
    {
      FlushBuffer();
      children.Add(node);
      lastNodeIsText = node.IsTextNode;
    }

    internal override void AcceptVisitor(NodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal void FlushBuffer()
    {
      if (stringBuilder.Length > 0) {
        children.Add(new TextNode(stringBuilder.ToString()));
        lastNodeIsText = true;
        lastChar = stringBuilder[^1];
        _ = stringBuilder.Clear();
        lastCharIsPunctuation = false;
      }
    }

    #region IOutput methods implementation

    /// <inheritdoc/>
    public IOutput Append(string text)
    {
      if (!string.IsNullOrEmpty(text)) {
        _ = stringBuilder.Append(text);
        lastCharIsPunctuation = false;
        lastChar = text[^1];
        StartOfCollection = false;
      }
      return this;
    }

    /// <inheritdoc/>
    public IOutput Append(char v)
    {
      _ = stringBuilder.Append(v);
      lastCharIsPunctuation = false;
      lastChar = v;
      StartOfCollection = false;
      return this;
    }

    /// <inheritdoc/>
    public IOutput Append(long v)
    {
      _ = stringBuilder.AppendFormat(invarianCulture, "{0}", v);
      lastCharIsPunctuation = false;
      lastChar = null;// char.MaxValue;//i highly doubt that this char will appear in translation :-);
      StartOfCollection = false;
      return this;
    }

    /// <inheritdoc/>
    public IOutput AppendLiteral(string text)
    {
      if (!string.IsNullOrEmpty(text)) {
        _ = stringBuilder.Append(text);
        lastCharIsPunctuation = false;
        lastChar = text[^1];
        StartOfCollection = false;
      }
      return this;
    }

    /// <inheritdoc/>
    public IOutput AppendLiteral(char v)
    {
      _ = stringBuilder.Append(v);
      lastCharIsPunctuation = false;
      lastChar = v; // do we need to set it here
      StartOfCollection = false;
      return this;
    }

    /// <inheritdoc/>
    public IOutput AppendOpeningPunctuation(string text)
    {
      if (!string.IsNullOrEmpty(text)) {
        _ = Append(text);
        lastCharIsPunctuation = true;
        lastChar = text[^1];
      }
      return this;
    }

    /// <inheritdoc/>
    public IOutput AppendClosingPunctuation(string text)
    {
      if (!string.IsNullOrEmpty(text)) {
        if (lastChar == ' ' && stringBuilder.Length > 0) {
          stringBuilder.Length--;// Remove space before closing punctuation
        }
        _ = Append(text);
        lastCharIsPunctuation = true;
        lastChar = text[^1];
      }
      return this;
    }

    /// <inheritdoc/>
    public IOutput AppendSpace()
    {
      _ = stringBuilder.Append(' ');
      lastCharIsPunctuation = true;
      lastChar = ' ';
      StartOfCollection = false;
      return this;
    }

    /// <inheritdoc/>
    public IOutput AppendSpaceIfNecessary()
    {
      if (lastNodeIsText && (lastCharIsPunctuation || (lastChar is ' ' or '(' or '\n'))) {
        lastCharIsPunctuation = false;
        return this;
      }
      return AppendSpace();
    }

    /// <inheritdoc/>
    public IOutput AppendNewLine(string text)
    {
      //reduces chances of checking for new line character
      //mostly it is used for batches
      if (!string.IsNullOrEmpty(text)) {
        _ = stringBuilder.Append(text);
        lastCharIsPunctuation = true;
        lastChar = text[^1];
        StartOfCollection = false;
      }
      return this;
    }

    #endregion

    public IEnumerator<Node> GetEnumerator() => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Constructors

    public ContainerNode(bool requireIndent)
    {
      RequireIndent = requireIndent;
    }

    public ContainerNode()
    {
      RequireIndent = false;
    }
  }
}