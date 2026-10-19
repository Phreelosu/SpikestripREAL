using RoR2.ExpansionManagement;
using System;

namespace Spikestrip2_0.Core
{
  public abstract class ContentBase<T> : ContentBase where T : ContentBase<T>
  {
    public static T instance { get; private set; }

    public ContentBase()
    {
      if (instance != null)
      {
        throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" was instantiated twice");
      }
      instance = this as T;
    }
  }

  public abstract class ContentBase
  {
    public abstract string ConfigName { get; }

    public abstract string ConfigSection { get; }

    public virtual Type DependsOn => null;

    public virtual ExpansionDef RequiredExpansionHolder => SpikestripContent.ExpansionDef;

    public abstract void Init();
  }
}
