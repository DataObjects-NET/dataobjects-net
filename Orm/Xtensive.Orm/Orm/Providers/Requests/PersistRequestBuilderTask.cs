// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.29

using System;
using System.Collections;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A task for <see cref="PersistRequestBuilder"/>.
  /// </summary>
  public sealed class PersistRequestBuilderTask
  {
    private readonly int hashCode;

    /// <summary>
    /// Gets the type.
    /// </summary>
    public TypeInfo Type { get; private set; }

    /// <summary>
    /// Gets the field map that describes updated fields.
    /// </summary>
    public BitArray ChangedFields { get; private set; }

    /// <summary>
    /// Gets the field map that describes available (fetched) fields.
    /// </summary>
    public BitArray AvailableFields { get; private set; }

    /// <summary>
    /// Gets the <see cref="PersistRequestKind"/>.
    /// </summary>
    public PersistRequestKind Kind { get; private set; }

    /// <summary>
    /// Gets flag indicating if validation should be performed.
    /// </summary>
    public bool ValidateVersion { get; private set; }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var other = obj as PersistRequestBuilderTask;
      if (other==null)
        return false;
      if (Type!=other.Type)
        return false;
      if (Kind!=other.Kind)
        return false;
      if (ValidateVersion!=other.ValidateVersion)
        return false;
      return CompareBits(AvailableFields, other.AvailableFields)
        && CompareBits(ChangedFields, other.ChangedFields);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return hashCode;
    }

    private int CalculateHashCode()
    {
      return Type.GetHashCode() ^ Kind.GetHashCode() ^ ValidateVersion.GetHashCode()
        ^ HashBits(AvailableFields) ^ HashBits(ChangedFields);
    }

    private int HashBits(BitArray bits)
    {
      if (bits==null)
        return int.MaxValue / 2;
      var result = 0;
      var max = Math.Min(bits.Count, 32);
      for (var i = 0; i < max; i++)
        result = (result << 1) | (bits[i] ? 1 : 0);
      return result;
    }

    private bool CompareBits(BitArray left, BitArray right)
    {
      if (left==null && right==null)
        return true;
      if (left!=null && right!=null && left.Count==right.Count) {
        for (var i = 0; i < left.Count; i++)
          if (left[i]!=right[i])
            return false;
        return true;
      }
      return false;
    }

    // Constructors

    internal static PersistRequestBuilderTask Insert(TypeInfo type)
    {
      return new PersistRequestBuilderTask(PersistRequestKind.Insert, type, null, null, false);
    }

    internal static PersistRequestBuilderTask Update(TypeInfo type, BitArray changedFields)
    {
      return new PersistRequestBuilderTask(PersistRequestKind.Update, type, null, changedFields, false);
    }

    internal static PersistRequestBuilderTask UpdateWithVersionCheck(TypeInfo type, BitArray availableFields, BitArray changedField)
    {
      return new PersistRequestBuilderTask(PersistRequestKind.Update, type, availableFields, changedField, true);
    }

    internal static PersistRequestBuilderTask Remove(TypeInfo type)
    {
      return new PersistRequestBuilderTask(PersistRequestKind.Remove, type, null, null, false);
    }

    internal static PersistRequestBuilderTask RemoveWithVersionCheck(TypeInfo type, BitArray availableFields)
    {
      return new PersistRequestBuilderTask(PersistRequestKind.Remove, type, availableFields, null, true);
    }

    // Constructors

    private PersistRequestBuilderTask(PersistRequestKind kind, TypeInfo type, BitArray availableFields, BitArray changedFields, bool validateVersion)
    {
      if (validateVersion && kind==PersistRequestKind.Insert)
        throw new ArgumentException(Strings.ExValidateVersionEqTrueIsIncompatibleWithPersistRequestKindEqInsert);

      Kind = kind;
      Type = type;
      ChangedFields = changedFields;
      AvailableFields = availableFields;
      ValidateVersion = validateVersion;

      hashCode = CalculateHashCode();
    }
  }
}