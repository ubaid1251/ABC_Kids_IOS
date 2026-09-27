using ScratchCardAsset.Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScratchCardAsset
{
	public class EraseProgress : MonoBehaviour
	{
		public ScratchCard Card;
		public event ProgressHandler OnProgress;
		public event ProgressHandler OnCompleted;

		public delegate void ProgressHandler(float progress);

		private ScratchCard.ScratchMode scratchMode;
		private RenderTexture percentRenderTexture;
		private RenderTargetIdentifier rti;
		private CommandBuffer commandBuffer;
		private Mesh mesh;
		private float currentProgress;
		public bool isCompleted;
		public RevealColorSoundHelp SoundObject;
		public bool eraseDone=false;
		#region MonoBehaviour Methods

		void Start()
		{
			isCompleted = false;

			if (GetComponent<AudioSource>())
			{
				if (PlayerPrefs.GetInt("sfx") == 1)
				{
					GetComponent<AudioSource>().enabled = false;
				}
			}
			Init();
		}

		void OnDestroy()
		{
			if (percentRenderTexture != null && percentRenderTexture.IsCreated())
			{
				percentRenderTexture.Release();
				Destroy(percentRenderTexture);
			}

			if (mesh != null)
			{
				Destroy(mesh);
			}

			if (commandBuffer != null)
			{
				commandBuffer.Release();
			}
		}

		void Update()
		{
			if (Card.Mode != scratchMode)
			{
				scratchMode = Card.Mode;
				ResetProgress();
			}

			if (Card.IsScratched && !isCompleted)
			{
				UpdateProgress();
			}
		}

		#endregion

		private void Init()
		{
			scratchMode = Card.Mode;
			commandBuffer = new CommandBuffer {name = "EraseProgress"};
			percentRenderTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32);
			rti = new RenderTargetIdentifier(percentRenderTexture);
			mesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
		}

		/// <summary>
		/// Calculates scratch progress
		/// </summary>
		private void CalcProgress()
		{
			if (!isCompleted)
			{
				var prevRenderTextureT = RenderTexture.active;
				RenderTexture.active = percentRenderTexture;
				var progressTexture = new Texture2D(percentRenderTexture.width, percentRenderTexture.height,
					TextureFormat.ARGB32, false, true);
				progressTexture.ReadPixels(new Rect(0, 0, percentRenderTexture.width, percentRenderTexture.height), 0,
					0);
				progressTexture.Apply();
				RenderTexture.active = prevRenderTextureT;
				var red = progressTexture.GetPixel(0, 0).r;
				currentProgress = red;
                if ((currentProgress * 100) < 10)
                {
					ABCManager.instance.ColorScratch.Stop();
					ABCManager.instance.ColorScratch.enabled = false;
					print("Reached");
					if (SoundObject)
					{
						SoundObject.Play();
					}
					Card.Clear();
                    for (int i = 0; i < Card.anim.Length; i++)
                    {
						Card.anim[i].enabled = true;
					}
					for (int i = 0; i < Card.other.Length; i++)
                    {
						Card.other[i].SetActive(true);
					}
					for (int i = 0; i < Card.otherToOff.Length; i++)
                    {
						Card.otherToOff[i].SetActive(false);
					}
					Card.dummy.enabled = false;
					isCompleted = true;
                    
					if (GetComponent<AudioSource>())
					{
						AudioSource c = GetComponent<AudioSource>();
						if (PlayerPrefs.GetInt("sfx") == 0)
						{
							c.Play();
						}
						else
						{
							c.enabled = false;
						}
						Invoke(nameof(ShowBalloon), c.clip.length);
						//ABCManager.instance.title.gameObject.SetActive(true);
					}
				}
			}
		}
		void ShowBalloon()
        {
			ABCManager.instance.balloonPanel.SetActive(true);
        }

		#region Public Methods

		public float GetProgress()
		{
			return currentProgress;
		}

		public void UpdateProgress()
		{
			if (eraseDone)
			{
				GL.LoadOrtho();
				commandBuffer.Clear();
				commandBuffer.SetRenderTarget(rti);
				commandBuffer.ClearRenderTarget(false, true, Color.clear);
				commandBuffer.DrawMesh(mesh, Matrix4x4.identity, Card.Progress);
				Graphics.ExecuteCommandBuffer(commandBuffer);
				CalcProgress();
			}
		}

		public void ResetProgress()
		{
			isCompleted = false;
		}

		#endregion
	}
}