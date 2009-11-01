// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Dom.Compiler
{
  /// <summary>
  /// Represents a <see cref="SqlCompiler"/> compilation results.
  /// </summary>
  public class SqlCompilerResults
  {
    private readonly string commandText;
//    private ReadOnlyCollection<SqlParameterRef> parameters;

    /// <summary>
    /// Gets the textual representation of Sql.Dom statement compilation.
    /// </summary>
    /// <value>The Sql text command.</value>
    public string CommandText
    {
      get { return commandText; }
    }

//    /// <summary>
//    /// Gets the parameters.
//    /// </summary>
//    /// <value>The parameters.</value>
//    public ReadOnlyCollection<SqlParameterRef> Parameters
//    {
//      get { return parameters; }
//    }

    internal SqlCompilerResults(string commandText)
    {
      this.commandText = commandText;
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
    /// </returns>
    public override string ToString()
    {
      return commandText;
    }
  }
}
