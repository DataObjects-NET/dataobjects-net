// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.13

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.ObjectMapping;
using Xtensive.Testing;
using Xtensive.Tests.ObjectMapping.SourceModel;
using Xtensive.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MapperHandlingStructureTests : MapperTestBase
  {
    [Test]
    public void StructureTransformationTest()
    {
      var mapper = GetStructureContainerMapper();
      var source = GetSourceStructureContainer();
      var target = (StructureContainerDto) mapper.Transform(source);
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.AuxString, target.AuxString);
      Assert.AreEqual(source.Structure.String, target.Structure.String);
      Assert.AreEqual(source.Structure.Int, target.Structure.Int);
      Assert.AreEqual(source.Structure.DateTime, target.Structure.DateTime);
      Assert.AreEqual(source.CompositeStructure.AuxInt, target.CompositeStructure.AuxInt);
      Assert.AreEqual((int) source.CompositeStructure.Structure.AuxDouble,
        target.CompositeStructure.Structure.AuxInt);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.AuxInt,
        target.CompositeStructure.Structure.Structure.AuxInt);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.StructureContainer.Id,
        target.CompositeStructure.Structure.Structure.StructureContainer.Id);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.StructureContainer.Structure.DateTime,
        target.CompositeStructure.Structure.Structure.StructureContainer.Structure.DateTime);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.StructureContainer.Structure.Int,
        target.CompositeStructure.Structure.Structure.StructureContainer.Structure.Int);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.StructureContainer.Structure.String,
        target.CompositeStructure.Structure.Structure.StructureContainer.Structure.String);
    }

    [Test]
    public void StructureComparisonTest()
    {
      var mapper = GetStructureContainerMapper();
      var source = GetSourceStructureContainer();
      var original = (StructureContainerDto) mapper.Transform(source);
      var modified = Clone(original);
      var newStructureString = modified.Structure.String + "M";
      modified.Structure = new StructureDto {
        DateTime = modified.Structure.DateTime, Int = modified.Structure.Int,
        String = newStructureString
      };
      var newCompositeStructureStructureStructure = new CompositeStructure2Dto {
        AuxInt = 11, StructureContainer = modified.CompositeStructure.Structure.Structure.StructureContainer
      };
      newCompositeStructureStructureStructure.StructureContainer.AuxString += "A";
      var newCompositeStructureStructure = new CompositeStructure1Dto {
        AuxInt = modified.CompositeStructure.Structure.AuxInt,
        Structure = newCompositeStructureStructureStructure
      };
      var newCompositeStructure = new CompositeStructure0Dto {
        AuxInt = modified.CompositeStructure.AuxInt, Structure = newCompositeStructureStructure
      };
      modified.CompositeStructure = newCompositeStructure;
      var operations = ((DefaultOperationLog) mapper.Compare(original, modified).Operations).ToList();
      Assert.AreEqual(3, operations.Count);
      ValidatePropertyOperation<StructureContainerDto>(original, operations[0], sc => sc.Structure.String,
        newStructureString, OperationType.SetProperty);
      ValidatePropertyOperation<StructureContainerDto>(original, operations[1],
        sc => sc.CompositeStructure.Structure.Structure.AuxInt, newCompositeStructureStructureStructure.AuxInt,
        OperationType.SetProperty);
      ValidatePropertyOperation<StructureContainerDto>(
        original.CompositeStructure.Structure.Structure.StructureContainer, operations[2], sc => sc.AuxString,
        modified.CompositeStructure.Structure.Structure.StructureContainer.AuxString,
        OperationType.SetProperty);
    }

    [Test]
    public void ModifyReferencePropertyOfStructureTest()
    {
      var mapper = GetStructureContainerMapper();
      var source = GetSourceStructureContainer();
      var original = (StructureContainerDto) mapper.Transform(source);
      var modified = Clone(original);
      var newStructureContainer = new StructureContainerDto {AuxString = "NEW",
        CompositeStructure = new CompositeStructure0Dto {AuxInt = 999}};
      var newCs2 = new CompositeStructure2Dto {
        AuxInt = modified.CompositeStructure.Structure.Structure.AuxInt,
        StructureContainer = newStructureContainer
      };
      var newCs1 = new CompositeStructure1Dto
      {
        AuxInt = modified.CompositeStructure.Structure.AuxInt,
        Structure = newCs2
      };
      modified.CompositeStructure = new CompositeStructure0Dto {
        AuxInt = modified.CompositeStructure.AuxInt,
        Structure = newCs1
      };
      var operations = ((DefaultOperationLog) mapper.Compare(original, modified).Operations).ToList();
      Assert.AreEqual(8, operations.Count);
      ValidateObjectCreation(newStructureContainer, operations[0]);
      ValidatePropertyOperation<StructureContainerDto>(
        newStructureContainer,
        operations[1], sc => sc.AuxString, newStructureContainer.AuxString, OperationType.SetProperty);
      ValidatePropertyOperation<StructureContainerDto>(
        newStructureContainer,
        operations[2], sc => sc.Structure.Int, default (int), OperationType.SetProperty);
      ValidatePropertyOperation<StructureContainerDto>(
        newStructureContainer,
        operations[3], sc => sc.Structure.String, default (string), OperationType.SetProperty);
      ValidatePropertyOperation<StructureContainerDto>(
        newStructureContainer,
        operations[4], sc => sc.Structure.DateTime, default (DateTime), OperationType.SetProperty);
      ValidatePropertyOperation<StructureContainerDto>(
        newStructureContainer,
        operations[5], sc => sc.CompositeStructure.AuxInt, newStructureContainer.CompositeStructure.AuxInt,
        OperationType.SetProperty);
      ValidatePropertyOperation<StructureContainerDto>(
        original,
        operations[6], sc => sc.CompositeStructure.Structure.Structure.StructureContainer,
        newStructureContainer, OperationType.SetProperty);
      ValidateObjectRemoval(original.CompositeStructure.Structure.Structure.StructureContainer, operations[7]);
    }

    [Test]
    public void LimitDepthOfGraphContainingStructureTest()
    {
      var mapper = GetStructureContainerMapper(2, GraphTruncationType.Throw);
      var source = GetSourceStructureContainer();
      AssertEx.ThrowsInvalidOperationException(() => mapper.Transform(source));
      mapper = GetStructureContainerMapper(5, GraphTruncationType.Throw);
      mapper.Transform(source);
      mapper = GetStructureContainerMapper(2, GraphTruncationType.SetDefaultValue);
      var target = (StructureContainerDto) mapper.Transform(source);
      Assert.AreEqual(source.Structure.DateTime, target.Structure.DateTime);
      Assert.AreEqual(default (CompositeStructure2Dto), target.CompositeStructure.Structure.Structure);
      mapper = GetStructureContainerMapper(1, GraphTruncationType.SetDefaultValue);
      target = (StructureContainerDto) mapper.Transform(source);
      Assert.AreEqual(source.Structure.DateTime, target.Structure.DateTime);
      Assert.AreEqual(default (CompositeStructure1Dto), target.CompositeStructure.Structure);
      mapper = GetStructureContainerMapper(3, GraphTruncationType.SetDefaultValue);
      target = (StructureContainerDto) mapper.Transform(source);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.AuxInt,
        target.CompositeStructure.Structure.Structure.AuxInt);
      Assert.IsNull(target.CompositeStructure.Structure.Structure.StructureContainer);
    }

    [Test]
    public void TransformationOfStructureFieldHavingConverterTest()
    {
      var mapper = GetStructureContainerMapperWithStructureConverter();
      var source = GetSourceStructureContainer();
      var target = (StructureContainerDto) mapper.Transform(source);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.AuxInt,
        target.CompositeStructure.Structure.Structure.AuxInt);
      Assert.IsNull(target.CompositeStructure.Structure.Structure.StructureContainer);
      Assert.AreEqual(source.CompositeStructure.Structure.Structure.AuxInt,
        target.CompositeStructure.Structure.Structure.AuxInt);
    }

    [Test]
    public void LimitDepthOfGraphContainingStructureWithConverterTest()
    {
      var mapper = GetStructureContainerMapperWithStructureConverter(2, GraphTruncationType.Throw);
      var source = GetSourceStructureContainer();
      AssertEx.ThrowsInvalidOperationException(() => mapper.Transform(source));
      mapper = GetStructureContainerMapperWithStructureConverter(2, GraphTruncationType.SetDefaultValue);
      var target = (StructureContainerDto) mapper.Transform(source);
      Assert.AreEqual(default (CompositeStructure2Dto), target.CompositeStructure.Structure.Structure);
    }

    private static DefaultMapper GetStructureContainerMapperWithStructureConverter(int? graphDepthLimit,
      GraphTruncationType truncationType)
    {
      var settings = new MapperSettings {
        GraphDepthLimit = graphDepthLimit, GraphTruncationType = truncationType
      };
      var mapping = new MappingBuilder()
        .MapStructure<Structure, StructureDto>()
        .MapType<StructureContainer, StructureContainerDto, Guid>(s => s.Id, s => s.Id)
        .MapStructure<CompositeStructure0, CompositeStructure0Dto>()
        .MapStructure<CompositeStructure1, CompositeStructure1Dto>()
          .MapProperty(c => (int) c.AuxDouble, c => c.AuxInt)
          .MapProperty(c => new CompositeStructure2Dto {AuxInt = c.Structure.AuxInt}, c => c.Structure)
        .MapStructure<CompositeStructure2, CompositeStructure2Dto>().Build();
      return new DefaultMapper(mapping, settings);
    }

    private static DefaultMapper GetStructureContainerMapperWithStructureConverter()
    {
      return GetStructureContainerMapperWithStructureConverter(null, GraphTruncationType.Default);
    }
  }
}