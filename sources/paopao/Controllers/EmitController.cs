using System;
using UnityEngine;

[RequireComponent(typeof(ParticleEmitter))]
[RequireComponent(typeof(ParticleRenderer))]
[RequireComponent(typeof(ParticleAnimator))]
public class EmitController : MonoBehaviour
{
	private void Start()
	{
		this.particleAnimator_ = base.GetComponent<ParticleAnimator>();
	}

	private void ModifyStartPosition()
	{
		Particle[] particles = base.particleEmitter.particles;
		for (int i = 0; i < particles.Length; i++)
		{
			if (particles[i].energy >= base.particleEmitter.minEnergy)
			{
				Vector3 position = particles[i].position;
				position.z = 0f;
				position.Normalize();
				Vector3 vector = position * this.firstPosRange;
				particles[i].position = vector;
				particles[i].velocity = position * this.speed;
			}
		}
		base.particleEmitter.particles = particles;
	}

	public void EmitStart()
	{
		if (this.isEmit_)
		{
			return;
		}
		this.isEmit_ = true;
		this.particleAnimator_.sizeGrow = this.scaleFactor - 1f;
		base.particleEmitter.minSize = this.particleSize;
		base.particleEmitter.maxSize = this.particleSize;
		base.particleEmitter.minEnergy = this.lifeCycle;
		base.particleEmitter.maxEnergy = this.lifeCycle;
		base.particleEmitter.useWorldSpace = false;
		base.particleEmitter.Emit(this.startParticleNum);
		this.ModifyStartPosition();
		this.emitStartTime_ = Time.time;
		this.lastEmitTime_ = Time.time;
	}

	private void FixedUpdate()
	{
		if (!this.isEmit_)
		{
			return;
		}
		if (Time.time - this.emitStartTime_ > this.animLength)
		{
			this.isEmit_ = false;
			return;
		}
		if (Time.time - this.lastEmitTime_ >= this.generateCycle)
		{
			base.particleEmitter.Emit(1);
			this.ModifyStartPosition();
			this.lastEmitTime_ = Time.time;
		}
	}

	private bool isEmit_;

	public float lifeCycle;

	public float generateCycle;

	public float speed;

	public float scaleFactor;

	public float particleSize;

	public float animLength;

	public int startParticleNum = 1;

	public float firstPosRange = 1f;

	private ParticleAnimator particleAnimator_;

	private float emitStartTime_;

	private float lastEmitTime_;
}
