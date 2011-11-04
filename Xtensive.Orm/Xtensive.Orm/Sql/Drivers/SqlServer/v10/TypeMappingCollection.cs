// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.03

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.SqlServer.v10
{
  /// <summary>
  /// A collection of <see cref="TypeMapping"/> objects.
  /// </summary>
  public class TypeMappingCollection : Sql.TypeMappingCollection
  {
    public TypeMapping Geometry { get; private set; }
    public TypeMapping Geography { get; private set; }

    public override TypeMapping TryGetMapping(Type type)
    {
      if (type.Name == "SqlGeometry")
        return Geometry;
      if (type.Name == "SqlGeography")
        return Geography;
      return base.TryGetMapping(type);
    }

    public override IEnumerator<TypeMapping> GetEnumerator()
    {
      yield return Boolean;
      yield return Char;
      yield return String;
      yield return Byte;
      yield return SByte;
      yield return Short;
      yield return UShort;
      yield return Int;
      yield return UInt;
      yield return Long;
      yield return ULong;
      yield return Float;
      yield return Double;
      yield return Decimal;
      yield return DateTime;
      yield return TimeSpan;
      yield return Guid;
      yield return ByteArray;
      yield return Geometry;
      yield return Geography;
    }
    
    // Constructors

    public TypeMappingCollection(TypeMapper h)
      : base(h)
    {
      var g = Type.GetType("Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
      if (g != null) 
        Geometry = BuildMapping(h, g, h.ReadGeometry, h.SetGeometryParameterValue, h.BuildGeometrySqlType);
      g = Type.GetType("Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
      if (g != null) 
        Geography = BuildMapping(h, g, h.ReadGeography, h.SetGeographyParameterValue, h.BuildGeographySqlType);
    }
  }
}