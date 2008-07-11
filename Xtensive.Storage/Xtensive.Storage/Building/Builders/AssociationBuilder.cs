// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.03

using Xtensive.Core;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Building.Builders
{
  internal static class AssociationBuilder
  {
    public static AssociationInfo BuildAssociation(FieldDef fieldDef, FieldInfo field)
    {
      BuildingContext context = BuildingScope.Context;
      TypeInfo referencedType = context.Model.Types[field.ValueType];
      Multiplicity m = field.IsEntitySet ? Multiplicity.ManyToZero : Multiplicity.OneToZero;
      AssociationInfo association = new AssociationInfo(field, referencedType, m, fieldDef.OnDelete);
      association.Name = context.NameProvider.BuildName(association);
      context.Model.Associations.Add(association);

      if (!string.IsNullOrEmpty(fieldDef.PairTo))
        context.PairedAssociations.Add(new Pair<AssociationInfo, string>(association, fieldDef.PairTo));

      return association;
    }

    public static void BuildPairedAssociation(AssociationInfo pairedAssociation, string masterFieldName)
    {
      FieldInfo masterField;
      if (!pairedAssociation.ReferencedType.Fields.TryGetValue(masterFieldName, out masterField))
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ExPairedFieldXWasNotFoundInYType, masterFieldName, pairedAssociation.ReferencedType.Name));

      if (masterField.IsPrimitive || masterField.IsStructure)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.PairedFieldXHasInsufficientTypeItShouldBeReferenceToEntityOrAEntitySet, masterFieldName));

      if (pairedAssociation.ReferencingField == masterField)
        throw new DomainBuilderException(
          string.Format(Resources.Strings.ReferencedFieldXAndPairedFieldAreEqual, pairedAssociation.ReferencingField.Name));

      FieldInfo pairedField = pairedAssociation.ReferencingField;
      AssociationInfo masterAssociation = masterField.Association;

      if (masterField.IsEntity) {
        if (pairedField.IsEntity) {
          masterAssociation.Multiplicity = Multiplicity.OneToOne;
          pairedAssociation.Multiplicity = Multiplicity.OneToOne;
          pairedAssociation.PairTo = masterAssociation;
        }
        if (pairedField.IsEntitySet) {
          masterAssociation.Multiplicity = Multiplicity.OneToMany;
          pairedAssociation.Multiplicity = Multiplicity.ManyToOne;
          pairedAssociation.PairTo = masterAssociation;
        }
      }

      if (masterField.IsEntitySet) {
        if (pairedField.IsEntity) {
          masterAssociation.Multiplicity = Multiplicity.ManyToOne;
          pairedAssociation.Multiplicity = Multiplicity.OneToMany;
          masterAssociation.PairTo = pairedAssociation;
        }
        if (pairedField.IsEntitySet) {
          masterAssociation.Multiplicity = Multiplicity.ManyToMany;
          pairedAssociation.Multiplicity = Multiplicity.ManyToMany;
          pairedAssociation.PairTo = masterAssociation;
        }
      }
    }
  }
}