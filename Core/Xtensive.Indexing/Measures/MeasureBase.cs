// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.22

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Base class for measures.
  /// </summary>
  /// <typeparam name="TItem">Type of measured item.</typeparam>
  /// <typeparam name="TResult">Type of measure value.</typeparam>
  [Serializable]
  public abstract class MeasureBase<TItem, TResult> : IMeasure<TItem, TResult>
  {
    private string name;
    private readonly Converter<TItem, TResult> resultExtractor;
    private TResult result;
    private bool hasResult;

    /// <inheritdoc/>
    public string Name
    {
      [DebuggerStepThrough]
      get { return name; }
    }

    /// <inheritdoc/>
    public Converter<TItem, TResult> ResultExtractor
    {
      [DebuggerStepThrough]
      get { return resultExtractor; }
    }

    /// <inheritdoc/>
    public TResult Result {
      [DebuggerStepThrough]
      get {
        if (!hasResult)
          throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
        return result;
      }
      [DebuggerStepThrough]
      protected set {
        result = value;
        hasResult = true;
      }
    }

    /// <inheritdoc/>
    object IMeasure<TItem>.Result
    {
      [DebuggerStepThrough]
      get { return Result; }
    }

    /// <inheritdoc/>
    public bool HasResult
    {
      [DebuggerStepThrough]
      get { return hasResult; }
    }

    /// <inheritdoc/>
    public bool Add(TItem item)
    {
      return Add(ResultExtractor(item));
    }

    /// <inheritdoc/>
    public bool Subtract(TItem item)
    {
      return Subtract(ResultExtractor(item));
    }

    /// <inheritdoc/>
    public virtual void Reset()
    {
      hasResult = false;
      result = default(TResult);
    }

    #region Abstract methods

    /// <inheritdoc/>
    public abstract IMeasure<TItem> CreateNew();

    /// <inheritdoc/>
    public IMeasure<TItem> CreateNew(string newName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(newName, "newName");
      MeasureBase<TItem, TResult> measure = (MeasureBase<TItem, TResult>)CreateNew();
      measure.name = newName;
      return measure;
    }

    /// <inheritdoc/>
    public abstract IMeasure<TItem, TResult> CreateNew(TResult result);

    /// <inheritdoc/>
    public abstract IMeasure<TItem> Add(IMeasure<TItem> measure);

    /// <inheritdoc/>  
    public abstract IMeasure<TItem> Subtract(IMeasure<TItem> measure);

    /// <inheritdoc/>
    public abstract bool AddWith(IMeasure<TItem> measure);

    /// <inheritdoc/>
    public abstract bool SubtractWith(IMeasure<TItem> measure);

    /// <summary>
    /// Adds the specified result to the current measure.
    /// </summary>
    /// <param name="extracted">Extracted result.</param>
    protected abstract bool Add(TResult extracted);

    /// <summary>
    /// Subtracts the specified result from the current measure.
    /// </summary>
    /// <param name="extracted">Extracted result.</param>
    protected abstract bool Subtract(TResult extracted);

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name"><see cref="Name"/> property value.</param>
    /// <param name="resultExtractor"><see cref="ResultExtractor"/> property value.</param>
    protected MeasureBase(string name, Converter<TItem, TResult> resultExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      ArgumentValidator.EnsureArgumentNotNull(resultExtractor, "resultExtractor");
      this.name = name;
      this.resultExtractor = resultExtractor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="name"><see cref="Name"/> property value.</param>
    /// <param name="resultExtractor"><see cref="ResultExtractor"/> property value.</param>
    /// <param name="result">Initial <see cref="Result"/> property value.</param>
    protected MeasureBase(string name, Converter<TItem, TResult> resultExtractor, TResult result)
      : this (name, resultExtractor)
    {
      Result = result;
    }
  }
}
