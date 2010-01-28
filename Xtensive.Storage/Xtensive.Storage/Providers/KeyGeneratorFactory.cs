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
    /// Creates the generator according to the specified <paramref name="keyProviderInfo>.
    /// </summary>
    /// <param name="keyProviderInfo">The <see cref="keyProviderInfo"/> instance that describes generator.</param>
    /// <returns><see cref="KeyGenerator"/> instance.</returns>
    /// <exception cref="InvalidOperationException">when <paramref name="keyProviderInfo> contains more then one key field.</exception>
    /// <exception cref="ArgumentOutOfRangeException">when <see cref="Type"/> of the key field is not supported.</exception>
    public KeyGenerator CreateGenerator(KeyProviderInfo keyProviderInfo)
    {
      KeyGenerator result = null;
      Type fieldType = keyProviderInfo.TupleDescriptor[0];
      TypeCode code = Type.GetTypeCode(fieldType);
      switch (code) {
      case TypeCode.SByte:
        result = CreateGenerator<SByte>(keyProviderInfo);
        break;
      case TypeCode.Byte:
        result = CreateGenerator<Byte>(keyProviderInfo);
        break;
      case TypeCode.Int16:
        result = CreateGenerator<Int16>(keyProviderInfo);
        break;
      case TypeCode.UInt16:
        result = CreateGenerator<UInt16>(keyProviderInfo);
        break;
      case TypeCode.Int32:
        result = CreateGenerator<Int32>(keyProviderInfo);
        break;
      case TypeCode.UInt32:
        result = CreateGenerator<UInt32>(keyProviderInfo);
        break;
      case TypeCode.Int64:
        result = CreateGenerator<Int64>(keyProviderInfo);
        break;
      case TypeCode.UInt64:
        result = CreateGenerator<UInt64>(keyProviderInfo);
        break;
      case TypeCode.Object:
        if (fieldType==typeof(Guid))
          result = new GuidKeyGenerator(keyProviderInfo);
        break;
      }
      if (result == null)
        throw new ArgumentOutOfRangeException(string.Format(Resources.Strings.ExTypeXIsNotSupported, fieldType.GetShortName()));
      return result;
    }
    
    /// <summary>
    /// Determines whether specific generator requires corresponding object in schema.
    /// </summary>
    /// <param name="keyProviderInfo">The generator info.</param>
    /// <returns>
    /// <see langword="true"/> if generator requires corresponding object in schema.
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool IsSchemaBoundGenerator(KeyProviderInfo keyProviderInfo);

    /// <summary>
    /// Creates the generator.
    /// </summary>
    /// <typeparam name="TFieldType">The type of the field.</typeparam>
    /// <param name="keyProviderInfo">The generator info.</param>
    /// <returns>Newly created <see cref="KeyGenerator"/>.</returns>
    protected abstract KeyGenerator CreateGenerator<TFieldType>(KeyProviderInfo keyProviderInfo);

  }
}