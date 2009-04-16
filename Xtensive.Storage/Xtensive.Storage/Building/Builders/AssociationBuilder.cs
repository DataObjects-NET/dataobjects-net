// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.03

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class AssociationBuilder
  {
    public static void BuildAssociation(FieldDef fieldDef, FieldInfo field)
    {
      BuildingContext context = BuildingContext.Current;
      TypeInfo referencedType = field.IsEntity ? context.Model.Types[field.ValueType] : context.Model.Types[field.ItemType];
      Multiplicity multiplicity = field.IsEntitySet ? Multiplicity.ZeroToMany : Multiplicity.ZeroToOne;
      var association = new AssociationInfo(field, referencedType, multiplicity, fieldDef.OnRemove);
      association.Name = context.NameBuilder.Build(association);
      context.Model.Associations.Add(association);

      if (!fieldDef.PairTo.IsNullOrEmpty()) {
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, fieldDef.PairTo));
      }
    }

    public static void BuildAssociation(AssociationInfo origin, FieldInfo field)
    {
      BuildingContext context = BuildingContext.Current;
      var association = new AssociationInfo(field, origin.ReferencedType, origin.Multiplicity, origin.OnRemove);
      association.Name = context.NameBuilder.Build(association);
      context.Model.Associations.Add(association);

      Pair<AssociationInfo, string> pairTo = context.PairedAssociations.Where(p => p.First==origin).FirstOrDefault();
      if (pairTo.First!=null)
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, pairTo.Second));
    }

    public static void BuildPairedAssociation(AssociationInfo slave, string masterFieldName)
    {
      FieldInfo masterField;
      if (!slave.ReferencedType.Fields.TryGetValue(masterFieldName, out masterField))
        throw new DomainBuilderException(
          string.Format(Strings.ExPairedFieldXWasNotFoundInYType, masterFieldName, slave.ReferencedType.Name));

      if (masterField.IsPrimitive || masterField.IsStructure)
        throw new DomainBuilderException(
          string.Format(Strings.PairedFieldXHasInsufficientTypeItShouldBeReferenceToEntityOrAEntitySet, masterFieldName));


      FieldInfo pairedField = slave.ReferencingField;
      AssociationInfo master = masterField.Association;
      if (master.Reversed!=null && master.Reversed!=slave)
        throw new InvalidOperationException(String.Format(Strings.ExMasterAssociationIsAlreadyPaired, master.Name, master.Reversed.Name));

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

      BuildPairSyncActions(master);
      if (!master.IsLoop)
        BuildPairSyncActions(slave);
    }

    private static void BuildPairSyncActions(AssociationInfo association)
    {
      if (BuildingContext.Current.Domain.PairSyncActions.ContainsKey(association))
        throw new DomainBuilderException(string.Format(Strings.ExPairToAttributeCanNotBeAppliedToXField,
          association.ReferencingField, association.ReferencingType.UnderlyingType.FullName, association.Reversed.ReferencingField, association.Reversed.ReferencingType.UnderlyingType.FullName));

      Func<IEntity, bool, IEntity> getValue = null;
      Action<IEntity, IEntity, bool> @break;
      Action<IEntity, IEntity, bool> create;

      switch (association.Multiplicity) {
      case Multiplicity.OneToOne:
        getValue = BuildGetPairedValueAction(association);
        @break = BuildBreakAssociationAction(association, OperationType.Set);
        create = BuildCreateAssociationAction(association, OperationType.Set);
        break;
      case Multiplicity.OneToMany:
        @break = BuildBreakAssociationAction(association, OperationType.Remove);
        create = BuildCreateAssociationAction(association, OperationType.Add);
        break;
      case Multiplicity.ManyToOne:
        getValue = BuildGetPairedValueAction(association);
        @break = BuildBreakAssociationAction(association, OperationType.Set);
        create = BuildCreateAssociationAction(association, OperationType.Set);
        break;
      case Multiplicity.ManyToMany:
        @break = BuildBreakAssociationAction(association, OperationType.Remove);
        create = BuildCreateAssociationAction(association, OperationType.Add);
        break;
      default:
        return;
      }
      var actionSet = new ActionSet(getValue, @break, create);

      BuildingContext.Current.Domain.PairSyncActions.Add(association, actionSet);
    }

    private static Func<IEntity, bool, IEntity> BuildGetPairedValueAction(AssociationInfo association)
    {
      return (entity, notify) => ((Entity)entity).GetFieldValue<IEntity>(association.ReferencingField, notify);
    }

    private static Action<IEntity, IEntity, bool> BuildBreakAssociationAction(AssociationInfo association, OperationType type)
    {
      if (type==OperationType.Set)
        return (master, slave, notify) => ((Entity)master).SetFieldValue<IEntity>(association.ReferencingField, null, notify);
      else
        return (master, slave, notify) => ((Entity)master).GetFieldValue<EntitySetBase>(association.ReferencingField, notify).Remove((Entity)slave, notify);
    }

    private static Action<IEntity, IEntity, bool> BuildCreateAssociationAction(AssociationInfo association, OperationType type)
    {
      if (type==OperationType.Set)
        return (master, slave, notify) => ((Entity)master).SetFieldValue(association.ReferencingField, slave, notify);
      else
        return (master, slave, notify) => ((Entity)master).GetFieldValue<EntitySetBase>(association.ReferencingField, notify).Add((Entity)slave, notify);
    }
  }
}