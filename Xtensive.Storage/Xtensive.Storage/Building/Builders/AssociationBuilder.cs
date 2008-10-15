// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.03

using System;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.PairIntegrity;
using Xtensive.Storage.Resources;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Building.Builders
{
  internal static class AssociationBuilder
  {
    public static void BuildAssociation(FieldDef fieldDef, FieldInfo field)
    {
      BuildingContext context = BuildingContext.Current;
      TypeInfo referencedType = context.Model.Types[field.ValueType];
      Multiplicity m = field.IsEntitySet ? Multiplicity.ManyToZero : Multiplicity.OneToZero;
      var association = new AssociationInfo(field, referencedType, m, fieldDef.OnRemove);
      association.Name = context.NameBuilder.Build(association);
      context.Model.Associations.Add(association);

      if (!fieldDef.PairTo.IsNullOrEmpty())
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, fieldDef.PairTo));
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

      if (slave.ReferencingField==masterField)
        throw new DomainBuilderException(
          string.Format(Strings.ReferencedFieldXAndPairedFieldAreEqual, slave.ReferencingField.Name));

      FieldInfo pairedField = slave.ReferencingField;
      AssociationInfo master = masterField.Association;
      if (master.Reversed!=null && master.Reversed!=slave)
        throw new InvalidOperationException(String.Format(Strings.ExMasterAssociationIsAlreadyPaired, master.Name, master.Reversed.Name));

      master.Reversed = slave;
      slave.Reversed = master;

      if (master.IsMaster && slave.IsMaster)
        master.IsMaster = false;

      if (masterField.IsEntity) {
        if (pairedField.IsEntity) {
          master.Multiplicity = Multiplicity.OneToOne;
          slave.Multiplicity = Multiplicity.OneToOne;
        }
        if (pairedField.IsEntitySet) {
          master.Multiplicity = Multiplicity.OneToMany;
          slave.Multiplicity = Multiplicity.ManyToOne;
        }
      }

      if (masterField.IsEntitySet) {
        if (pairedField.IsEntity) {
          master.Multiplicity = Multiplicity.ManyToOne;
          slave.Multiplicity = Multiplicity.OneToMany;
        }
        if (pairedField.IsEntitySet) {
          master.Multiplicity = Multiplicity.ManyToMany;
          slave.Multiplicity = Multiplicity.ManyToMany;
        }
      }

      BuildPairSyncActions(master);
      BuildPairSyncActions(slave);
    }

    private static void BuildPairSyncActions(AssociationInfo association)
    {
      Func<Entity, Entity> getValue = null;
      Action<Entity, Entity> @break;
      Action<Entity, Entity> create;

      switch (association.Multiplicity) {
      case Multiplicity.OneToOne:
        getValue = BuildGetPairedValueAction(association);
        @break = BuildBreakAssociationAction(association, OperationType.Set);
        create = BuildCreateAssociationAction(association, OperationType.Set);
        break;
      case Multiplicity.OneToMany:
        getValue = BuildGetPairedValueAction(association);
        @break = BuildBreakAssociationAction(association, OperationType.Set);
        create = BuildCreateAssociationAction(association, OperationType.Set);
        break;
      case Multiplicity.ManyToOne:
        @break = BuildBreakAssociationAction(association, OperationType.Remove);
        create = BuildCreateAssociationAction(association, OperationType.Add);
        break;
      case Multiplicity.ManyToMany:
        @break = BuildBreakAssociationAction(association, OperationType.Remove);
        create = BuildCreateAssociationAction(association, OperationType.Add);
        break;
      default:
        return;
      }
      PairIntegrity.ActionSet actionSet = new ActionSet(getValue, @break, create);
      BuildingContext.Current.Domain.PairSyncActions.Add(association, actionSet);
    }

    private static Func<Entity, Entity> BuildGetPairedValueAction(AssociationInfo association)
    {
//      if (association.ReferencingField.UnderlyingProperty!=null) {
//        Type dh = typeof (DelegateHelper);
//        MethodInfo mi = dh.GetMethod("CreateGetMemberDelegate");
//        MethodInfo cmi = mi.MakeGenericMethod(new[] {typeof(Entity), typeof (Entity)});
//        return (Func<Entity, Entity>) cmi.Invoke(null, new[] {association.ReferencingField.UnderlyingProperty.Name});
//      }
      return entity => entity.GetProperty<Entity>(association.ReferencingField.Name);
    }

    private static Action<Entity, Entity> BuildBreakAssociationAction(AssociationInfo association, OperationType type)
    {
      if (type == OperationType.Set)
        return (master, slave) => master.SetProperty<Entity>(association.ReferencingField.Name, null);
      else
        return (master, slave) => master.GetProperty<EntitySet>(association.ReferencingField.Name).Remove(slave);
    }

    private static Action<Entity, Entity> BuildCreateAssociationAction(AssociationInfo association, OperationType type)
    {
      if (type == OperationType.Set)
        return (master, slave) => master.SetProperty<Entity>(association.ReferencingField.Name, slave);
      else
        return (master, slave) => master.GetProperty<EntitySet>(association.ReferencingField.Name).Add(slave);
    }
  }
}