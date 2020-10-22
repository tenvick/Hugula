﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Hugula.Utils;

namespace Hugula.Audio
{


    /// <summary>
    /// AudioSource Prefab
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourcePrefab : MonoBehaviour
    {
        // [Flags]
        public enum AudioSourceType
        {
            ///<summary>
            /// 音乐节点
            ///</summary>
            Music,
            ///<summary>
            /// 音效节点
            ///</summary>
            Effect,
            // ///<summary>
            // /// 自定义节点
            // ///</summary>
            // Manual

        }

        [Tooltip("audio source")]
        [SerializeField]
        private AudioSource m_AudioSource;

        public AudioSource audioSource
        {
            get
            {
                return m_AudioSource;
            }
        }
        private float m_Volume;
        private float m_EaseTime;

        public AudioSourceType type = AudioSourceType.Effect;

        [Range(0, 10)]
        public float easeTime = 1f;

        bool m_IsStop = false;
        void Awake()
        {
            if (m_AudioSource == null) m_AudioSource = GetComponent<AudioSource>();
            if (type == AudioSourceType.Music)
                AudioManager.AddMusicAudioSource(this);
            else
                AudioManager.AddSoundAudioSource(this);
        }

        void OnDestroy()
        {
            if (type == AudioSourceType.Music)
                AudioManager.RemoveMusicAudioSource(this);
            else
                AudioManager.RemoveSoundAudioSource(this);

        }

        void OnAudioClipComp(AudioClip data, object userData)
        {
            AudioClip clip = data;//(AudioClip)data;

            if (clip)
            {
                audioSource.clip = clip;
                Play(m_Volume, easeTime);
            }
        }

        public void Stop(float easeTime = -1)
        {
            if (float.Equals(easeTime, -1f))
                easeTime = this.easeTime;

            if (audioSource.clip == null)
                audioSource.Stop();
            else
            {
                VolumeTo(0, easeTime);
            }
            m_IsStop = true;
        }

        public void VolumeTo(float value, float easeTime = -1)
        {
            if(m_IsStop) return;

            if (float.Equals(easeTime, -1f))
                easeTime = this.easeTime;

            LeanTween.value(this.gameObject, (v) => { audioSource.volume = v; }, 0, value, easeTime);
        }

        public void Play(float volume)
        {
            Play(volume, this.easeTime);
        }

        public void Play(float volume, float easeTime)
        {
            audioSource.volume = 0;
            audioSource.Play();
            m_IsStop = false;
            VolumeTo(volume, easeTime);
        }

        public void PlayAsync(string clipname, float volume, float easeTime = -1)
        {
            m_IsStop = false;
            // var abName = clipname.ToLower() + Common.CHECK_ASSETBUNDLE_SUFFIX;
            m_Volume = volume;
            m_EaseTime = easeTime;
            ResLoader.LoadAssetAsync<AudioClip>(clipname,OnAudioClipComp,null);
            // ResLoader.LoadAssetAsync(abName, clipname, typeof(AudioClip), OnAudioClipComp, null);
        }

        public void PlayOnShot(AudioClip clip, float volumeScale)
        {
            m_IsStop = false;
            audioSource?.PlayOneShot(clip, volumeScale);
        }

        public void PlayScheduled(double time)
        {
            m_IsStop = false;
            audioSource?.PlayScheduled(time);
        }
    }
}
