using System;

namespace Xtensive.Aspects.Weaver.ExtensionBase
{
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
  public sealed class ValidatorStageAttribute : Attribute
  {
    public Type StageType { get; private set; }

    public ValidatorStageAttribute(Type validatorStageType)
    {
      StageType = validatorStageType;
    }
  }
}