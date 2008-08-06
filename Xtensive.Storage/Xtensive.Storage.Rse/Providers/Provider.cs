// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>.
  /// </summary>
  [Serializable]
  public abstract class Provider : 
    IEnumerable<Tuple>,
    IInitializable,
    IHasServices
  {
    private RecordSetHeader header;

    /// <summary>
    /// Gets or sets the source providers 
    /// "consumed" by this provider to produce results of current provider.
    /// </summary>
    public Provider[] Sources { get; private set; }

    /// <summary>
    /// Gets the header of the record sequence this provide produces.
    /// </summary>
    public RecordSetHeader Header {
      get {
        EnsureHeaderIsBuilt();
        return header;
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

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    /// <summary>
    /// Performs initialization of the provider if type of this is <paramref name="constructedProviderType"/>.
    /// </summary>
    /// <param name="constructedProviderType">Type of the constructed provider.</param>
    protected void Initialize(Type constructedProviderType)
    {
      if (constructedProviderType==GetType())
        Initialize();
    }

    /// <summary>
    /// Performs initialization of the provider.
    /// </summary>
    protected abstract void Initialize();

    private void EnsureHeaderIsBuilt()
    {
      if (header == null) lock (this) if (header == null)
        header = BuildHeader();
    }
      
    #region ToString() implementation

    private const string ProviderTypeSuffix = "Provider";
    private const int IndentingCharCount = 2;

    /// <inheritdoc/>
    public sealed override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      BuildString(sb, 0);
      return sb.ToString();
    }

    private void BuildString(StringBuilder sb, int indent)
    {
      BuildTitle(sb, indent);

      foreach (Provider source in Sources)
        source.BuildString(sb, indent + IndentingCharCount);
    }

    private void BuildTitle(StringBuilder sb, int indent)
    {      
      string providerName = GetType().Name.TryCutSuffix(ProviderTypeSuffix);
      string parameters = GetStringParameters();

      sb.Append(new string(' ', indent));
      sb.Append(providerName);

      if (!parameters.IsNullOrEmpty())
        sb.Append(string.Format(" ({0})", parameters));

      sb.Append(Environment.NewLine);
    }

    /// <summary>
    /// Gets the provider parameters for the <see cref=" ToString"/> method.    
    /// </summary>
    /// <returns>Provider parameters represented by single line string.</returns>
    public virtual string GetStringParameters()
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
