using System;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Generators;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  [HierarchyRoot(typeof (GuidGenerator), "Id")]
  public sealed class Carrier : Entity
  {
    [Field]
    public Guid Id { get; set; }

    [Field]
    public string Name { get; set; }

    /// <summary>
    /// Gets collection of <see cref="Company"/> assigned to carrier. 
    /// It's autoproperty. :) And it's many-to-many relation.
    /// </summary>
    [Field]
    public EntitySet<Company> Companies { get; set; }

    /// <summary>
    /// Carrier Address. Structure.
    /// Attention! Autoproperty.
    /// </summary>
    [Field]
    public CarrierAddress Address { get; set; }
  }
}