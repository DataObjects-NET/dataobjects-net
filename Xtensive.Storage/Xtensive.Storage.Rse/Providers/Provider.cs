// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>.
  /// </summary>
  [Serializable]
  public abstract class Provider : 
    IEnumerable<Tuple>,
    IHasServices,
    IInitializable
  {
    protected const string ToString_ProviderTypeSuffix = "Provider";
    protected const string ToString_Parameters = " ({0})";
    protected const int    ToString_IndentSize = 2;
    private Provider[] sources;
    private RecordSetHeader header;
    private bool isInitialized;

    /// <summary>
    /// Gets or sets the source providers 
    /// "consumed" by this provider to produce results of current provider.
    /// </summary>
    public Provider[] Sources {
      [DebuggerStepThrough]
      get { return sources; }
      [DebuggerStepThrough]
      protected set {
        if (sources!=null)
          throw Exceptions.AlreadyInitialized("Sources");
        sources = value;
      }
    }

    /// <summary>
    /// Gets or sets the header of the record sequence this provide produces.
    /// </summary>
    public RecordSetHeader Header
    {
      [DebuggerStepThrough]
      get { return header; }
      [DebuggerStepThrough]
      protected set {
        if (header!=null)
          throw Exceptions.AlreadyInitialized("Header");
        header = value;
      }
    }

    /// <inheritdoc/>
    public abstract T GetService<T>()
      where T : class;

    /// <summary>
    /// Builds the <see cref="Header"/>.
    /// This method is invoked just once on each provider.
    /// </summary>
    /// <returns>Newly created <see cref="RecordSetHeader"/> to assign to <see cref="Header"/> property.</returns>
    protected abstract RecordSetHeader BuildHeader();

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public abstract IEnumerator<Tuple> GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    /// <summary>
    /// Performs initialization (see <see cref="Initialize()"/>) of the provider 
    /// if type of <see langword="this" /> is the same as <paramref name="ctorType"/>.
    /// Invoked by <see cref="InitializableAttribute"/> aspect in the epilogue of any 
    /// constructor of this type and its ancestors.
    /// </summary>
    /// <param name="ctorType">The type, which constructor has invoked this method.</param>
    protected void Initialize(Type ctorType)
    {
      if (ctorType==GetType() && !isInitialized) {
        isInitialized = true;
        Initialize();
      }
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

    /// <summary>
    /// Appends the provider's representation to the specified <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="indent">The indent.</param>
    protected internal virtual void AppendBodyTo(StringBuilder sb, int indent)
    {
      AppendTitleTo(sb, indent);
      indent = indent + ToString_IndentSize;
      foreach (Provider source in Sources)
        source.AppendBodyTo(sb, indent);
    }

    /// <summary>
    /// Appends the provider's title representation to the specified <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to use.</param>
    /// <param name="indent">The indent.</param>
    protected internal void AppendTitleTo(StringBuilder sb, int indent)
    {      
      sb.Append(new string(' ', indent))
        .Append(TitleToString())
        .AppendLine();
    }

    /// <summary>
    /// Gets the string representation of provider title
    /// for the <see cref="ToString"/> method.    
    /// </summary>
    /// <returns>Provider title as a single line string.</returns>
    public virtual string TitleToString()
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
    public virtual string ParametersToString()
    {
      return string.Empty;
    }

    #endregion


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sources"><see cref="Sources"/> property value.</param>
    protected Provider(params Provider[] sources)
    {
      Sources = sources;
    }
  }
}
