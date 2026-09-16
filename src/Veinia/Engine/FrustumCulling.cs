using Microsoft.Xna.Framework;

namespace VeiniaFramework
{
	public class FrustumCulling
	{
		public static bool autoCulling = true;
		public static float frustumCullTime = .25f;
		public static float frustumRatioToCamera = 2.5f;


		public void Update()
		{
			if (autoCulling && Timers.IsUp("frustumCull") && Globals.loader.current != null)
			{
				Cull(Globals.loader.current);
				Timers.New("frustumCull", frustumCullTime);
			}
		}

		public static void Cull(Level level, float? frustumRatioToCameraOverride = null)
		{
			float scale = frustumRatioToCameraOverride == null ? frustumRatioToCamera : frustumRatioToCameraOverride.Value;

			var frustum = Globals.camera.GetBoundingFrustum(scaleFactor: scale);

			for (int i = 0; i < level.scene.Count; i++)
			{
				var cull = frustum.Contains(level.scene[i].transform.screenPos.ToVector3()) == ContainmentType.Disjoint;
				level.scene[i].frustumCulled = cull;
			}
		}
	}
}