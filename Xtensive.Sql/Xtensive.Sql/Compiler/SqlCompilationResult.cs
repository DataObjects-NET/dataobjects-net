// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Collections.Generic;
using Xtensive.Sql.Compiler.Internals;

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
    private volatile int lastResultLength;

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
    /// Gets the textual representation of SQL DOM statement compilation.
    /// </summary>
    /// <value>The SQL text command.</value>
    public string GetCommandText()
    {
      if (resultText!=null)
        return resultText;
      string result = PostCompiler.Compile(resultNode, null, null, lastResultLength);
      lastResultLength = result.Length;
      return result;
    }

    /// <summary>
    /// Gets the textual representation of SQL DOM statement compilation.
    /// All variants are chosen according to a <paramref name="variantKeys"/>.
    /// </summary>
    /// <param name="variantKeys">Keys that determine which variants are to be used.</param>
    /// <returns>he SQL text command.</returns>
    public string GetCommandText(IEnumerable<object> variantKeys)
    {
      if (resultText!=null)
        return resultText;
      string result = PostCompiler.Compile(resultNode, variantKeys, null, lastResultLength);
      lastResultLength = result.Length;
      return result;
    }

    /// <summary>
    /// Gets the textual representation of SQL DOM statement compilation.
    /// All delayed parameter names are choosen according to a <paramref name="holesMapping"/>.
    /// </summary>
    /// <param name="holesMapping">A dictionary that assigns value for each hole in query.</param>
    /// <returns>The SQL text command.</returns>
    public string GetCommandText(IDictionary<object, string> holesMapping)
    {
      if (resultText!=null)
        return resultText;
      string result = PostCompiler.Compile(resultNode, null, holesMapping, lastResultLength);
      lastResultLength = result.Length;
      return result;
    }

    /// <summary>
    /// Gets the textual representation of SQL DOM statement compilation.
    /// All delayed parameter names are choosen according to a <paramref name="holesMapping"/>.
    /// All variants are chosen according to a <paramref name="variantKeys"/>.
    /// </summary>
    /// <param name="variantKeys">Keys that determine which variants are to be used.</param>
    /// <param name="holesMapping">A dictionary that assigns for each parameter.</param>
    /// <returns>The SQL text command.</returns>
    public string GetCommandText(IEnumerable<object> variantKeys, IDictionary<object, string> holesMapping)
    {
      if (resultText!=null)
        return resultText;
      string result = PostCompiler.Compile(resultNode, variantKeys, holesMapping, lastResultLength);
      lastResultLength = result.Length;
      return result;
    }


    // Constructors

    internal SqlCompilationResult(Node result, IDictionary<object, string> parameterNames)
    {
      if (result==null) {
        resultText = string.Empty;
        return;
      }
      var textNode = result as TextNode;
      if (textNode!=null && textNode.Next==null)
        resultText = textNode.Text;
      else
        resultNode = result;
      this.parameterNames = parameterNames;
    }
  }
}
