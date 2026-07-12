using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioSource walkAudioSource;
    public AudioSource sfxAudioSource;

    public AudioClip walk;
    public AudioClip jump;
    public AudioClip land;
    public AudioClip attack;
    public AudioClip hurt;
    public AudioClip death;
    public AudioClip dash;
    public AudioClip pickup;
    public AudioClip drop;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (walkAudioSource == null)
            walkAudioSource = audioSource;

        if (sfxAudioSource == null)
            sfxAudioSource = CreateSfxAudioSource();

        AssignFallbackClips();
    }

    private void AssignFallbackClips()
    {
        if (dash == null)
            dash = jump;

        if (attack == null)
            attack = land;

        if (hurt == null)
            hurt = land;

        if (death == null)
            death = hurt != null ? hurt : land;

        if (pickup == null)
            pickup = jump;

        if (drop == null)
            drop = pickup;
    }

    private void PlayClip(AudioClip clip)
    {
        if (sfxAudioSource == null || clip == null)
            return;

        sfxAudioSource.PlayOneShot(clip);
    }

    private AudioSource CreateSfxAudioSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();

        if (audioSource != null)
        {
            source.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
            source.volume = audioSource.volume;
            source.pitch = audioSource.pitch;
            source.priority = audioSource.priority;
            source.spatialBlend = audioSource.spatialBlend;
            source.minDistance = audioSource.minDistance;
            source.maxDistance = audioSource.maxDistance;
            source.rolloffMode = audioSource.rolloffMode;
        }

        source.playOnAwake = false;
        source.loop = false;
        return source;
    }

    public void PlayWalk()
    {
        if (walkAudioSource == null || walk == null)
            return;

        if (walkAudioSource.isPlaying && walkAudioSource.clip == walk)
            return;

        walkAudioSource.clip = walk;
        walkAudioSource.loop = true;
        walkAudioSource.Play();
    }

    public void StopWalk()
    {
        if (walkAudioSource == null || walkAudioSource.clip != walk)
            return;

        walkAudioSource.Stop();
        walkAudioSource.clip = null;
        walkAudioSource.loop = false;
    }

    public void PlayJump()
    {
        PlayClip(jump);
    }

    public void PlayLand()
    {
        PlayClip(land);
    }

    public void PlayAttack()
    {
        PlayClip(attack);
    }

    public void PlayHurt()
    {
        PlayClip(hurt);
    }

    public void PlayDeath()
    {
        PlayClip(death);
    }

    public void PlayDash()
    {
        PlayClip(dash);
    }

    public void PlayPickup()
    {
        PlayClip(pickup);
    }

    public void PlayDrop()
    {
        PlayClip(drop);
    }
}
