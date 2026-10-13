using System.IO;

namespace VeiniaFramework.Samples.BlockBreaker
{
	public class MetalBlock : Tile
	{
		private int hitPoints = 2;


		public override void Hit()
		{
			hitPoints--;
			GetComponent<Sprite>().ChangeTexture(Path.Combine("Sprites", "Metal Tile Broken"));
			if (hitPoints <= 0)
				base.Hit();
		}
	}
}
