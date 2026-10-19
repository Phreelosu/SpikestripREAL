using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Spikestrip2_0.Core
{
  public static class BuffBehaviours
  {
    private struct BuffBehaviour
    {
      public BuffDef buffDef;

      public Type behaviour;

      public bool serverOnly;
    }

    private class SyncBuffBehaviour : INetMessage
    {
      private int index;

      private int count;

      private NetworkInstanceId bodyId;

      public SyncBuffBehaviour()
      {
      }

      public SyncBuffBehaviour(int index, int count, NetworkInstanceId bodyId)
      {
        this.index = index;
        this.count = count;
        this.bodyId = bodyId;
      }

      public void Serialize(NetworkWriter writer)
      {
        writer.Write(index);
        writer.Write(count);
        writer.Write(bodyId);
      }

      public void Deserialize(NetworkReader reader)
      {
        index = reader.ReadInt32();
        count = reader.ReadInt32();
        bodyId = reader.ReadNetworkId();
      }

      public void OnReceived()
      {
        if (NetworkServer.active)
        {
          return;
        }
        GameObject bodyObject = Util.FindNetworkObject(bodyId);
        if (bodyObject && bodyObject.TryGetComponent(out CharacterBody body))
        {
          UpdateBehaviour(body, behaviours[index], count);
        }
      }
    }

    private static readonly List<BuffBehaviour> behaviours = new List<BuffBehaviour>();

    internal static void Init()
    {
      NetworkingAPI.RegisterMessageType<SyncBuffBehaviour>();
    }

    public static void Register(BuffDef buffDef, Type behaviour, bool serverOnly = true)
    {
      if (behaviours.Count == 0)
      {
        On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
      }
      behaviours.Add(new BuffBehaviour { buffDef = buffDef, behaviour = behaviour, serverOnly = serverOnly });
    }

    private static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
    {
      orig(self, buffType, newCount);
      if (!NetworkServer.active || !self)
      {
        return;
      }
      for (int i = 0; i < behaviours.Count; i++)
      {
        if (behaviours[i].buffDef.buffIndex != buffType)
        {
          continue;
        }
        UpdateBehaviour(self, behaviours[i], newCount);
        if (!behaviours[i].serverOnly && self.TryGetComponent(out NetworkIdentity identity))
        {
          new SyncBuffBehaviour(i, newCount, identity.netId).Send(NetworkDestination.Clients);
        }
      }
    }

    private static void UpdateBehaviour(CharacterBody body, BuffBehaviour buffBehaviour, int stack)
    {
      CharacterBody.ItemBehavior behaviour = (CharacterBody.ItemBehavior)body.GetComponent(buffBehaviour.behaviour);
      if (stack > 0)
      {
        if (!behaviour)
        {
          behaviour = (CharacterBody.ItemBehavior)body.gameObject.AddComponent(buffBehaviour.behaviour);
          behaviour.body = body;
          behaviour.enabled = true;
        }
        behaviour.stack = stack;
      }
      else if (behaviour)
      {
        UnityEngine.Object.Destroy(behaviour);
      }
    }
  }
}
