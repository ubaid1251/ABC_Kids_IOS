using System.Collections.Generic;
using UnityEngine;

namespace ScratchCardAsset.Core
{
	/// <summary>
	/// Process Input for ScratchCard (robust fingerId -> compact slot mapping)
	/// Replaces direct indexing by touch.fingerId to avoid IndexOutOfRangeException.
	/// Mouse uses slot 0.
	/// </summary>
	public class ScratchCardInput
	{
		#region Events

		public event ScratchHandler OnScratch;
		public event ScratchStartHandler OnScratchStart;
		public event ScratchLineHandler OnScratchLine;
		public event ScratchHoleHandler OnScratchHole;
		public delegate Vector2 ScratchHandler(Vector2 position);
		public delegate void ScratchStartHandler();
		public delegate void ScratchLineHandler(Vector2 start, Vector2 end);
		public delegate void ScratchHoleHandler(Vector2 position);

		#endregion

		public bool IsScratching
		{
			get
			{
				if (isScratching != null)
				{
					foreach (var scratching in isScratching)
					{
						if (scratching)
							return true;
					}
				}
				return false;
			}
		}

		private ScratchCard scratchCard;
		private Vector2[] eraseStartPositions;
		private Vector2[] eraseEndPositions;
		private Vector2 erasePosition;
		private bool[] isScratching;
		private bool[] isStartPosition;
#if UNITY_WEBGL
		private bool isWebgl = true;
#else
		private bool isWebgl = false;
#endif

		private const int MaxTouchCount = 10;

		// mapping: touch.fingerId -> compact slot (0..MaxTouchCount-1)
		private Dictionary<int, int> pointerToSlot;
		private Queue<int> freeSlots;

		public ScratchCardInput(ScratchCard card)
		{
			scratchCard = card;

			isScratching = new bool[MaxTouchCount];
			isStartPosition = new bool[MaxTouchCount];
			eraseStartPositions = new Vector2[MaxTouchCount];
			eraseEndPositions = new Vector2[MaxTouchCount];

			for (var i = 0; i < isStartPosition.Length; i++)
			{
				isStartPosition[i] = true;
			}

			// init mapping and free slot pool
			pointerToSlot = new Dictionary<int, int>(MaxTouchCount);
			freeSlots = new Queue<int>(MaxTouchCount);
			for (int i = 0; i < MaxTouchCount; i++)
			{
				freeSlots.Enqueue(i);
			}
		}

		public void Update()
		{
			if (!scratchCard.InputEnabled)
				return;

			// Touch input
			if (Input.touchSupported && Input.touchCount > 0 && !isWebgl)
			{
				// iterate current touches
				foreach (var touch in Input.touches)
				{
					int rawFingerId = touch.fingerId;

					// Began: acquire slot and initialize
					if (touch.phase == TouchPhase.Began)
					{
						int slot = AcquireSlot(rawFingerId);
						if (slot == -1)
						{
							//Debug.LogWarningFormat("[Scratch] No free slot for fingerId={0}. Ignoring touch.", rawFingerId);
							continue;
						}

						// initialize the slot state
						isScratching[slot] = false;
						isStartPosition[slot] = true;
					}

					// Moved / Stationary: use acquired slot and process
					if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
					{
						int slot = AcquireSlot(rawFingerId);
						if (slot == -1)
						{
							// shouldn't happen if Began was processed, but guard anyway
							continue;
						}
						TryScratch(slot, touch.position);
					}

					// Ended / Canceled: release slot
					if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
					{
						int slot;
						if (pointerToSlot.TryGetValue(rawFingerId, out slot))
						{
							isScratching[slot] = false;
						}
						ReleaseSlot(rawFingerId);
					}
				}
			}
			else // Mouse fallback (use slot 0)
			{
				// Defensive: ensure slot 0 exists (it does because MaxTouchCount >= 1)
				if (Input.GetMouseButtonDown(0))
				{
					isScratching[0] = false;
					isStartPosition[0] = true;
				}
				if (Input.GetMouseButton(0))
				{
					TryScratch(0, Input.mousePosition);
				}
				if (Input.GetMouseButtonUp(0))
				{
					isScratching[0] = false;
				}
			}
		}

		/// <summary>
		/// Acquire slot for a fingerId. Returns existing mapping or a new slot.
		/// Returns -1 if no free slot available.
		/// </summary>
		private int AcquireSlot(int fingerId)
		{
			if (pointerToSlot.TryGetValue(fingerId, out int existingSlot))
				return existingSlot;

			if (freeSlots.Count == 0)
				return -1;

			int slot = freeSlots.Dequeue();
			pointerToSlot[fingerId] = slot;
			return slot;
		}

		/// <summary>
		/// Release mapping for fingerId and return slot to pool.
		/// </summary>
		private void ReleaseSlot(int fingerId)
		{
			if (pointerToSlot.TryGetValue(fingerId, out int slot))
			{
				pointerToSlot.Remove(fingerId);

				// reset state for cleanliness
				if (slot >= 0 && slot < MaxTouchCount)
				{
					isScratching[slot] = false;
					isStartPosition[slot] = true;
					eraseStartPositions[slot] = Vector2.zero;
					eraseEndPositions[slot] = Vector2.zero;
				}

				freeSlots.Enqueue(slot);
			}
		}

		private void TryScratch(int slot, Vector2 position)
		{
			// Defensive guard: ensure slot is in-range
			if (slot < 0 || slot >= MaxTouchCount)
			{
				//Debug.LogErrorFormat("[Scratch] TryScratch called with invalid slot={0}. MaxTouchCount={1}", slot, MaxTouchCount);
				return;
			}

			if (OnScratch != null)
			{
				erasePosition = OnScratch(position);
			}

			if (isStartPosition[slot])
			{
				eraseStartPositions[slot] = erasePosition;
				eraseEndPositions[slot] = eraseStartPositions[slot];
				isStartPosition[slot] = !isStartPosition[slot];
			}
			else
			{
				eraseStartPositions[slot] = eraseEndPositions[slot];
				eraseEndPositions[slot] = erasePosition;
			}

			if (!isScratching[slot])
			{
				eraseEndPositions[slot] = eraseStartPositions[slot];
				isScratching[slot] = true;

				// fire scratch start event once per slot when scratching begins
				if (OnScratchStart != null)
				{
					OnScratchStart();
				}
			}
		}

		public void Scratch()
		{
			for (var i = 0; i < isScratching.Length; i++)
			{
				var scratching = isScratching[i];
				if (scratching)
				{
					if (eraseStartPositions[i] == eraseEndPositions[i])
					{
						if (OnScratchHole != null)
						{
							// hole uses last known erasePosition as a reasonable guess
							OnScratchHole(erasePosition);
						}
					}
					else
					{
						if (OnScratchLine != null)
						{
							OnScratchLine(eraseStartPositions[i], eraseEndPositions[i]);
						}
					}
				}
			}
		}
	}
}
