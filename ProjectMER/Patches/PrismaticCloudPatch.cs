using CustomPlayerEffects;
using Hazards;
using HarmonyLib;
using ProjectMER.Features.Serializable;

namespace ProjectMER.Patches;

internal static class PrismaticCloudPatch
{
	[HarmonyPatch(typeof(PrismaticCloud), "ServerEnableEffect")]
	private static class EffectPatch
	{
		private static bool Prefix(PrismaticCloud __instance, ReferenceHub target, bool changeOrigin = true)
		{
			StatusEffectBase? effect = target?.playerEffectsController?.ChangeState(
				SerializablePrismaticCloud.GetEffectName(__instance),
				SerializablePrismaticCloud.GetEffectIntensity(__instance),
				SerializablePrismaticCloud.GetEffectDuration(__instance));
			if (changeOrigin && effect is Prismatic prismatic)
				prismatic.OriginCloud = __instance;

			return false;
		}
	}

	[HarmonyPatch(typeof(PrismaticCloud), "CheckExplosion")]
	private static class DestructibilityPatch
	{
		private static bool Prefix(PrismaticCloud __instance) => SerializablePrismaticCloud.CanBeDestroyed(__instance);
	}
}
