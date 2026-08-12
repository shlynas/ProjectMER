using System.Runtime.CompilerServices;
using AdminToys;
using Hazards;
using LabApi.Features.Wrappers;
using Mirror;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using RelativePositioning;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable;

public sealed class SerializablePrismaticCloud : SerializableObject, IIndicatorDefinition
{
	public float HazardDuration { get; set; } = 90f;
	public string EffectName { get; set; } = "Prismatic";
	public byte EffectIntensity { get; set; } = 1;
	public float EffectDuration { get; set; } = 3f;
	public bool IsDestroyable { get; set; } = true;

	public void Configure(PrismaticCloud cloud)
	{
		cloud.HazardDuration = Mathf.Max(0f, HazardDuration);
		RuntimeSettings.Remove(cloud);
		RuntimeSettings.Add(cloud, this);
	}

	public static string GetEffectName(PrismaticCloud cloud) =>
		NormalizeEffectName(GetRuntimeSettings(cloud)?.EffectName);

	public static byte GetEffectIntensity(PrismaticCloud cloud) =>
		GetRuntimeSettings(cloud)?.EffectIntensity ?? 1;

	public static float GetEffectDuration(PrismaticCloud cloud) =>
		Mathf.Max(0f, GetRuntimeSettings(cloud)?.EffectDuration ?? 3f);

	public static bool CanBeDestroyed(PrismaticCloud cloud) =>
		GetRuntimeSettings(cloud)?.IsDestroyable ?? true;

	public static void SyncPosition(PrismaticCloud cloud, Vector3 position)
	{
		cloud.SourcePosition = position;
		cloud.SynchronizedPosition = new RelativePosition(position);
	}

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		GameObject cloudObject = instance == null
			? UnityEngine.Object.Instantiate(PrefabManager.PrismaticCloud)
			: instance;

		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		cloudObject.transform.SetPositionAndRotation(position, rotation);
		cloudObject.transform.localScale = Scale;
		HazardDuration = Mathf.Max(0f, HazardDuration);

		if (cloudObject.TryGetComponent(out PrismaticCloud cloud))
		{
			Configure(cloud);
			SyncPosition(cloud, position);
		}

		_prevHazardDuration = HazardDuration;
		_prevEffectName = EffectName;
		_prevEffectIntensity = EffectIntensity;
		_prevEffectDuration = EffectDuration;
		_prevIsDestroyable = IsDestroyable;

		if (instance == null)
			NetworkServer.Spawn(cloudObject);

		return cloudObject;
	}

	public GameObject SpawnOrUpdateIndicator(Room room, GameObject? instance = null)
	{
		PrimitiveObjectToy cube = instance == null
			? UnityEngine.Object.Instantiate(PrefabManager.PrimitiveObject)
			: instance.GetComponent<PrimitiveObjectToy>();
		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);

		cube.transform.SetPositionAndRotation(position, rotation);
		cube.NetworkPrimitiveType = PrimitiveType.Cube;
		cube.NetworkPrimitiveFlags = PrimitiveFlags.Visible;
		cube.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		cube.NetworkMaterialColor = new Color(0.7f, 0.2f, 0.9f, 0.7f);

		return cube.gameObject;
	}

	public override bool RequiresReloading =>
		HazardDuration != _prevHazardDuration ||
		EffectName != _prevEffectName ||
		EffectIntensity != _prevEffectIntensity ||
		EffectDuration != _prevEffectDuration ||
		IsDestroyable != _prevIsDestroyable ||
		base.RequiresReloading;

	private float _prevHazardDuration;
	private string _prevEffectName = string.Empty;
	private byte _prevEffectIntensity;
	private float _prevEffectDuration;
	private bool _prevIsDestroyable;

	private static SerializablePrismaticCloud? GetRuntimeSettings(PrismaticCloud cloud) =>
		cloud != null && RuntimeSettings.TryGetValue(cloud, out SerializablePrismaticCloud settings) ? settings : null;

	private static string NormalizeEffectName(string? effectName)
	{
		if (string.IsNullOrWhiteSpace(effectName))
			return "Prismatic";

		return effectName!.Trim();
	}

	private static readonly ConditionalWeakTable<PrismaticCloud, SerializablePrismaticCloud> RuntimeSettings = new();
}
