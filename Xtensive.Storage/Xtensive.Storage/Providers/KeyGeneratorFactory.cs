// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using System;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Generator factory.
  /// </summary>
  public abstract class KeyGeneratorFactory : HandlerBase
  {
    /// <summary>
    /// Creates the generator according to the specified <paramref name="generatorInfo"/>.
    /// </summary>
    /// <param name="generatorInfo">The <see cref="generatorInfo"/> instance that describes generator.</param>
    /// <returns><see cref="KeyGenerator"/> instance.</returns>
    /// <exception cref="InvalidOperationException">when <paramref name="generatorInfo"/> contains more then one key field.</exception>
    /// <exception cref="ArgumentOutOfRangeException">when <see cref="Type"/> of the key field is not supported.</exception>
    public KeyGenerator CreateGenerator(GeneratorInfo generatorInfo)
    {
      KeyGenerator result = null;
      Type fieldType = generatorInfo.TupleDescriptor[0];
      TypeCode code = Type.GetTypeCode(fieldType);
      switch (code) {
      case TypeCode.SByte:
        result = CreateGenerator<SByte>(generatorInfo);
        break;
      case TypeCode.Byte:
        result = CreateGenerator<Byte>(generatorInfo);
        break;
      case TypeCode.Int16:
        result = CreateGenerator<Int16>(generatorInfo);
        break;
      case TypeCode.UInt16:
        result = CreateGenerator<UInt16>(generatorInfo);
        break;
      case TypeCode.Int32:
        result = CreateGenerator<Int32>(generatorInfo);
        break;
      case TypeCode.UInt32:
        result = CreateGenerator<UInt32>(generatorInfo);
        break;
      case TypeCode.Int64:
        result = CreateGenerator<Int64>(generatorInfo);
        break;
      case TypeCode.UInt64:
        result = CreateGenerator<UInt64>(generatorInfo);
        break;
      case TypeCode.Object:
        if (fieldType==typeof(Guid))
          result = new GuidKeyGenerator(generatorInfo);
        break;
      }
      if (result == null)
        throw new ArgumentOutOfRangeException(string.Format(Resources.Strings.ExTypeXIsNotSupported, fieldType.GetShortName()));
      result.Handlers = Handlers;
      return result;
    }
    
    /// <summary>
    /// Determines whether specific generator requires corresponding object in schema.
    /// </summary>
    /// <param name="generatorInfo">The generator info.</param>
    /// <returns>
    /// <see langword="true"/> if generator requires corresponding object in schema.
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool IsSchemaBoundGenerator(GeneratorInfo generatorInfo);

    /// <summary>
    /// Creates the generator.
    /// </summary>
    /// <typeparam name="TFieldType">The type of the field.</typeparam>
    /// <param name="generatorInfo">The generator info.</param>
    /// <returns>Newly created <see cref="KeyGenerator"/>.</returns>
    protected abstract KeyGenerator CreateGenerator<TFieldType>(GeneratorInfo generatorInfo);

  }
}