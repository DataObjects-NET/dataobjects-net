// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class TypeAdvancedConverter :
    StrictAdvancedConverterBase<Type>,
    IAdvancedConverter<Type, string>,
    IAdvancedConverter<string, Type>
  {
    string IAdvancedConverter<Type, string>.Convert(Type value)
    {
      return value.AssemblyQualifiedName;
    }

    Type IAdvancedConverter<string, Type>.Convert(string value)
    {
      return Type.GetType(value, true, false);
    }


    // Constructors

    public TypeAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}