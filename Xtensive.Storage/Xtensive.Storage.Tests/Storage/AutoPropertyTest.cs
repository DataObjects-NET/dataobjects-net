// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.05.20

using System;
using System.Diagnostics;
using Xtensive.Core.Aspects;
using Xtensive.Integrity.Aspects.Constraints;
using Xtensive.Integrity.Validation;

namespace Xtensive.Storage.Tests.Storage
{
  public class AutoPropertyTest : IValidationAware
  {
    [ReplaceAutoProperty("Value")]
    public int Id { get; set; }
    [ReplaceAutoProperty("Value")]
    [Transactional]
    public string Title { get; set; }
    [Transactional]
    [NotNullOrEmptyConstraint]
    public string Description { get; set; }

    public T GetValue<T>(string name)
    {
      return default(T);
    }

    public void SetValue<T>(string name, T value)
    {
      
    }

    public ValidationContextBase Context
    {
      get { throw new NotImplementedException(); }
    }

    public void OnValidate()
    {
    }
  }
}