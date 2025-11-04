using System;
using UnityEngine;

namespace PlazmaGames.Animation
{
	public class AnimationRequest
	{
		public MonoBehaviour value;
		public float duration;
		public Action<float> animationFunction;
		public Action onComplete;

		public bool hasStarted;
		public bool hasCompleted;
		public Coroutine coroutine;

		public AnimationRequest(MonoBehaviour value, float duration, Action<float> animationFunction, Action onComplete = null)
		{
			this.value = value;
			this.duration = duration;
			this.animationFunction = animationFunction;
			this.onComplete = onComplete;
			this.hasStarted = false;
			this.hasCompleted = false;
			this.coroutine = null;
		}
	}
}
