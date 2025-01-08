using UnityEngine;
using UniRx;
using System;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class SoundEffectHandler : MonoBehaviour
{
    [SerializeField] private AudioClip[] shotSounds, damageSounds, doorSounds;
    [SerializeField] private AudioClip hitSound, refrectSound, explosionSound, charShowSound, healHPSound, dashSound, jumpSound, landSound, passTheGreen, completedProcessSound;
    [SerializeField] private AudioClip[] bgm;

    [SerializeField] AudioSource audioSourceSE, audioSourceBGM;
    [SerializeField] GameObject audioSourceSEObj, audioSourceBGMObj;

    private BGMCtrl bgmCtrl;
    private SoundEffectCtrl soundEffectCtrl;
    private PlayerAcitonSECtrl playerAcitonSECtrl;
    private UISoundCtrl uiSoundCtrl;
    private DoorSoundCtrl doorSoundCtrl;

    private void Start()
    {
        /*Debug.LogWarning("SoundEffectHandler Start");
        if (audioSourceSE == null)
        {
            Debug.Log("audioSourceSE is null");
            audioSourceSE = audioSourceSEObj.GetComponent<AudioSource>();
        }
        if (audioSourceBGM == null)
        {
            Debug.Log("audioSourceBGM is null");
            audioSourceBGM = audioSourceBGMObj.GetComponent<AudioSource>();
        }*/
        bgmCtrl = new BGMCtrl(audioSourceBGM, bgm);
        soundEffectCtrl = new SoundEffectCtrl(audioSourceSE, shotSounds, damageSounds, hitSound, refrectSound, explosionSound, charShowSound, healHPSound);
        playerAcitonSECtrl = new PlayerAcitonSECtrl(audioSourceSE, dashSound, jumpSound, landSound);
        uiSoundCtrl = new UISoundCtrl(audioSourceSE, passTheGreen, completedProcessSound);
        doorSoundCtrl = new DoorSoundCtrl(audioSourceSE, doorSounds);
    }

    private void OnDestroy()
    {
        soundEffectCtrl.Dispose();
        bgmCtrl.Dispose();
        playerAcitonSECtrl.Dispose();
        doorSoundCtrl.Dispose();
        uiSoundCtrl.Dispose();

        bgmCtrl = null;
        soundEffectCtrl = null;
        playerAcitonSECtrl = null;
        doorSoundCtrl = null;
        uiSoundCtrl = null;
    }

    void PlayShotMame()
    {
        audioSourceSE.PlayOneShot(shotSounds[0]);
    }

    void PlayLowCharge()
    {
        audioSourceSE.PlayOneShot(shotSounds[1]);
    }

    void PlayCharge()
    {
        audioSourceSE.PlayOneShot(shotSounds[2]);
    }
}

public class BGMCtrl
{
    private AudioSource audioSourceBGM;
    private AudioClip[] bgm;

    private readonly CompositeDisposable _disposables;
    public static Subject<int> onPlayBGM = new Subject<int>();
    public static Subject<Unit> onStopBGM = new Subject<Unit>();

    //public static Subject<Unit> onFadeOut = new Subject<Unit>();
    //public static Subject<Unit> onFadeInAndPlayBGM = new Subject<Unit>();

