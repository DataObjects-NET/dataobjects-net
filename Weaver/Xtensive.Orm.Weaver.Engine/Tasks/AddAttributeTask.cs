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

    public override ActionResult Execute(ProcessorContext context)
    {
      target.CustomAttributes.Add(new CustomAttribute(attributeConstructor));
      return ActionResult.Success;
    }

    public AddAttributeTask(ICustomAttributeProvider target, MethodReference attributeConstructor)
    {
      if (target==null)
        throw new ArgumentNullException("target");
      if (attributeConstructor==null)
        throw new ArgumentNullException("attributeConstructor");

      this.target = target;
      this.attributeConstructor = attributeConstructor;
    }
  }
}