// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections.Generic;
namespace Xtensive.Core.Conversion
{
  [Serializable]
  internal class ConvertingEnumerator<T1, T2> : IEnumerator<T2>
  {
    private readonly IEnumerator<T1> innerEnumerator;
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

    public void Dispose()
    {
      currentIsValid = false;
      innerEnumerator.Dispose();
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

    public ConvertingEnumerator(IEnumerator<T1> innerEnumerator, Converter<T1, T2> converter)
    {
      ArgumentValidator.EnsureArgumentNotNull(innerEnumerator, "innerEnumerator");
      ArgumentValidator.EnsureArgumentNotNull(converter, "converter");
      this.innerEnumerator = innerEnumerator;
      this.converter = converter;
    }
  }
}