    public BGMCtrl(AudioSource audioSourceBGM, AudioClip[] bgm)
    {
        _disposables = new CompositeDisposable();

        this.audioSourceBGM = audioSourceBGM;
        this.bgm = bgm;
        audioSourceBGM.loop = true;
        audioSourceBGM.volume = 0f;

        onPlayBGM.Subscribe(async index =>
        {
            audioSourceBGM.clip = bgm[index];
            audioSourceBGM.Play();
            //Debug.Log("BGMを再生");
            await audioSourceBGM.DOFade(endValue: 0.4f, duration: 2.0f).ToUniTask();
            //Debug.Log("BGMのフェードインが完了");
        }).AddTo(_disposables);

        onStopBGM.Subscribe(async _ =>
        {
            Debug.Log("BGMフェードアウト開始");
            await audioSourceBGM.DOFade(endValue: 0, duration: 1);
            Debug.Log("BGMフェードアウト完了");
            audioSourceBGM.Stop();
        }).AddTo(_disposables);

        /*onFadeOut.Subscribe(_ =>
        {
            audioSourceBGM.DOFade(endValue: 0, duration: 1);
        }).AddTo(_disposables);

        onFadeInAndPlayBGM.Subscribe(_ =>
        {
            PlayBGM(1);

            audioSourceBGM.DOFade(endValue: 1, duration: 0.5f);
        }).AddTo(_disposables);*/
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

public class SoundEffectCtrl
{
    private AudioSource audioSourceSE;
    private AudioClip[] shotSound, damageSound;
    private AudioClip hitSound, refrectSound, explosionSound, charShowSound, healHPSound;

    private static Subject<int> onPlayShotSE = new Subject<int>();
    public static IObserver<int> OnPlayShotSE => onPlayShotSE;

    private static Subject<int> onPlayDamageSE = new Subject<int>();
    public static IObserver<int> OnPlayDamageSE => onPlayDamageSE;

    private static Subject<int> onPlayDeathSE = new Subject<int>();
    public static IObserver<int> OnPlayDeathSE => onPlayDeathSE;

    private static Subject<int> onPlayHitSE = new Subject<int>();
    public static IObserver<int> OnPlayHitSE => onPlayHitSE;

    public static Subject<Unit> onPlayExplosionSE = new Subject<Unit>();
    public static IObserver<Unit> OnPlayExplosionSE => onPlayExplosionSE;

    public static Subject<Unit> onPlayCharShowSE = new Subject<Unit>();
    public static IObserver<Unit> OnPlayCharShowSE => onPlayCharShowSE;

    public static Subject<Unit> onPlayHealHPSound = new Subject<Unit>();
    public static IObserver<Unit> OnPlayHealHPSound => onPlayHealHPSound;

    private readonly CompositeDisposable _disposables;

    public SoundEffectCtrl(AudioSource audioSourceSE, AudioClip[] soundEffects, AudioClip[] damageSound, AudioClip hitSound, AudioClip refrectSound, AudioClip explosionSound, AudioClip charShowSound, AudioClip healHPSound)
    {
        _disposables = new CompositeDisposable();

        this.healHPSound = healHPSound;
        this.charShowSound = charShowSound;
        this.explosionSound = explosionSound;
        this.hitSound = hitSound;
        this.damageSound = damageSound;
        this.audioSourceSE = audioSourceSE;
        this.shotSound = soundEffects;
        this.refrectSound = refrectSound;

        onPlayShotSE.Subscribe(index =>
        {
            PlayShotEffect(index);
        })
        .AddTo(_disposables);

        onPlayDamageSE.Subscribe(index =>
        {
            PlayDamageSound(index);
        })
        .AddTo(_disposables);

        onPlayDeathSE.Subscribe(index =>
        {
            PlayDeathSound(index);
        })
        .AddTo(_disposables);

        onPlayHitSE.Subscribe(num =>
        {
            if (num == 0)
                PlayShellHitSound();
            else
                PlayShellRefrectSound();
        })
        .AddTo(_disposables);

        onPlayExplosionSE.Subscribe(_ =>
        {
            PlayExplosionSound();
        })
        .AddTo(_disposables);

        onPlayCharShowSE.Subscribe(_ =>
        {
            PlayCharShowSound();
        })
        .AddTo(_disposables);

        onPlayHealHPSound.Subscribe(_ =>
        {
            PlayHealHPSound();
        })
        .AddTo(_disposables);
    }

    private void PlayShotEffect(int soundEffectIndex)
    {
        audioSourceSE.PlayOneShot(shotSound[soundEffectIndex], 0.5f);
    }

    private void PlayDamageSound(int soundEffectIndex)
    {
        audioSourceSE.PlayOneShot(damageSound[soundEffectIndex], 0.7f);
    }

    private void PlayDeathSound(int soundEffectIndex)
    {
        audioSourceSE.PlayOneShot(damageSound[soundEffectIndex]);
    }

    private void PlayShellHitSound()
    {
        audioSourceSE.PlayOneShot(hitSound);
    }

    private void PlayShellRefrectSound()
    {
        audioSourceSE.PlayOneShot(refrectSound);
    }

    private void PlayExplosionSound()
    {
        audioSourceSE.PlayOneShot(explosionSound, 0.5f);
    }

    private void PlayCharShowSound()
    {
        audioSourceSE.PlayOneShot(charShowSound);
    }

    private void PlayHealHPSound()
    {
        audioSourceSE.PlayOneShot(healHPSound);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

public class PlayerAcitonSECtrl
{
    private static Subject<AudioClip> onPlaySE = new Subject<AudioClip>();
    public static IObserver<AudioClip> OnPlaySE => onPlaySE;

    private readonly CompositeDisposable _disposables;

    private AudioSource audioSource;
    public static AudioClip dashSound, jumpSound, landSound;
    public PlayerAcitonSECtrl(AudioSource audioSource, AudioClip dashSound, AudioClip jumpSound, AudioClip landSound)
    {
        _disposables = new CompositeDisposable();

        this.audioSource = audioSource;
        PlayerAcitonSECtrl.dashSound = dashSound;
        PlayerAcitonSECtrl.jumpSound = jumpSound;
        PlayerAcitonSECtrl.landSound = landSound;

        onPlaySE.Subscribe(sound =>
        {
            PlaySound(sound);
        })
        .AddTo(_disposables);
    }

    private void PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

public class UISoundCtrl
{
    private readonly CompositeDisposable _disposables;
    public static Subject<Unit> onPlayPassTheGreenSE = new Subject<Unit>();
    public static Subject<Unit> onCompletedProcessSE = new Subject<Unit>();

    AudioSource audioSourceSE;
    public UISoundCtrl(AudioSource audioSourceSE, AudioClip passTheGreen, AudioClip completedProcessSound)
    {
        _disposables = new CompositeDisposable();
        this.audioSourceSE = audioSourceSE;

        onPlayPassTheGreenSE.Subscribe(_ =>
        {
            PlaySound(passTheGreen);
        })
        .AddTo(_disposables);

        onCompletedProcessSE.Subscribe(_ =>
        {
            PlaySound(completedProcessSound);
        })
        .AddTo(_disposables);
    }

    private void PlaySound(AudioClip sound)
    {
        audioSourceSE.PlayOneShot(sound);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}

public class DoorSoundCtrl
{
    public static Subject<Unit> onPlayDoorOpenSE = new Subject<Unit>();
    public static Subject<Unit> onPlayDoorCloseSE = new Subject<Unit>();

    private readonly CompositeDisposable _disposables;

    private AudioSource audioSourceSE;
    private AudioClip[] doorSounds;
    public DoorSoundCtrl(AudioSource audioSourceSE, AudioClip[] doorSounds)
    {
        _disposables = new CompositeDisposable();
        this.audioSourceSE = audioSourceSE;
        this.doorSounds = doorSounds;

        onPlayDoorOpenSE.Subscribe(_ =>
        {
            PlaySound(0);
        }).AddTo(_disposables);

        onPlayDoorCloseSE.Subscribe(_ =>
        {
            PlaySound(1);
        }).AddTo(_disposables);
    }

    private void PlaySound(int index)
    {
        Debug.Log("ドアの音を再生");
        audioSourceSE.PlayOneShot(doorSounds[index]);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
