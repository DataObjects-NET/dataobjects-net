// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;
using Xtensive.Sql.Compiler.Internals;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Compiler
{
  /// <summary>
  /// Represents a <see cref="SqlCompiler"/> compilation results.
  /// </summary>
  public sealed class SqlCompilationResult
  {
    private readonly Node resultNode;
    private readonly string resultText;
    private readonly IDictionary<object, string> parameterNames;

    /// <inheritdoc/>
    public override string ToString()
    {
      return GetCommandText();
    }

    /// <summary>
    /// Gets the name of the <paramref name="parameter"/> assigned during compilation.
    /// All explicitly named parameters are not searched by this method.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>Assigned name.</returns>
    public string GetParameterName(object parameter)
    {
      return parameterNames[parameter];
    }

    /// <summary>
    /// Gets the textual representation of Sql.Dom statement compilation.
    /// </summary>
    /// <value>The Sql text command.</value>
    public string GetCommandText()
    {
      if (resultText != null)
        return resultText;
      return new VariantCompiler(resultNode).Compile();
    }

    /// <summary>
    /// Gets the textual representation of Sql.Dom statement compilation.
    /// All variants are chosen according to a <paramref name="variantKeys"/>.
    /// </summary>
    /// <param name="variantKeys">Keys that determine which variants are to be used.</param>
    /// <returns></returns>
    public string GetCommandText(IEnumerable<object> variantKeys)
    {
      if (resultText != null)
        return resultText;
      return new VariantCompiler(resultNode, variantKeys).Compile();
    }

    internal SqlCompilationResult(Node result, IDictionary<object, string> parameterNames)
    {
      if (result == null) {
        resultText = string.Empty;
        return;
      }
      var text = result as TextNode;
      if (text != null && text.Next == null)
        resultText = text.Text;
      else
        resultNode = result;
      this.parameterNames = parameterNames;
    }
  }
}
