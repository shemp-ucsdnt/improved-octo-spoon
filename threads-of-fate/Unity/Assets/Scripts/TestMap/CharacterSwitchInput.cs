using UnityEngine;
using ThreadsOfFate.Core;

namespace ThreadsOfFate.TestMap
{
	// F1/F2/F3 select Sorrel/Thyme/Chervil. Always enabled (unlike the
	// per-character input scripts, which are toggled by the switch
	// controller itself).
	public class CharacterSwitchInput : MonoBehaviour
	{
		public CharacterSwitchController controller;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F1)) TrySwitch(CharacterId.Sorrel);
			if (Input.GetKeyDown(KeyCode.F2)) TrySwitch(CharacterId.Thyme);
			if (Input.GetKeyDown(KeyCode.F3)) TrySwitch(CharacterId.Chervil);
		}

		private void TrySwitch(CharacterId id)
		{
			controller.SwitchTo(id);
		}
	}
}
