using System;
using System.IO;
using Newtonsoft.Json;
// ReSharper disable ConvertToConstant.Global

namespace BlackHoles;

[Serializable]
public class Settings {
	public static Settings CurrentSettings { get; set; }
	public static readonly Settings DefaultSettings = new();
	
	public const string SettingsFileName = "sim-settings.json";
	public const string DefaultsFileName = "sim-defaults.json";

	public string version = Core.VERSION;
	public string build = Core.BUILD;
	
	public bool fullscreen = false;
	public bool vsync = true;
	public bool showMouse = true;
	public float targetFramerate = 60.0f;
	
	public int resolutionX = 1280;
	public int resolutionY = 720;
	public float fov = 60.0f;
	public float simulationStepSize = 0.1f;
	public float skyboxBrightness = 0.5f;
	
	public float diskInnerRadius = 3.5f;
	public float diskOuterRadius = 15.0f;
	public float diskThickness = 0.2f;
	
	public float diskMaxTemp = 4900.0f;
	public float diskMinTemp = 4300.0f;
	public float diskMaxVelocity = 0.1f;
	public float diskMinVelocity = 0.01f;
	
	public static int ResolutionX => CurrentSettings.resolutionX;
	public static int ResolutionY => CurrentSettings.resolutionY;

	public static bool CreateIfMissing(string filename, Settings settings = null) {
		if (File.Exists(filename)) return false;
		
		WriteSettings(filename, settings ?? DefaultSettings);
		return true;
	}

	public static Settings ReadOrCreateIfMissing(string filename, Settings settings = null) {
		settings ??= DefaultSettings;
		return CreateIfMissing(filename, settings) ? settings : ReadSettings(filename);
	}

	public static void AutoLoadCurrentSettings(string filename) {
		CurrentSettings = ReadOrCreateIfMissing(filename);
	}

	public static Settings ReadSettings(string filename) {
		using StreamReader streamReader = new(filename);
		JsonSerializer serializer = new();
		return (Settings)serializer.Deserialize(streamReader, typeof(Settings));
	}

	public static void WriteSettings(string filename, Settings settings) {
		string indentedSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);
		
		using StreamWriter streamWriter = new(filename);
		streamWriter.Write(indentedSettings);
	}
}