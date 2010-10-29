// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.20

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Comparison;
using Xtensive.Configuration;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Index configuration. 
  /// Used by <see cref="IndexBase{TKey,TItem}"/> and its descendants
  /// to unify index initialization and configuration.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TItem">The type of the index item.</typeparam>
  [Serializable]
  public class IndexConfigurationBase<TKey, TItem>: ConfigurationBase,
    IHasKeyExtractor<TKey, TItem>,
    IHasKeyComparers<TKey>,
    IDeserializationCallback
  {
    private Converter<TItem, TKey> keyExtractor;
    private AdvancedComparer<TKey> keyComparer;
    [NonSerialized] 
    private AdvancedComparer<Entire<TKey>> entireKeyComparer;
    [NonSerialized]
    private Func<Entire<TKey>, TKey, int> asymmetricKeyCompare;
    private IMeasureSet<TItem> measures = new MeasureSet<TItem>();
    private UrlInfo location;

    /// <summary>
    /// Gets or sets the location of the index.
    /// </summary>
    public UrlInfo Location
    {
      [DebuggerStepThrough]
      get { return location; }
      set
      {
        this.EnsureNotLocked();
        location = value;
      }
    }

    /// <summary>
    /// Gets the set of measures.
    /// </summary>
    public IMeasureSet<TItem>Measures
    {
      [DebuggerStepThrough]
      get { return measures; }
    }

    /// <summary>
    /// Gets or sets the key extractor associated with the index.
    /// </summary>
    public Converter<TItem, TKey> KeyExtractor
    {
      [DebuggerStepThrough]
      get { return keyExtractor; }
      set {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        keyExtractor = value;
      }
    }

    /// <summary>
    /// Gets or sets the key comparer.
    /// </summary>
    public AdvancedComparer<TKey> KeyComparer
    {
      [DebuggerStepThrough]
      get { return keyComparer; }
      set {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        keyComparer = value;
        if (entireKeyComparer==null && keyComparer.Provider!=null)
          entireKeyComparer = keyComparer.Provider.GetComparer<Entire<TKey>>().ApplyRules(keyComparer.ComparisonRules);
        if (asymmetricKeyCompare==null && entireKeyComparer!=null)
          asymmetricKeyCompare = entireKeyComparer.GetAsymmetric<TKey>();
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="Entire{T}"/> comparer for <typeparamref name="TKey"/> type.
    /// </summary>
    public AdvancedComparer<Entire<TKey>> EntireKeyComparer
    {
      [DebuggerStepThrough]
      get { return entireKeyComparer; }
      set {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        entireKeyComparer = value;
        asymmetricKeyCompare = entireKeyComparer.GetAsymmetric<TKey>();
      }
    }

    /// <summary>
    /// Gets or sets the delegate used to compare 
    /// <see cref="Entire{T}"/> for <typeparamref name="TKey"/> type and <typeparamref name="TKey"/> type.
    /// </summary>
    public Func<Entire<TKey>, TKey, int> AsymmetricKeyCompare
    {
      [DebuggerStepThrough]
      get { return asymmetricKeyCompare; }
      set {
        this.EnsureNotLocked();
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        asymmetricKeyCompare = value;
      }
    }

    /// <inheritdoc/>
    public override void Validate()
    {
      if (keyExtractor==null)
        throw Exceptions.NotInitialized("KeyExtractor");
      if (keyComparer == null)
        throw Exceptions.NotInitialized("KeyComparer");
    }

    #region Clone implementation

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new IndexConfigurationBase<TKey, TItem>();
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      IndexConfigurationBase<TKey, TItem> indexConfigurationBase = (IndexConfigurationBase<TKey, TItem>)source;
      keyExtractor         = indexConfigurationBase.keyExtractor;
      keyComparer          = indexConfigurationBase.keyComparer;
      entireKeyComparer    = indexConfigurationBase.entireKeyComparer;
      asymmetricKeyCompare = indexConfigurationBase.asymmetricKeyCompare;
      measures             = (MeasureSet<TItem>)indexConfigurationBase.measures.Clone();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public IndexConfigurationBase()
    {
      measures.Add(new CountMeasure<TItem, long>());
      measures.Add(new SizeMeasure<TItem>());
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public IndexConfigurationBase(Converter<TItem, TKey> keyExtractor)
      : this()
    {
      KeyExtractor = keyExtractor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="keyComparer"><see cref="KeyComparer"/> property value.</param>
    public IndexConfigurationBase(Converter<TItem, TKey> keyExtractor, AdvancedComparer<TKey> keyComparer)
      : this(keyExtractor)
    {
      KeyComparer  = keyComparer;
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization"/>
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      IDeserializationCallback deserializationCallback = keyComparer.Provider as IDeserializationCallback;
      if (deserializationCallback!=null)
        deserializationCallback.OnDeserialization(sender);
      entireKeyComparer = keyComparer.Provider.GetComparer<Entire<TKey>>().ApplyRules(keyComparer.ComparisonRules);
      asymmetricKeyCompare = entireKeyComparer.GetAsymmetric<TKey>();
    }
  }
}