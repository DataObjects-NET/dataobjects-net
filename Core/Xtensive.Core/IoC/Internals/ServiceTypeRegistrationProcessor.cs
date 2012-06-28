// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.IoC
{
  internal sealed class ServiceTypeRegistrationProcessor : TypeRegistrationProcessorBase
  {
    public override Type BaseType {
      get { return typeof (object); }
    }

    protected override bool IsAcceptable(TypeRegistry registry, TypeRegistration registration, Type type)
    {
      return true;
    }
  }
}