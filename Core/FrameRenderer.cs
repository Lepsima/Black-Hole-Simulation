using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlackHoles;

public class RenderPoint {
	public float zoom;
	public float orbit_azimuth;
	public float orbit_elevation;
	
	public float aim_azimuth;
	public float aim_elevation;

	public static RenderPoint Lerp(RenderPoint a, RenderPoint b, float t) {
		return new RenderPoint {
			zoom = float.Lerp(a.zoom, b.zoom, t),
			orbit_azimuth = float.Lerp(a.orbit_azimuth, b.orbit_azimuth, t),
			orbit_elevation = float.Lerp(a.orbit_elevation, b.orbit_elevation, t),
			aim_azimuth = float.Lerp(a.aim_azimuth, b.aim_azimuth, t),
			aim_elevation = float.Lerp(a.aim_elevation, b.aim_elevation, t),
		};
	}
}

public class FrameRenderer : Configurable {
	public static FrameRenderer Instance;
	
	private GraphicsDevice graphicsDevice;
	private SpriteBatch spriteBatch;
    private Rectangle screenRect;
    
    // Render values
	private RenderTarget2D renderTarget;
	private string renderPath;
	private float renderTime;
	
	private bool areFramesComplete => renderFrame + 1 >= renderTotalFrames;
	private int renderFrame;
	private int renderTotalFrames;

	private RenderPoint renderStartPoint;
	private RenderPoint renderEndPoint;

	private bool isRendering => renderState == RenderState.GeneratingFrames;
	private RenderState renderState;
	
	private enum RenderState {
		RealTime,
		VideoRendering,
		GeneratingFrames,
		RenderComplete
	}
 	
	public FrameRenderer() {
		Instance = this;
	}

	public void SetVideoPoint(bool startPoint) {
		RenderPoint renderPoint = Camera.Instance.GetRenderPoint();

		if (startPoint) renderStartPoint = renderPoint;
		else renderEndPoint = renderPoint;

		if (renderState == RenderState.VideoRendering && renderStartPoint != null && renderEndPoint != null) {
			renderState = RenderState.GeneratingFrames;
		}
	}
	
	public void Load(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) {
		this.graphicsDevice = graphicsDevice;
		this.spriteBatch = spriteBatch;
	}
	
	protected override void LoadSettings() {
		renderState = Settings.CurrentSettings.videoRenderMode ? RenderState.VideoRendering : RenderState.RealTime;
		
		renderTotalFrames = (int)MathF.Round(Settings.CurrentSettings.videoFrameRate * Settings.CurrentSettings.videoDuration);
		renderPath = Settings.CurrentSettings.videoRenderLOCALDirectory;
		renderTime = 0.0f;
		renderFrame = 0;

        screenRect = new Rectangle(0, 0, Settings.ResolutionX, Settings.ResolutionY); 

		if (renderState == RenderState.VideoRendering) {
			renderTarget = new RenderTarget2D(graphicsDevice, Settings.ResolutionX, Settings.ResolutionY);
			Directory.CreateDirectory(renderPath);
			
		} else {
			renderTarget?.Dispose();
		}
	}

	private void WriteRenderToDisk(RenderTarget2D target) {
		string frameName = $"frame_{renderFrame:00000}.png";
		Stream stream = File.OpenWrite(Path.Combine(Settings.CurrentSettings.videoRenderLOCALDirectory, frameName));
		
		target.SaveAsPng(stream, Settings.ResolutionX, Settings.ResolutionY);
		stream.Dispose();
	}
	
	private void DrawScene(RenderTarget2D renderTarget, float seconds) {
		// Render black hole and bloom
		Texture2D blackHoleTex = BlackHole.Instance.Draw(seconds);
		Texture2D bloomTex = BloomFilter.Instance.Draw(blackHoleTex);

		// Initiate target
		graphicsDevice.SetRenderTarget(renderTarget);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
		
		// Draw renders to target
		spriteBatch.Draw(blackHoleTex, screenRect, Color.White);
		spriteBatch.Draw(bloomTex, screenRect, Color.White);
		spriteBatch.End();
	}

	private void DrawRenderToWindow() {
		graphicsDevice.SetRenderTarget(null);
		spriteBatch.Begin();
		spriteBatch.Draw(renderTarget, screenRect, Color.White);
		spriteBatch.End();
	}
	
	private string[] GetMenuText() {
		return renderState switch {
			RenderState.RealTime => TextRenderer.GetMenuText(Core.CurrentMenu, Core.IsMenuOpen),
			
			RenderState.VideoRendering => [
				"RENDERING MODE - WAITING FOR USER INPUT",
				
				"\nIf you want to exit RENDER MODE, press [O] and set 'videoRenderMode' to false" +
				"\nand then press [Enter] to apply the updated settings.",
				
				$"\nSTART POINT SET: {renderStartPoint != null}", $"END POINT SET: {renderEndPoint != null}",
				
				"\nRENDERING WILL START AUTOMATICALLY ONCE BOTH POINTS ARE SET",
				
				"\nPress [1] to select current camera coordinates as START POINT",
				"Press [2] to select current camera coordinates as END POINT",
			],
			
			RenderState.GeneratingFrames => [
				"RENDERING MODE - GENERATING IMAGES",
				
				$"\nWRITING TO: {Settings.CurrentSettings.videoRenderLOCALDirectory}",
				$"FRAME: {renderFrame + 1} / {renderTotalFrames}",
				$"RENDER TIME: {renderTime} / {Settings.CurrentSettings.videoDuration}",
			],
			
			RenderState.RenderComplete => [
				"RENDERING MODE - RENDER COMPLETE",
				"\nAll frames have been generated and stored successfully.",
				
				"\nYou can close the app now, and review the results in: " +
				$"\n{Settings.CurrentSettings.videoRenderLOCALDirectory}",
			],
			_ => null
		};
	}
	
	public void DrawFrame(GameTime gameTime) {
		if (isRendering) {
			// Logic
			UpdateRenderProgress();
			AnimateRenderCamera();
			
			// Rendering
			DrawScene(renderTarget, renderTime);
			DrawRenderToWindow();
			
			// Writing
			WriteRenderToDisk(renderTarget);
			renderFrame++;
			
			if (renderState == RenderState.GeneratingFrames && areFramesComplete) {
				renderState = RenderState.RenderComplete;
			}
		}
		else {
			// Default real-time render
			DrawScene(null, (float)gameTime.TotalGameTime.TotalSeconds);
		}

		// Display the correct menu screen
		TextRenderer.Instance.Draw(GetMenuText());
		return;

		void AnimateRenderCamera() {
			float t = (renderFrame + 1.0f) / renderTotalFrames;
			RenderPoint renderPoint = RenderPoint.Lerp(renderStartPoint, renderEndPoint, t);
			Camera.Instance.SetRenderData(renderPoint);
		}

		void UpdateRenderProgress() {
			// Set no framerate limit when rendering frames
			double frameRate = renderState == RenderState.GeneratingFrames ? 1000d : Settings.CurrentSettings.targetFramerate;
			Core.Instance.TargetElapsedTime = TimeSpan.FromSeconds(1.0d / frameRate);

			// Custom delta time, not real-time dependent
			float videoDelta = 1.0f / Settings.CurrentSettings.videoFrameRate;
			renderTime += videoDelta;
		}
	}
}