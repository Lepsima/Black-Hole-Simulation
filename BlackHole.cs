using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlackHoles;

public class BlackHole : Configurable {
	public static BlackHole Instance { get; private set; }
	private GraphicsDevice graphicsDevice;
	private Effect computeShader;
	
	private Texture2D outputTex;
	private Texture2D backgroundTex;
	private Texture2D noiseTex;

	private const int THREADS = 8;
	private int THREADS_X;
	private int THREADS_Y;
	private const int THREADS_Z = 1;
	
	public BlackHole() {
		Instance = this;
	}

	public void Load(GraphicsDevice graphicsDevice, ContentManager content) {
		this.graphicsDevice	= graphicsDevice;
		
		computeShader = content.Load<Effect>("BlackHole");
		backgroundTex = content.Load<Texture2D>("background");
		noiseTex = content.Load<Texture2D>("noiseTexture512x256");
	}

	protected override void LoadSettings() {
		THREADS_X = Settings.ResolutionX / THREADS + 1;
		THREADS_Y = Settings.ResolutionY / THREADS + 1;

		outputTex?.Dispose();
		outputTex = new Texture2D(graphicsDevice, Settings.ResolutionX, Settings.ResolutionY, ShaderAccess.ReadWrite);
		
		computeShader.Parameters["width"].SetValue(Settings.ResolutionX);
		computeShader.Parameters["height"].SetValue(Settings.ResolutionY);
		
		computeShader.Parameters["output"].SetValue(outputTex);
		computeShader.Parameters["noise_tex"].SetValue(noiseTex);
		computeShader.Parameters["background_tex"].SetValue(backgroundTex);
		computeShader.Parameters["skybox_brightness"].SetValue(Settings.CurrentSettings.skyboxBrightness);
		
		computeShader.Parameters["disk_r1"].SetValue(Settings.CurrentSettings.diskInnerRadius);
		computeShader.Parameters["disk_r2"].SetValue(Settings.CurrentSettings.diskOuterRadius);
		computeShader.Parameters["disk_y"].SetValue(Settings.CurrentSettings.diskThickness);
		computeShader.Parameters["base_step_size"].SetValue(Settings.CurrentSettings.simulationStepSize);
		
		computeShader.Parameters["MaxTemp"].SetValue(Settings.CurrentSettings.diskMaxTemp);
		computeShader.Parameters["MinTemp"].SetValue(Settings.CurrentSettings.diskMinTemp);
		computeShader.Parameters["MaxVel"].SetValue(Settings.CurrentSettings.diskMaxVelocity);
		computeShader.Parameters["MinVel"].SetValue(Settings.CurrentSettings.diskMinVelocity);
		
		
		computeShader.Parameters["cam_aspect"].SetValue((float)Settings.ResolutionX / Settings.ResolutionY); 
		computeShader.Parameters["cam_tan_half_fov"].SetValue(MathF.Tan(MathHelper.ToRadians(Settings.CurrentSettings.fov * 0.5f)));
	}
	
	public Texture2D Draw(GameTime gameTime) {
		Vector3[] cameraData = Camera.Instance.GetShaderData();
		computeShader.Parameters["cam_pos"].SetValue(cameraData[0]);
		computeShader.Parameters["cam_forward"].SetValue(cameraData[1]);
		computeShader.Parameters["cam_right"].SetValue(cameraData[2]);
		computeShader.Parameters["cam_up"].SetValue(cameraData[3]);
		computeShader.Parameters["disk_time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds * 10.0f);
		
		computeShader.CurrentTechnique.Passes[0].ApplyCompute();
		graphicsDevice.DispatchCompute(THREADS_X, THREADS_Y, THREADS_Z);

		return outputTex;
	}
}