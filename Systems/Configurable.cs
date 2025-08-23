using System.Collections.Generic;

namespace BlackHoles;

public abstract class Configurable {
	private static readonly List<Configurable> All = [];
	
	protected Configurable() {
		All.Add(this);
	}

	protected abstract void LoadSettings();

	public static void ApplySettingsToAll() {
		foreach (Configurable configurable in All) configurable.LoadSettings();
	}
}