using AdminToys;
using LabApi.Features.Wrappers;
using MapGeneration.Distributors;
using Mirror;
using ProjectMER.Features.Enums;
using ProjectMER.Features.Extensions;
using ProjectMER.Features.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace ProjectMER.Features.Serializable;

public sealed class SerializableConnector : SerializableObject, IIndicatorDefinition
{
	public MirrorPrefabType ConnectorType { get; set; } = MirrorPrefabType.BrokenElectricalBox;

	public override GameObject SpawnOrUpdateObject(Room? room = null, GameObject? instance = null)
	{
		GameObject connector = instance == null
			? UnityEngine.Object.Instantiate(PrefabManager.GetMirrorPrefab(ConnectorType))
			: instance;

		Vector3 position = room.GetAbsolutePosition(Position);
		Quaternion rotation = room.GetAbsoluteRotation(Rotation);
		_prevIndex = Index;

		connector.transform.SetPositionAndRotation(position, rotation);
		connector.transform.localScale = Scale;

		_prevType = ConnectorType;
		_prevScale = Scale;
		_prevPosition = Position;
		_prevRotation = Rotation;

		if (connector.TryGetComponent(out StructurePositionSync structurePositionSync))
		{
			structurePositionSync.Network_position = connector.transform.position;
			structurePositionSync.Network_rotationY = (sbyte)Mathf.RoundToInt(connector.transform.rotation.eulerAngles.y / 5.625f);
		}

		if (instance == null)
			NetworkServer.Spawn(connector);

		return connector;
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
		cube.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
		cube.NetworkMaterialColor = new Color(0.9f, 0.6f, 0.1f, 0.8f);

		return cube.gameObject;
	}

	public override bool RequiresReloading =>
		ConnectorType != _prevType ||
		Scale != _prevScale ||
		Position != _prevPosition ||
		Rotation != _prevRotation ||
		base.RequiresReloading;

	private MirrorPrefabType _prevType;
	private Vector3 _prevScale;
	private Vector3 _prevPosition;
	private Vector3 _prevRotation;
}
