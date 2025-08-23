using System;
using Microsoft.Xna.Framework;

namespace BlackHoles;

/* --- Optimization Records ---
 *
 * 21 fps -> Base implementation, no optimizations
 * 44 fps -> Adjusted step size, iterations, escape radius and small optimizations
 * 
*/

public class FrameStats {
	public static FrameStats Instance { get; private set; }
	
	private double _lastFrameTime;

	private double _accumulatedFrameTime;
	private int _framesToAccumulate;

	private const int FPS_Interval = 8;

	public double FPS { get; private set; }
	public double DeltaTime { get; private set; }

	public Action OnFPSChanged;
	
	public FrameStats() {
		Instance = this;
	}
	
	public void Update(GameTime gameTime) {
		double time = gameTime.TotalGameTime.TotalSeconds;
		DeltaTime = time - _lastFrameTime;
		
		_accumulatedFrameTime += DeltaTime;
		_framesToAccumulate--;

		if (_framesToAccumulate <= 0) {
			FPS = FPS_Interval / _accumulatedFrameTime;
			OnFPSChanged?.Invoke();
			
			_framesToAccumulate = FPS_Interval;
			_accumulatedFrameTime = 0;
		}
		
		_lastFrameTime = time;
	}
}