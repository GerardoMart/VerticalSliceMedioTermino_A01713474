using UnityEngine;

public enum Sfx { Warning, Slam, PlayerHit, BossHit, Block, GuardBreak, Victory, Defeat, Select }

// Música de fondo + efectos. Todos los clips son opcionales: si un campo
// está vacío, simplemente no suena (no rompe el juego).
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips (opcionales)")]
    [SerializeField] private AudioClip warning;
    [SerializeField] private AudioClip slam;
    [SerializeField] private AudioClip playerHit;
    [SerializeField] private AudioClip bossHit;
    [SerializeField] private AudioClip block;
    [SerializeField] private AudioClip guardBreak;
    [SerializeField] private AudioClip victory;
    [SerializeField] private AudioClip defeat;
    [SerializeField] private AudioClip select;

    public void Play(Sfx sfx)
    {
        AudioClip clip = null;
        switch (sfx)
        {
            case Sfx.Warning:    clip = warning;    break;
            case Sfx.Slam:       clip = slam;       break;
            case Sfx.PlayerHit:  clip = playerHit;  break;
            case Sfx.BossHit:    clip = bossHit;    break;
            case Sfx.Block:      clip = block;      break;
            case Sfx.GuardBreak: clip = guardBreak; break;
            case Sfx.Victory:    clip = victory;    break;
            case Sfx.Defeat:     clip = defeat;     break;
            case Sfx.Select:     clip = select;     break;
        }
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}
