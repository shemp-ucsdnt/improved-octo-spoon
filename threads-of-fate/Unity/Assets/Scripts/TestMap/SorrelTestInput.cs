using UnityEngine;
using ThreadsOfFate.Sorrel;

namespace ThreadsOfFate.TestMap
{
	// Sorrel's slice of the keyboard test harness. All 18 canonical forms
	// exist as data (see TestMapBuilder.BuildSorrelKit); these 4 are just
	// the ones exposed to the keyboard, chosen to match the test map's
	// existing world-puzzle pieces (heavy switch, narrow passage, hazard
	// flyover) one-for-one with the discipline's own family lineup:
	//   1/2/3/4    = transform into Fangstrider/Drift-Eel/Cragclaw/Root-Sprite
	//                (requires already holding that form's coin)
	//   0          = revert to base form
	//   Left Mouse = use the current form's special ability (flat MP cost)
	//
	// Disabled by default; CharacterSwitchController enables this only while
	// Sorrel is the active character.
	public class SorrelTestInput : MonoBehaviour
	{
		public SorrelTransformationController transformation;

		public const string FangstriderId = "Fangstrider";
		public const string DriftEelId = "DriftEel";
		public const string CragclawId = "Cragclaw";
		public const string RootSpriteId = "RootSprite";

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1)) transformation.TransformInto(FangstriderId);
			if (Input.GetKeyDown(KeyCode.Alpha2)) transformation.TransformInto(DriftEelId);
			if (Input.GetKeyDown(KeyCode.Alpha3)) transformation.TransformInto(CragclawId);
			if (Input.GetKeyDown(KeyCode.Alpha4)) transformation.TransformInto(RootSpriteId);
			if (Input.GetKeyDown(KeyCode.Alpha0)) transformation.RevertToBaseForm();

			if (Input.GetMouseButtonDown(0)) transformation.UseFormAbility();
		}
	}
}
