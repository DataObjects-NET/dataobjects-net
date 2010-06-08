// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.03

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class AssociationBuilder
  {
    public static void BuildAssociation(FieldDef fieldDef, FieldInfo field)
    {
      var context = BuildingContext.Demand();
      var referencedType = field.IsEntity ? context.Model.Types[field.ValueType] : context.Model.Types[field.ItemType];
      var multiplicity = field.IsEntitySet ? Multiplicity.ZeroToMany : Multiplicity.ZeroToOne;
      var association = new AssociationInfo(field, referencedType, multiplicity, fieldDef.OnOwnerRemove, fieldDef.OnTargetRemove);
      association.Name = context.NameBuilder.BuildAssociationName(association);
      context.Model.Associations.Add(association);
      field.Association = association;

      if (!fieldDef.PairTo.IsNullOrEmpty())
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, fieldDef.PairTo));
    }

    public static void BuildAssociation(AssociationInfo origin, FieldInfo field)
    {
      var context = BuildingContext.Demand();
      var association = new AssociationInfo(field, origin.TargetType, origin.Multiplicity, origin.OnOwnerRemove, origin.OnTargetRemove);
      association.Name = context.NameBuilder.BuildAssociationName(association);
      context.Model.Associations.Add(association);
      field.Association = association;

      var pairTo = context.PairedAssociations.Where(p => p.First==origin).FirstOrDefault();
      if (pairTo.First!=null)
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, pairTo.Second));
    }

    public static void BuildPairedAssociation(AssociationInfo slave, string masterFieldName)
    {
      FieldInfo masterField;
      if (!slave.TargetType.Fields.TryGetValue(masterFieldName, out masterField))
        throw new DomainBuilderException(
          string.Format(Strings.ExPairedFieldXWasNotFoundInYType, masterFieldName, slave.TargetType.Name));

      if (masterField.IsPrimitive || masterField.IsStructure)
        throw new DomainBuilderException(
          string.Format(Strings.ExPairedFieldXHasWrongTypeItShouldBeReferenceToEntityOrAEntitySet, masterFieldName));

      var pairedField = slave.OwnerField;
      var master = masterField.Association;
      var pairedFieldOwner = pairedField.DeclaringType.UnderlyingType;

      if (masterField.IsEntity && !pairedFieldOwner.IsAssignableFrom(masterField.ValueType))
        throw new DomainBuilderException(string.Format(
          Strings.ExPairedFieldForFieldXYShouldBeAssignableToTypeZ,
          pairedField.DeclaringType.UnderlyingType.GetShortName(),
          pairedField.Name, pairedFieldOwner.GetShortName()));
      if (masterField.IsEntitySet && !pairedFieldOwner.IsAssignableFrom(masterField.ItemType))
        throw new DomainBuilderException(string.Format(
          Strings.PairedFieldForFieldXYShouldBeEntitySetOfTypeAssignableToZ,
          pairedField.DeclaringType.UnderlyingType.GetShortName(),
          pairedField.Name, pairedFieldOwner.GetShortName()));

      if (master.Reversed!=null) {
        if (master.Reversed!=slave
          || (masterField.IsEntitySet && pairedField.IsEntitySet) // Unclear which side is virtual
          || (masterField.IsEntity    && pairedField.IsEntity))   // Unclear which side is virtual
          throw new InvalidOperationException(String.Format(
            Strings.ExMasterAssociationIsAlreadyPaired, master.Name, master.Reversed.Name));
      }

      slave.IsMaster = false;
      master.IsMaster = true;

      master.Reversed = slave;
      slave.Reversed = master;

      if (masterField.IsEntity) {
        if (pairedField.IsEntity) {
          master.Multiplicity = Multiplicity.OneToOne;
          slave.Multiplicity = Multiplicity.OneToOne;
        }
        if (pairedField.IsEntitySet) {
          master.Multiplicity = Multiplicity.ManyToOne;
          slave.Multiplicity = Multiplicity.OneToMany;
        }
      }

      if (masterField.IsEntitySet) {
        if (pairedField.IsEntity) {
          master.Multiplicity = Multiplicity.OneToMany;
          slave.Multiplicity = Multiplicity.ManyToOne;
        }
        if (pairedField.IsEntitySet) {
          master.Multiplicity = Multiplicity.ManyToMany;
          slave.Multiplicity = Multiplicity.ManyToMany;
        }
      }

      if (master.Multiplicity==Multiplicity.OneToMany) {
        master.IsMaster = false;
        slave.IsMaster = true;
      }

      // First pair of actions. They must always be equal
      if (!slave.OnTargetRemove.HasValue && !master.OnOwnerRemove.HasValue) {
        slave.OnTargetRemove = OnRemoveAction.Deny;
        master.OnOwnerRemove = OnRemoveAction.Deny;
      }
      if (!slave.OnTargetRemove.HasValue)
        slave.OnTargetRemove = master.OnOwnerRemove;
      if (!master.OnOwnerRemove.HasValue)
        master.OnOwnerRemove = slave.OnTargetRemove;
      if (master.OnOwnerRemove!=slave.OnTargetRemove)
        throw new DomainBuilderException(
          string.Format(Strings.ExOnOwnerRemoveActionIsNotEqualToOnTargetRemoveAction,
          master.OwnerType.Name, master.OwnerField.Name, slave.OwnerType.Name, slave.OwnerField.Name));

      // Second pair of actions. They also must be equal to each other
      if (!master.OnTargetRemove.HasValue && !slave.OnOwnerRemove.HasValue) {
        master.OnTargetRemove = OnRemoveAction.Deny;
        slave.OnOwnerRemove = OnRemoveAction.Deny;
      }
      if (!master.OnTargetRemove.HasValue)
        master.OnTargetRemove = slave.OnOwnerRemove;
      if (!slave.OnOwnerRemove.HasValue)
        slave.OnOwnerRemove = master.OnTargetRemove;
      if (slave.OnOwnerRemove != master.OnTargetRemove)
        throw new DomainBuilderException(
          string.Format(Strings.ExOnOwnerRemoveActionIsNotEqualToOnTargetRemoveAction,
          slave.OwnerType.Name, slave.OwnerField.Name, master.OwnerType.Name, master.OwnerField.Name));
    }
  }
}