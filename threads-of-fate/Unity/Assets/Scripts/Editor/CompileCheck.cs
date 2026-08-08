using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThreadsOfFate.Tooling
{
	// Batch-mode entry point for `-executeMethod ThreadsOfFate.Tooling.CompileCheck.Run`.
	// By the time this method runs, Unity has already imported the project and
	// compiled every C# script under Assets/ — if compilation had failed,
	// batch mode logs the errors and never reaches this method at all. Its
	// only job is to prove that back to the caller: write a short report to
	// render-output/ (so the oci-unity-render skill's rsync step has
	// something to pull back) and exit 0.
	public static class CompileCheck
	{
		public static void Run()
		{
			string outputDir = Path.Combine(Application.dataPath, "..", "render-output");
			Directory.CreateDirectory(outputDir);

			string reportPath = Path.Combine(outputDir, "compile_check_report.txt");
			File.WriteAllText(reportPath,
				"Compile check passed — all C# scripts under Assets/ compiled successfully.\n" +
				$"Unity version: {Application.unityVersion}\n" +
				$"Platform: {Application.platform}\n" +
				$"Timestamp (UTC): {DateTime.UtcNow:O}\n");

			Debug.Log("[CompileCheck] All scripts compiled successfully.");
			EditorApplication.Exit(0);
		}
	}
}
