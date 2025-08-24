using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlackHoles;

public class FrameRenderer : Configurable {
	private GraphicsDevice graphicsDevice;
	private SpriteBatch spriteBatch;
    private Rectangle screenRect;
    
    // Render values
	private RenderTarget2D renderTarget;
	private bool isInVideoRenderMode;
	private float renderTime;
	private int renderFrame;
	private int renderTotalFrames;
	private string renderPath;
	
	public void Load(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) {
		this.graphicsDevice = graphicsDevice;
		this.spriteBatch = spriteBatch;
	}
	
	protected override void LoadSettings() {
		isInVideoRenderMode = Settings.CurrentSettings.videoRenderMode;
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

		// Use fixed delta time for video rendering
		if (isInVideoRenderMode) {
			if (renderFrame >= renderTotalFrames) {
				return;
			}
			
			float videoDelta = 1.0f / Settings.CurrentSettings.videoFrameRate;
			renderTime += videoDelta;
			seconds = renderTime;
		}

		// Render black hole and bloom
		Texture2D blackHoleTex = BlackHole.Instance.Draw(seconds);
		Texture2D bloomTex = BloomFilter.Instance.Draw(blackHoleTex);

		// Initiate target
		graphicsDevice.SetRenderTarget(isInVideoRenderMode ? renderTarget : null);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
		
		// Draw renders to target
		spriteBatch.Draw(blackHoleTex, screenRect, Color.White);
		spriteBatch.Draw(bloomTex, screenRect, Color.White);
		spriteBatch.End();

		// Draw menu/info
		if (isInVideoRenderMode) {
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
				"RENDERING MODE - THIS INFO DOES NOT APPEAR IN FRAMES",
				$"WRITING TO: {Settings.CurrentSettings.videoRenderLOCALDirectory}",
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
		}
		else {
			// Draw the menu text
			TextRenderer.Instance.DrawMenuText(Core.CurrentMenu, Core.IsMenuOpen);
			
		}
		
		renderFrame++;
	}
}