using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace PlasmaCoreSpikestripContent.Core;

public static class PlasmaUtils
{
	public static Vector3 RandomTerrainPositionInCylinder(Vector3 position, float radius, float topOfCylinderHeight)
	{
		float num = UnityEngine.Random.Range(0f, radius);
		float f = UnityEngine.Random.Range(0f, (float)Math.PI * 2f);
		Vector3 vector = new Vector3(position.x + Mathf.Sin(f) * num, position.y + topOfCylinderHeight, position.z + Mathf.Cos(f) * num);
		Ray ray = new Ray(vector, Vector3.down);
		RaycastHit hitInfo = default(RaycastHit);
		if (Physics.Raycast(ray, out hitInfo, topOfCylinderHeight * 3f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
		{
			return hitInfo.point;
		}
		return vector - Vector3.down * topOfCylinderHeight * 3f;
	}

	public static Vector3 GetNearestNode(Vector3 position, float maxDist = float.PositiveInfinity, HullClassification hullSize = HullClassification.Human, bool useAirNodes = false)
	{
		Vector3 position2 = position;
		if (!SceneInfo.instance)
		{
			PlasmaLog.LogWarn("GetNearestNode: SceneInfo.instance is null!");
			return position2;
		}
		NodeGraph nodeGraph = ((!useAirNodes) ? SceneInfo.instance.airNodes : SceneInfo.instance.groundNodes);
		NodeGraph.NodeIndex nodeIndex = nodeGraph.FindClosestNode(position, hullSize, maxDist);
		if (nodeIndex != NodeGraph.NodeIndex.invalid)
		{
			nodeGraph.GetNodePosition(nodeIndex, out position2);
			return position2;
		}
		return position;
	}

	public static void RegisterNetworkPrefabs(GameObject[] gameObjects)
	{
		for (int i = 0; i < gameObjects.Length; i++)
		{
			RegisterNetworkPrefab(gameObjects[i]);
		}
	}

	public static void RegisterNetworkPrefab(GameObject gameObject)
	{
		NetworkIdentity networkIdentity = gameObject.GetComponent<NetworkIdentity>();
		if (!networkIdentity)
		{
			PlasmaLog.LogWarn("RegisterNetworkPrefab: Adding NetworkID to prefab " + gameObject.name + ", as it has none.");
			networkIdentity = gameObject.AddComponent<NetworkIdentity>();
		}
		if ((bool)networkIdentity)
		{
			if (networkIdentity.assetId.IsValid())
			{
				ClientScene.RegisterPrefab(gameObject);
				return;
			}
			MethodBase method = new StackFrame(2).GetMethod();
			ClientScene.RegisterPrefab(gameObject, NetworkHash128.Parse(PrefabAPIMakeHash(gameObject.name + ((MemberInfo)method).DeclaringType.AssemblyQualifiedName + ((MemberInfo)method).Name)));
		}
	}

	public static string PrefabAPIMakeHash(string s)
	{
		MD5 mD = MD5.Create();
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(s));
		foreach (byte b in array)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		mD.Dispose();
		return stringBuilder.ToString();
	}
}
