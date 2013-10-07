// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections.Generic;
using Xtensive.Core;


namespace Xtensive.Conversion
{
  [Serializable]
  internal class ConvertingEnumerator<T1, T2> : IEnumerator<T2>
  {
    private IEnumerator<T1> innerEnumerator;
    private readonly Converter<T1, T2> converter;
    private T2 current;
    private bool currentIsValid = false;

    internal T1 InnerCurrent
    {
      get { return innerEnumerator.Current; }
    }
    
    object System.Collections.IEnumerator.Current
    {
      get { return Current; }
    }

    public T2 Current
    {
      get {
        if (!currentIsValid) {
          current = converter(innerEnumerator.Current);
          currentIsValid = true;
        }
        return current;
      }
    }    

    public bool MoveNext()
    {
      currentIsValid = false;
      return innerEnumerator.MoveNext();
    }

    public void Reset()
    {
      currentIsValid = false;
      innerEnumerator.Reset();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="innerEnumerator">The inner enumerator.</param>
    /// <param name="converter">The converter.</param>
    public ConvertingEnumerator(IEnumerator<T1> innerEnumerator, Converter<T1, T2> converter)
    {
      ArgumentValidator.EnsureArgumentNotNull(innerEnumerator, "innerEnumerator");
      ArgumentValidator.EnsureArgumentNotNull(converter, "converter");
      this.innerEnumerator = innerEnumerator;
      this.converter = converter;
    }


    // Destructor
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      currentIsValid = false;
      innerEnumerator.Dispose();
      innerEnumerator = null;
    }
  }
}