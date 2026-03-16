using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class JPWater : MonoBehaviour
{
	[Header("REFLECTION RENDERTEXTURE")]
	public Camera reflectionCamera = null;
	[Range(8, 1024)] public int resolution = 128;
	public Material waterMat = null;
	private RenderTexture reflectionRt = null;
	private static bool onRend = false;

	[Header("UNDERWATER FX")]
	[SerializeField] bool useUnderwaterFx = true;
	public Light directionalLight;
	public Behaviour sunflare; // Changed from FlareLayer as FlareLayer is deprecated/unsupported in URP
	public AudioSource underwaterSnd;
	public Texture[] lightCookie;
	[SerializeField] private float underwaterDensity = 0.0f;
	private float startFogDensity = 0.001f;
	private Vector3 startLightDir = Vector3.zero;
	private Color startFogColor = new Color(0.7f, 0.9f, 1.0f, 1.0f);
	private float screenWaterY = 1;

	[Header("WATER PARTICLES FX")]
	[SerializeField] bool useParticlesFx = true;
	public ParticleSystem ripples;
	public ParticleSystem splash;
	public AudioClip splashSnd;
	private float count = 0;

	private void Start()
	{
		startFogDensity = RenderSettings.fogDensity;
		startFogColor = RenderSettings.fogColor;
		if (directionalLight) startLightDir = directionalLight.transform.forward;
	}

	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += RenderWaterReflection;
	}

	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= RenderWaterReflection;
	}

	private void RenderWaterReflection(ScriptableRenderContext context, Camera cam)
	{
		if (onRend | !cam | !reflectionCamera | !waterMat) return;

		// Prevent recursive rendering if the camera is the reflection camera itself
		if (cam.cameraType == CameraType.Reflection || cam == reflectionCamera) return;

		if (reflectionRt == null | (reflectionRt && reflectionRt.width != resolution))
		{ reflectionRt = RenderTexture.GetTemporary(resolution, resolution, 24); }

		Vector3 n = transform.up, p = transform.position; float l = -Vector3.Dot(n, p);
		Matrix4x4 m = new Matrix4x4
		{
			m00 = 1 - 2 * n.x * n.x,
			m01 = -2 * n.x * n.y,
			m02 = -2 * n.x * n.z,
			m03 = -2 * l * n.x,
			m10 = -2 * n.x * n.y,
			m11 = 1 - 2 * n.y * n.y,
			m12 = -2 * n.y * n.z,
			m13 = -2 * l * n.y,
			m20 = -2 * n.x * n.z,
			m21 = -2 * n.y * n.z,
			m22 = 1 - 2 * n.z * n.z,
			m23 = -2 * l * n.z,
			m30 = 0,
			m31 = 0,
			m32 = 0,
			m33 = 1
		};

		//Set Cam
		reflectionCamera.enabled = false;
		Vector3 n2 = -cam.worldToCameraMatrix.MultiplyVector(n).normalized;
		Vector4 clip = new Vector4(n2.x, n2.y, n2.z, -Vector3.Dot(cam.worldToCameraMatrix.MultiplyPoint((p + n) * 0.9f), n2));
		reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clip);
		reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * m;

		reflectionCamera.clearFlags = cam.clearFlags;
		reflectionCamera.backgroundColor = cam.backgroundColor;
		reflectionCamera.farClipPlane = cam.farClipPlane;
		reflectionCamera.nearClipPlane = cam.nearClipPlane;
		reflectionCamera.orthographic = cam.orthographic;
		reflectionCamera.fieldOfView = cam.fieldOfView;
		reflectionCamera.aspect = cam.aspect;
		reflectionCamera.orthographicSize = cam.orthographicSize;
		reflectionCamera.targetTexture = reflectionRt;

		reflectionCamera.transform.SetPositionAndRotation(cam.transform.position, cam.transform.rotation);
		if (reflectionCamera.rect.size != Vector2.one) return;

		onRend = true;
		GL.invertCulling = true;

		// URP specific render call
		UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);

		GL.invertCulling = false;
		onRend = false;

		waterMat.SetTexture("_ReflectionRT", reflectionRt);
	}

	//UNDERWATER EFFECT
	private void FixedUpdate()
	{
		// Camera.current is null in URP during FixedUpdate. Use Camera.main instead.
		Camera cam = Camera.main;
		if (!Application.isPlaying | !useUnderwaterFx | !cam) return;

		//Get screen water altitude
		float d_l = cam.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane)).y;
		float u_l = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, cam.nearClipPlane)).y;
		float d_r = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, cam.nearClipPlane)).y;
		float u_r = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane)).y;
		screenWaterY = Mathf.Clamp((Mathf.Min(d_l, d_r) - transform.position.y) / (Mathf.Min(d_l, d_r) - Mathf.Min(u_l, u_r)), -16.0f, 16.0f);

		//Get water material color
		// Note: Using Color instead of Color32 for Lerp in modern Unity to prevent precision loss
		Color col = Color.Lerp(waterMat.GetColor("_ReefCol"), waterMat.GetColor("_Col") * 1.5f, screenWaterY / 4f);

		//Fog color & density
		RenderSettings.fogColor = Color.Lerp(startFogColor, col, Mathf.Clamp01(screenWaterY));
		RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, underwaterDensity, Mathf.Clamp01(screenWaterY));
		cam.backgroundColor = RenderSettings.fogColor;

		if (screenWaterY > 0.5f)
		{
			if (!underwaterSnd.isPlaying)
			{
				underwaterSnd.Play();
				if (sunflare) sunflare.enabled = false;
				cam.clearFlags = CameraClearFlags.SolidColor;
				if (directionalLight) directionalLight.transform.forward = -Vector3.up;
			}
			if (lightCookie.Length > 0 && directionalLight)
				directionalLight.cookie = lightCookie[Mathf.FloorToInt((Time.fixedTime * 16) % lightCookie.Length)];
		}
		else if (underwaterSnd.isPlaying)
		{
			underwaterSnd.Stop();
			if (sunflare) sunflare.enabled = true;
			cam.clearFlags = CameraClearFlags.Skybox;
			if (directionalLight)
			{
				directionalLight.transform.forward = startLightDir;
				directionalLight.cookie = null;
			}
		}
	}

	//PARTICLES EFFECT
	private void OnTriggerStay(Collider col) { if (!useParticlesFx) return; WaterParticleFX(col, ripples); }
	private void OnTriggerExit(Collider col) { if (!useParticlesFx) return; WaterParticleFX(col, splash); }
	private void OnTriggerEnter(Collider col) { if (!useParticlesFx) return; WaterParticleFX(col, splash); }

	private void WaterParticleFX(Collider col, ParticleSystem particleFx)
	{
		count += Time.fixedDeltaTime; ParticleSystem particle;

		// Removed Creature cs reference to prevent compilation errors if the Creature class is missing in this context.
		// If you need the specific Jurassic Pack creature logic, ensure the Creature class exists in your project.

		if (col.transform.root.GetComponent<Rigidbody>())
		{
			// Spawn generic particle logic
			if (particleFx != null)
			{
				Vector3 pos = new Vector3(col.transform.position.x, transform.position.y + 0.01f, col.transform.position.z);
				particle = Instantiate(particleFx, pos, Quaternion.identity);
				particle.gameObject.hideFlags = HideFlags.HideAndDontSave;
				Destroy(particle.gameObject, 3.0f);
			}
		}
	}
}