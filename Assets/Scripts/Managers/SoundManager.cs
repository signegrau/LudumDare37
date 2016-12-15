using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager single;
	public AudioClip advanceButtonPushSound;
	public AudioClip springBoardSound;
	public AudioClip boostSound;
	public AudioClip landSound;
	public AudioClip laserSound;
	public AudioClip jumpSound;
	public AudioClip[] splatSounds;
	public AudioClip[] footStepSounds;
	public AudioSource _source;
	private AudioSource advanceButtonPushSoundSource, springBoardSoundSource, boostSoundSource, landSoundSource, jumpSoundSource, laserSoundSource, splatSoundsSource, footStepSoundsSource;
	private int avoidFootstepIndex;
	private int avoidSplatIndex;

    private void Awake()
    {
		if (single == null) {
			single = this;
			_source = GetComponent<AudioSource>();

			advanceButtonPushSoundSource = gameObject.AddComponent<AudioSource>();
			advanceButtonPushSoundSource.clip = advanceButtonPushSound;

			springBoardSoundSource = gameObject.AddComponent<AudioSource>();
			springBoardSoundSource.clip = springBoardSound;
		
			boostSoundSource = gameObject.AddComponent<AudioSource>();
			boostSoundSource.clip = boostSound;
		
			landSoundSource = gameObject.AddComponent<AudioSource>();
			landSoundSource.clip = landSound;
		
			jumpSoundSource = gameObject.AddComponent<AudioSource>();
			jumpSoundSource.clip = jumpSound;

			laserSoundSource = gameObject.AddComponent<AudioSource>();
			laserSoundSource.clip = laserSound;

			splatSoundsSource = gameObject.AddComponent<AudioSource>();
			footStepSoundsSource = gameObject.AddComponent<AudioSource>();
		}
    }

	private int PlayRandomSound(AudioSource src, AudioClip[] clips, int avoidIndex=-1) {
		int index;
		do {
			index = Random.Range(0, clips.Length -1);
		} while (index == avoidIndex);
		src.clip = clips[index];
		src.Play();
		return index;
	}

	public void PlayAdvanceSound() { advanceButtonPushSoundSource.Play();}
	public void PlaySpringBoardSound() { springBoardSoundSource.Play(); }
	public void PlayBoostSound() { boostSoundSource.Play(); }
	public void PlayJumpSound() { jumpSoundSource.Play(); }
	public void PlayLandSound() { landSoundSource.Play(); }
	public void PlayLaserSound() { laserSoundSource.Play(); }
	public void PlayFootstepSound() {
		avoidFootstepIndex = PlayRandomSound(footStepSoundsSource, footStepSounds, avoidFootstepIndex);
	}

	public void PlaySplatSound() {
		avoidSplatIndex = PlayRandomSound(splatSoundsSource, splatSounds, avoidSplatIndex);
	}

}
