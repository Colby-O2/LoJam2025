using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlazmaGames.Core.MonoSystem;

namespace PlazmaGames.Animation
{
	public interface IAnimationMonoSystem : IMonoSystem
	{
		public void RequestAnimation(AnimationRequest request, bool force = false);
		public void RequestAnimation(MonoBehaviour value, float duration, Action<float> animationFunction, Action onComplete = null, bool force = false);
		public void StopAllAnimations(MonoBehaviour value);
		public bool HasAnimationRunning(MonoBehaviour value);
		public bool HasQuededAnimations(MonoBehaviour value);
	}
}
