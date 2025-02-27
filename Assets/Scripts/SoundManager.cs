using UnityEngine;



public enum SoundType
{
    Fireball,
    Charge,
    Run,
    Jump
}
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList; 
    public static SoundManager instance;
    private AudioSource audioSource;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public static void PlaySound(SoundType sound,float volume = 1)
    {
        instance.audioSource.PlayOneShot(instance.soundList[(int)sound],volume);
    }
}
