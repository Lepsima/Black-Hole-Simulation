using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlackHoles;

public class Camera {
	public static Camera Instance { get; private set; }
	private const float maxRadius = 50f;
	private const float minRadius = 0.5f;
	
	private const float zoomSpeed = 0.0025f;
	private const float orbitSpeed = 0.0025f;
	private const float aimSpeed = 0.0025f;
	
	private float radius = 15.0f;
	private float orbit_azimuth;
	private float orbit_elevation = float.Pi / 1.9f;

	private float aim_azimuth = float.Pi;
	private float aim_elevation = float.Pi / 1.9f;
	
	private bool isOrbiting;
	private bool isAiming;
	
	private int lastX;
	private int lastY;
	private int lastScroll;

	public Camera() {
		Instance = this;
	}

	public Vector3[] GetShaderData() {
		Vector3[] data = new Vector3[4];
		data[0] = Position();
		data[1] = Vector3.Normalize(Direction());
		data[2] = Vector3.Normalize(Vector3.Cross(data[1], Vector3.Up));
		data[3] = Vector3.Normalize(Vector3.Cross(data[2], data[1]));
		return data;
	}
	
	public Vector3 Position() {
		return new Vector3(
			radius * MathF.Sin(orbit_elevation) * MathF.Cos(orbit_azimuth),
			radius * MathF.Cos(orbit_elevation),
			radius * MathF.Sin(orbit_elevation) * MathF.Sin(orbit_azimuth)
		);
	}

	public Vector3 Direction() {
		return new Vector3(
			MathF.Sin(aim_elevation) * MathF.Cos(aim_azimuth + orbit_azimuth),
			MathF.Cos(aim_elevation),
			MathF.Sin(aim_elevation) * MathF.Sin(aim_azimuth + orbit_azimuth)
		);
	}

	public void Update() {
		MouseState mouseState = Mouse.GetState();
		
		// Scroll zoom
		int scroll = mouseState.ScrollWheelValue;
		int deltaScroll = scroll - lastScroll;

		radius = Math.Clamp(radius - deltaScroll * zoomSpeed, minRadius, maxRadius);
		lastScroll = scroll;
		
		// Position delta
		int x = mouseState.Position.X;
		int y = mouseState.Position.Y;

		float dx = x - lastX;
		float dy = y - lastY;
		
		lastX = x;
		lastY = y;
		
		if (dx == 0 && dy == 0) return;

		// Movement type
		isOrbiting = mouseState.LeftButton == ButtonState.Pressed || mouseState.MiddleButton == ButtonState.Pressed;
		isAiming = mouseState.RightButton == ButtonState.Pressed;
		
		// Orbit camera around center
		if (isOrbiting && !isAiming) {
			orbit_azimuth -= dx * orbitSpeed;
			orbit_elevation -= dy * orbitSpeed * 0.25f;
			orbit_elevation = Math.Clamp(orbit_elevation, 0.01f, float.Pi - 0.01f);
			
			// Aim camera freely
		} else if (isAiming) {
			aim_azimuth += dx * aimSpeed;
			aim_elevation -= dy * aimSpeed;
			aim_elevation = Math.Clamp(aim_elevation, 0.01f, float.Pi - 0.01f);
		}
	}
}