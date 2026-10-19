using System.Collections.Generic;
using GrooveSaladSpikestripContent;
using RoR2;
using UnityEngine;
using UnityEngine.Events;
using Spikestrip2_0.Core;

public class LoadMaterialFromAdress : MonoBehaviour
{
	public CharacterModel targetCharacterModel;

	public Renderer targetRenderer;

	public string[] matAddresses;

	public UnityEvent action;

	public ParticleSystemRenderer targetTrail;

	private void Awake()
	{
		if ((bool)targetTrail)
		{
			targetTrail.trailMaterial = SpikestripUtil.AddressablesLoad<Material>(matAddresses[0]);
		}
		if ((bool)targetCharacterModel)
		{
			for (int i = 0; i < matAddresses.Length && i < targetCharacterModel.baseRendererInfos.Length; i++)
			{
				targetCharacterModel.baseRendererInfos[i].defaultMaterial = SpikestripUtil.AddressablesLoad<Material>(matAddresses[i]);
			}
		}
		if ((bool)targetRenderer)
		{
			List<Material> list = new List<Material>();
			for (int num = matAddresses.Length - 1; num >= 0; num--)
			{
				list.Add(SpikestripUtil.AddressablesLoad<Material>(matAddresses[num]));
			}
			targetRenderer.SetMaterials(list);
		}
		action.Invoke();
	}
}
