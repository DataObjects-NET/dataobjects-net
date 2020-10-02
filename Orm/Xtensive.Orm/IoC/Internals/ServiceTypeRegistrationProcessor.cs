// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.01.30

using System;
using Xtensive.Collections;
using Xtensive.Reflection;

namespace Xtensive.IoC
{
  internal sealed class ServiceTypeRegistrationProcessor : TypeRegistrationProcessorBase
  {
    public override Type BaseType => WellKnownTypes.Object;

    protected override bool IsAcceptable(TypeRegistration registration, Type type)
    {
      return true;
    }
  }
}
