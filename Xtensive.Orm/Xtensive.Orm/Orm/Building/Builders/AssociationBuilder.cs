// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.03

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm.Building.Builders
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
      field.Associations.Add(association);

      if (!fieldDef.PairTo.IsNullOrEmpty())
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, fieldDef.PairTo));
    }

    public static void BuildAssociation(AssociationInfo origin, FieldInfo field)
    {
      var context = BuildingContext.Demand();
      var association = new AssociationInfo(field, origin.TargetType, origin.Multiplicity, origin.OnOwnerRemove, origin.OnTargetRemove);
      association.Name = context.NameBuilder.BuildAssociationName(association);
      context.Model.Associations.Add(association);
      var associationsToRemove = field.Associations
        .Where(a => a.TargetType == association.TargetType)
        .ToList();
      foreach (var toRemove in associationsToRemove)
        field.Associations.Remove(toRemove);
      field.Associations.Add(association);

      var pairTo = context.PairedAssociations.Where(p => p.First==origin).FirstOrDefault();
      if (pairTo.First!=null)
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, pairTo.Second));
    }

    public static void BuildReversedAssociation(AssociationInfo origin, string fieldName)
    {
      var context = BuildingContext.Demand();
      var owner = origin.TargetType;
      var field = owner.Fields[fieldName];
      var multiplicity = origin.Multiplicity;
      if (origin.Multiplicity == Multiplicity.OneToMany)
        multiplicity = Multiplicity.ManyToOne;
      else if (origin.Multiplicity == Multiplicity.ManyToOne)
        multiplicity = Multiplicity.OneToMany;
      else if (origin.Multiplicity == Multiplicity.ZeroToMany)
        multiplicity = field.IsEntity 
          ? Multiplicity.ZeroToOne
          : Multiplicity.ZeroToMany;
      else if (origin.Multiplicity == Multiplicity.ZeroToOne)
        multiplicity = field.IsEntity
          ? Multiplicity.ZeroToOne
          : Multiplicity.ZeroToMany;

      var association = new AssociationInfo(field, origin.OwnerType, multiplicity, origin.OnTargetRemove, origin.OnOwnerRemove);
      association.Name = context.NameBuilder.BuildAssociationName(association);
      AssociationInfo existing;
      if (!context.Model.Associations.TryGetValue(association.Name, out existing)) {
        context.Model.Associations.Add(association);
        association.Ancestors.AddRange(field.Associations);

        var associationsToRemove = field.Associations
          .Where(a => a.TargetType == association.TargetType)
          .ToList();
        foreach (var toRemove in associationsToRemove)
          field.Associations.Remove(toRemove);

        field.Associations.Add(association);
      }
    }

    public static void BuildPairedAssociation(AssociationInfo slave, string masterFieldName)
    {
      FieldInfo masterField;
      if (!slave.TargetType.Fields.TryGetValue(masterFieldName, out masterField))
        throw new DomainBuilderException(
          string.Format(Strings.ExPairedFieldXYWasNotFoundInZType, 
            slave.OwnerType.Name, masterFieldName, slave.TargetType.Name));

      if (masterField.IsPrimitive || masterField.IsStructure)
        throw new DomainBuilderException(
          string.Format(Strings.ExPairedFieldXHasWrongTypeItShouldBeReferenceToEntityOrAEntitySet, masterFieldName));

      var pairedField = slave.OwnerField;
      var master = masterField.GetAssociation(slave.OwnerType);
      var pairedFieldOwnerType = pairedField.DeclaringType.UnderlyingType;

      if (masterField.IsEntity)
        if ( master == null
          || master.TargetType != slave.OwnerType 
          || master.OwnerType != slave.TargetType
          || !masterField.ValueType.IsAssignableFrom(pairedFieldOwnerType))
//          || !pairedFieldOwnerType.IsAssignableFrom(masterField.ValueType))
          throw new DomainBuilderException(string.Format(
            Strings.ExXYFieldPairedToZAFieldShouldBeBButCurrentIsC,
            masterField.ReflectedType.UnderlyingType.GetShortName(),
            masterField.Name,
            pairedField.ReflectedType.UnderlyingType.GetShortName(),
            pairedField.Name,
            pairedFieldOwnerType.GetShortName(),
            masterField.ValueType.GetShortName()));

      if (masterField.IsEntitySet)
        if (master == null
          || master.TargetType != slave.OwnerType 
          || master.OwnerType != slave.TargetType 
          || !masterField.ItemType.IsAssignableFrom(pairedFieldOwnerType))
//          || !pairedFieldOwnerType.IsAssignableFrom(masterField.ItemType))
          throw new DomainBuilderException(string.Format(
            Strings.ExXYFieldPairedToZAFieldShouldBeEntitySetOfBButCurrentIsC,
            masterField.ReflectedType.UnderlyingType.GetShortName(),
            masterField.Name,
            pairedField.ReflectedType.UnderlyingType.GetShortName(),
            pairedField.Name,
            pairedFieldOwnerType.GetShortName(),
            masterField.ItemType.GetShortName()));

      if (master.Reversed!=null) {
        if (master.Reversed!=slave
          || (masterField.IsEntitySet && pairedField.IsEntitySet) // Unclear which side is virtual
          || (masterField.IsEntity    && pairedField.IsEntity))   // Unclear which side is virtual
          throw new InvalidOperationException(String.Format(
            Strings.ExFieldXYIsAlreadyPairedWithABRemoveCD,
            master.OwnerType.UnderlyingType.GetShortName(),
            master.OwnerField.Name,
            master.TargetType.UnderlyingType.GetShortName(),
            master.Reversed.OwnerField.Name,
            slave.OwnerType.UnderlyingType.GetShortName(), 
            slave.OwnerField.Name));
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