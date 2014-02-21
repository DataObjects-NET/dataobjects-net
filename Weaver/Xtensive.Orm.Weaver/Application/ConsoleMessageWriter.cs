// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;

namespace Xtensive.Orm.Weaver.Application
{
  internal sealed class ConsoleMessageWriter : MessageWriter
  {
    public override void Write(ProcessorMessage message)
    {
      Console.WriteLine(message);
    }
  }
}