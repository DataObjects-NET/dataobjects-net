// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using Xtensive.Storage.Validation;

namespace Xtensive.Integrity.Tests
{
  public class ValidationContext: ValidationContextBase
  {
    public new void Reset()
    {
      base.Reset();
    }


    // Constructors

    public ValidationContext() 
    {}
  }
}