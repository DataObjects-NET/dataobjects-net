// Copyright (C) 2008-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any query provider.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{DebuggerDisplayName}, Source count = {Sources.Length}")]
  public abstract class Provider
  {
    private const string ToString_ProviderTypeSuffix = "Provider";
    private const string ToString_Parameters = " ({0})";
    private const int    ToString_IndentSize = 2;

    private RecordSetHeader header;

    /// <summary>
    /// Gets <see cref="ProviderType"/> of the current instance.
    /// </summary>
    public ProviderType Type { get; }

    /// <summary>
    /// Gets or sets the source providers 
    /// "consumed" by this provider to produce results of current provider.
    /// </summary>
    public IReadOnlyList<Provider> Sources { [DebuggerStepThrough] get; }

    /// <summary>
    /// Gets or sets the header of the record sequence this provide produces.
    /// </summary>
    public RecordSetHeader Header
    {
      [DebuggerStepThrough]
      get { return header; }
      [DebuggerStepThrough]
      private set {
        if (header!=null)
          throw Exceptions.AlreadyInitialized("Header");
        header = value;
      }
    }

    /// <summary>
    /// Builds the <see cref="Header"/>.
    /// This method is invoked just once on each provider.
    /// </summary>
    /// <returns>Newly created <see cref="RecordSetHeader"/> to assign to <see cref="Header"/> property.</returns>
    protected abstract RecordSetHeader BuildHeader();

    private string DebuggerDisplayName {
      get { return GetType().GetShortName(); }
    }

    /// <summary>
    /// Performs initialization of the provider.
    /// </summary>
    protected virtual void Initialize()
    {
      Header = BuildHeader();
    }

    #region ToString method

    /// <inheritdoc/>
    public sealed override string ToString()
    {
      var sb = new StringBuilder();
      AppendBodyTo(sb, 0);
      return sb.ToString();
    }

    private void AppendBodyTo(StringBuilder sb, int indent)
    {
      AppendTitleTo(sb, indent);
      indent = indent + ToString_IndentSize;
      foreach (var source in Sources)
        source.AppendBodyTo(sb, indent);
    }

    private void AppendTitleTo(StringBuilder sb, int indent)
    {      
      sb.Append(TitleToString().Indent(indent))
        .AppendLine();
    }

    private string TitleToString()
    {
      var sb = new StringBuilder();
      string providerName = GetType().GetShortName().TryCutSuffix(ToString_ProviderTypeSuffix);
      string parameters = ParametersToString();

      sb.Append(providerName);
      if (!parameters.IsNullOrEmpty())
        sb.AppendFormat(ToString_Parameters, parameters);
      return sb.ToString();
    }

    /// <summary>
    /// Gets the string representation of provider parameters 
    /// for the <see cref="ToString"/> method.
    /// </summary>
    /// <returns>Provider parameters as a single line string.</returns>
    protected virtual string ParametersToString()
    {
      return string.Empty;
    }

    #endregion


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type of the provider.</param>
    /// <param name="sources"><see cref="Sources"/> property value.</param>
    protected Provider(ProviderType type, IReadOnlyList<Provider> sources)
    {
      Type = type;
      Sources = sources;
    }
  }
}
