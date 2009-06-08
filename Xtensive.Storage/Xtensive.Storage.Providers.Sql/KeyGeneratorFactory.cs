// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.07

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// Generator factory.
  /// </summary>
  public abstract class KeyGeneratorFactory: Providers.KeyGeneratorFactory
  {
    /// <inheritdoc/>
    public override bool IsSchemaBoundGenerator(GeneratorInfo generatorInfo)
    {
      if (generatorInfo.KeyGeneratorType!=typeof (KeyGenerator))
        return false;
      
      var generatorTypeCode = Type.GetTypeCode(generatorInfo.TupleDescriptor[0]);
      return generatorTypeCode==TypeCode.SByte
        || generatorTypeCode==TypeCode.Byte
        || generatorTypeCode==TypeCode.Int16
        || generatorTypeCode==TypeCode.UInt16
        || generatorTypeCode==TypeCode.Int32
        || generatorTypeCode==TypeCode.UInt32
        || generatorTypeCode==TypeCode.Int64
        || generatorTypeCode==TypeCode.UInt64;
    }
  }
}