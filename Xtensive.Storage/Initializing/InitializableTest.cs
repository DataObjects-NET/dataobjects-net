// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.01

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Tuples;

//[assembly:Initializable(AttributeTargetTypes = "*")]

namespace Xtensive.Storage.Initializing.Initializable1
{
  [Serializable]
  public abstract class Provider :
    IEnumerable<Tuple>,
    IInitializable,
    IHasServices
  {
    private int value;

    #region Interfaces

    public IEnumerator<Tuple> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    public override string ToString()
    {
      return value.ToString();
    }

    public T GetService<T>() where T : class
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    protected void Initialize(Type ctorType)
    {
      if (ctorType==GetType())
        Initialize();
    }

    protected abstract void Initialize();

    protected Provider(int value)
    {
      this.value = value;
    }
  }
  
  public abstract class CompilableProvider : Provider
  {
    protected CompilableProvider(int value) 
      : base(value)
    {      
    }
  }
}
