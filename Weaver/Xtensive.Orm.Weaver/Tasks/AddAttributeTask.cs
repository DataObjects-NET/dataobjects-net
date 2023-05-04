// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class AddAttributeTask : WeavingTask
  {
    private readonly ICustomAttributeProvider target;
    private readonly MethodReference attributeConstructor;
    private readonly object[] parameters;

    public override ActionResult Execute(ProcessorContext context)
    {
      var attribute = new CustomAttribute(attributeConstructor);
      for (var i = 0; i < attributeConstructor.Parameters.Count; i++) {
        var type = attributeConstructor.Parameters[i].ParameterType;
        var value = parameters[i];
        attribute.ConstructorArguments.Add(new CustomAttributeArgument(type, value));
      }
      target.CustomAttributes.Add(attribute);
      return ActionResult.Success;
    }

    public AddAttributeTask(ICustomAttributeProvider target, MethodReference attributeConstructor, params object[] parameters)
    {
      ArgumentNullException.ThrowIfNull(target);
      ArgumentNullException.ThrowIfNull(attributeConstructor);
      if (attributeConstructor.Parameters.Count!=parameters.Length)
        throw new ArgumentException("Invalid number of constructor parameters");

      this.target = target;
      this.attributeConstructor = attributeConstructor;
      this.parameters = parameters;
    }
  }
}