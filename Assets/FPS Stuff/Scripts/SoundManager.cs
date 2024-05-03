using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Weapon;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    public AudioSource ShootingChannel;

    // 1911
    public AudioClip m1911Shot;

    public AudioSource reloadSound1911;
    public AudioSource emptyMagazineSound1911;

    // AK47
    public AudioClip AK74Shot;

    public AudioSource reloadSoundAK74;

    // BM4

    public AudioClip BM4Shot;

    public AudioSource reloadSoundBM4;

    // Uzi

    public AudioClip UziShot;

    public AudioSource reloadSoundUzi;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayShootingSound(WeaponModel weapon)
    {
        // This method plays sounds simultaneously. Since each bullet shot plays a sound, it would sound bad if the sounds cut eachother off. Pay attention to the method used.
        
        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                ShootingChannel.PlayOneShot(m1911Shot);
                break;

            case WeaponModel.AK47:
                ShootingChannel.PlayOneShot(AK74Shot);
                break;

            case WeaponModel.BM4:
                ShootingChannel.PlayOneShot(BM4Shot);
                break;

            case WeaponModel.Uzi:
                ShootingChannel.PlayOneShot(UziShot);
                break;
        }
    }

    public void PlayReloadSound(WeaponModel weapon)
    {
        // This method plays sounds just once. If you try to play a sound right after, it will cut off the previously played sound. Pay attention to the method used.

        switch (weapon)
        {
            case WeaponModel.Pistol1911:
                reloadSound1911.Play();
                break;

            case WeaponModel.AK47:
                reloadSoundAK74.Play();
                break;

            case WeaponModel.BM4:
                reloadSoundBM4.Play();
                break;

            case WeaponModel.Uzi:
                reloadSoundUzi.Play();
                break;
        }
    }
}
