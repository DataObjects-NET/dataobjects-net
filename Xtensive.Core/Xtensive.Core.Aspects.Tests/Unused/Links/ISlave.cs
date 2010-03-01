// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using Xtensive.Core.Links;
namespace Xtensive.Core.Aspects.Tests.Links
{
  public interface ISlave : ILinkOwner
  {
    IMaster Master {get;}
  }
}
