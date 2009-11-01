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
      if (!pairedAssociation.ReferencedType.Fields.TryGetValue(masterFieldName, out masterField)) {
        Log.Error(string.Format("Paired field '{0}' was not found in '{1}' type.", masterFieldName, pairedAssociation.ReferencedType.Name));
        return;
      }
      if (masterField.IsPrimitive || masterField.IsStructure) {
        Log.Error(string.Format("Paired field '{0}' has insufficient type. It should be reference to Entity or a EntitySet.", masterFieldName));
        return;
      }
      if (pairedAssociation.ReferencingField == masterField) {
        Log.Error("Referenced field and paired field are equal.");
        return;
      }

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