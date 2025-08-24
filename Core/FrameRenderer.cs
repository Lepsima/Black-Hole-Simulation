using System;
using System.IO;
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
	private bool isInVideoRenderMode;
	private bool isReadyToRender;
	private float renderTime;
	private int renderFrame;
	private int renderTotalFrames;
	private string renderPath;
	
	private RenderPoint renderStartPoint;
	private RenderPoint renderEndPoint;
	
	public FrameRenderer() {
		Instance = this;
	}

	public void SetVideoPoint(bool startPoint) {
		RenderPoint renderPoint = Camera.Instance.GetRenderPoint();

		if (startPoint) {
			renderStartPoint = renderPoint;
		}
		else {
			renderEndPoint = renderPoint;
		}
	}
	
	public void Load(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) {
		this.graphicsDevice = graphicsDevice;
		this.spriteBatch = spriteBatch;
	}
	
	protected override void LoadSettings() {
		isInVideoRenderMode = Settings.CurrentSettings.videoRenderMode;
		isReadyToRender = false;
		renderTotalFrames = (int)MathF.Round(Settings.CurrentSettings.videoFrameRate * Settings.CurrentSettings.videoDuration);
		renderPath = Settings.CurrentSettings.videoRenderLOCALDirectory;
		renderTime = 0.0f;
		renderFrame = 0;

        screenRect = new Rectangle(0, 0, Settings.ResolutionX, Settings.ResolutionY); 

		if (isInVideoRenderMode) {
			renderTarget = new RenderTarget2D(graphicsDevice, Settings.ResolutionX, Settings.ResolutionY);
			Directory.CreateDirectory(renderPath);
			
		} else {
			renderTarget?.Dispose();
		}
	}

	public void DrawFrame(GameTime gameTime) {
		float seconds = (float)gameTime.TotalGameTime.TotalSeconds;
		isReadyToRender = renderStartPoint != null && renderEndPoint != null;

		// Use fixed delta time for video rendering
		if (isInVideoRenderMode && isReadyToRender) {
			if (renderFrame >= renderTotalFrames) {
				return;
			}
			
			float videoDelta = 1.0f / Settings.CurrentSettings.videoFrameRate;
			renderTime += videoDelta;
			seconds = renderTime;
			
			float t = (renderFrame + 1.0f) / renderTotalFrames;
			RenderPoint renderPoint = RenderPoint.Lerp(renderStartPoint, renderEndPoint, t);
			Camera.Instance.SetRenderData(renderPoint);
		}

		// Render black hole and bloom
		Texture2D blackHoleTex = BlackHole.Instance.Draw(seconds);
		Texture2D bloomTex = BloomFilter.Instance.Draw(blackHoleTex);

		// Initiate target
		graphicsDevice.SetRenderTarget((isInVideoRenderMode && isReadyToRender) ? renderTarget : null);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
		
		// Draw renders to target
		spriteBatch.Draw(blackHoleTex, screenRect, Color.White);
		spriteBatch.Draw(bloomTex, screenRect, Color.White);
		spriteBatch.End();

		// Draw menu/info
		if (isInVideoRenderMode) {
			if (isReadyToRender) {
				string frameName = $"frame_{renderFrame:00000}.png";

				// Maybe store as lower resolution
				int x = (int)(Settings.ResolutionX * Settings.CurrentSettings.videoStoreWidthScale);
				int y = (int)(Settings.ResolutionY * Settings.CurrentSettings.videoStoreHeightScale);
			
				// Store frame
				Stream stream = File.OpenWrite(Path.Combine(renderPath, frameName));
				renderTarget.SaveAsPng(stream, x, y);
				stream.Dispose();
			
				// Render details
				string[] renderInfo = [
					"RENDERING MODE - GENERATING IMAGES",
					$"\nWRITING TO: {Settings.CurrentSettings.videoRenderLOCALDirectory}",
					$"FRAME NAME: {frameName}",
					$"FRAME: {renderFrame + 1} / {renderTotalFrames}",
					$"RENDER TIME: {renderTime} / {Settings.CurrentSettings.videoDuration}",
					(renderFrame + 1 >= renderTotalFrames) ? "RENDER FINISHED. \nYou may now close the app and review the rendered frames" :  "RENDERING..."
				];
			
				// Render the frame and info to the screen
				graphicsDevice.SetRenderTarget(null);
				spriteBatch.Begin();
				spriteBatch.Draw(renderTarget, screenRect, Color.White);
				spriteBatch.End();
			
				TextRenderer.Instance.Draw(renderInfo);
				renderFrame++;
			}
			else {
				// Render details
				string[] renderInfo = [
					"RENDERING MODE - WAITING FOR USER INPUT",
					$"\nSTART POINT SET: {renderStartPoint != null}",
					$"END POINT SET: {renderStartPoint != null}",
					"\nRENDERING WILL START AUTOMATICALLY ONCE BOTH POINTS ARE SET",
					"\nPress [1] to select current camera coordinates as START POINT",
					"Press [2] to select current camera coordinates as END POINT",
				];
				
				TextRenderer.Instance.Draw(renderInfo);
			}
		}
		else {
			// Draw the menu text
			TextRenderer.Instance.DrawMenuText(Core.CurrentMenu, Core.IsMenuOpen);
			
		}
	}
}