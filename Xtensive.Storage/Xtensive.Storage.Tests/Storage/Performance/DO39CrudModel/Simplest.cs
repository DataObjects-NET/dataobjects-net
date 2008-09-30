// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.24

using DataObjects.NET;

namespace Xtensive.Storage.Tests.Storage.Performance.DO39CrudModel
{
  public abstract class Simplest : DataObject
  {
    public abstract long Value { get; set; }

    protected virtual void OnCreate(long value)
    {
      base.OnCreate();
      Value = value;
    }
  }
}