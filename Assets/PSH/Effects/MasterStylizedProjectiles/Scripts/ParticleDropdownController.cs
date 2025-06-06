using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MasterStylizedProjectile
{
    public class ParticleDropdownController : MonoBehaviour
    {
        public BulletShooter shooter;
        public Dropdown dropdown;
        public AudioClip changeClip;

        new AudioSource audio;
        private void Start()
        {
            dropdown = GetComponent<Dropdown>();
            RefreshDropdown();
            dropdown.onValueChanged.AddListener(OnSelect);

            audio = gameObject.AddComponent<AudioSource>();
            if (changeClip != null)
            {
                audio.clip = changeClip;
            }

        }
        private void Update()
        {
            if(Keyboard.current.aKey.wasPressedThisFrame)
            {
                dropdown.value -= 1;
                OnSelect(dropdown.value);
                //RefreshDropdown();
                if (changeClip != null)
                {
                    audio.Play();
                }
            }
            if(Keyboard.current.dKey.wasPressedThisFrame)
            {
                dropdown.value += 1;
                OnSelect(dropdown.value);
                //RefreshDropdown();
                if (changeClip != null)
                {
                    audio.Play();
                }
            }
        }
        public void RefreshDropdown()
        {
            dropdown.ClearOptions();
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            for (int i = 0; i < shooter.datas.Effects.Count; i++)
            {
                options.Add(new Dropdown.OptionData( shooter.datas.Effects[i].EffectName));
            }
            dropdown.AddOptions(options);
        }
        public void OnSelect(int index)
        {
            shooter.Index = index;
        }
    }
}
