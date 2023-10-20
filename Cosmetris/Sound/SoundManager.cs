/*
 * SoundManager.cs is part of Cosmetris.
 *
 * Copyright (c) 2023 CKProductions, https://ckproductions.dev/
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.IO;
using Cosmetris.Settings;
using Cosmetris.Util;
using Microsoft.Xna.Framework.Audio;

namespace Cosmetris.Sound;

public class SoundManager
{
    private readonly List<Music> _music = new();

    private readonly List<SFX> _sfx = new();

    private SoundEffectInstance _currentMusic;

    public SoundManager()
    {
        LoadSounds("SFX");
        LoadSounds("Music");

        UpdateVolumes();
    }

    private void LoadSounds(string folder)
    {
        // Get how many files are in the star folder
        foreach (var tex in Directory.GetFiles($"{ContentUtil.Instance.RootDirPath}/{folder}"))
        {
            var file = new FileInfo(tex);
            var path = $@"{file.ToString().Replace($"{ContentUtil.Instance.RootDirPath}/", @"").Replace(@".xnb", @"")}";
            var load = Cosmetris.Instance.Content.Load<SoundEffect>(path);

            if (folder.ToLower().Equals("sfx"))
                _sfx.Add(new SFX(load, file.Name.Replace(@".xnb", @""), path));
            else if (folder.ToLower().Equals("music"))
                _music.Add(new Music(load, file.Name.Replace(@".xnb", @""), path));
        }
    }

    public void UpdateVolumes()
    {
        var sfxVolume = GameSettings.Instance.GetValue<float>("Audio", "Sound Effect Volume");
        var musicVolume = GameSettings.Instance.GetValue<float>("Audio", "Music Volume");
        var masterVolume = GameSettings.Instance.GetValue<float>("Audio", "Master Volume");
        var musicEnabled = GameSettings.Instance.GetValue<bool>("Audio", "Music");
        var sfxEnabled = GameSettings.Instance.GetValue<bool>("Audio", "Sound Effects");

        if (sfxEnabled)
            foreach (var sfx in _sfx)
                sfx.Volume = sfxVolume * masterVolume;
        else
            foreach (var sfx in _sfx)
                sfx.Volume = 0;

        foreach (var music in _music)
            if (musicEnabled)
            {
                music.SoundEffectInstance.Volume = musicVolume * masterVolume;
                music.Volume = musicVolume * masterVolume;
            }
            else
            {
                music.SoundEffectInstance.Volume = 0;
                music.Volume = 0;
            }
    }

    public void PlaySFX(string name)
    {
        var sfx = _sfx.Find(s => s.Name.Equals(name));
        sfx.SoundEffect.Play(sfx.Volume, sfx.Pitch, sfx.Pan);
    }

    public void PlayMusic(string name)
    {
        var music = _music.Find(m => m.Name.Equals(name));

        if (_currentMusic != null && music.SoundEffectInstance != _currentMusic)
            if (_currentMusic.State == SoundState.Playing)
                _currentMusic.Stop();

        if (music.SoundEffectInstance == _currentMusic)
            return;

        _currentMusic = music.SoundEffectInstance;

        _currentMusic.IsLooped = true;
        _currentMusic.Play();
    }

    public void PlayMusic(string name, string warnSound)
    {
        var music = _music.Find(m => m.Name.Equals(name));
        var warn = _music.Find(m => m.Name.Equals(warnSound));

        if (_currentMusic != null && warn.SoundEffectInstance != _currentMusic)
            if (_currentMusic.State == SoundState.Playing)
                _currentMusic.Stop();

        _currentMusic = warn.SoundEffectInstance;

        _currentMusic.Play();
        Timer.Instance.CreateTimer((float)warn.SoundEffect.Duration.TotalMilliseconds, (s, o) =>
        {
            if (_currentMusic != null && music.SoundEffectInstance != _currentMusic)
                if (_currentMusic.State == SoundState.Playing)
                    _currentMusic.Stop();

            _currentMusic = music.SoundEffectInstance;

            _currentMusic.IsLooped = true;
            _currentMusic.Play();
        });
    }

    public void PlayMusicSpecial(string startName, string loopName, string warnSound = "")
    {
        var startMusic = _music.Find(m => m.Name.Equals(startName));
        var loopMusic = _music.Find(m => m.Name.Equals(loopName));

        if (warnSound.Length > 0)
        {
            var warn = _music.Find(m => m.Name.Equals(warnSound));

            var warnTime = (float)warn.SoundEffect.Duration.TotalMilliseconds;

            if (_currentMusic != null && warn.SoundEffectInstance != _currentMusic)
                if (_currentMusic.State == SoundState.Playing)
                    _currentMusic.Stop();

            _currentMusic = warn.SoundEffectInstance;

            _currentMusic.Play();
            Timer.Instance.CreateTimer(warnTime, (s, o) => { PlayMusic(startMusic, loopMusic); });
        }
        else
        {
            if (_currentMusic != null && startMusic.SoundEffectInstance != _currentMusic)
                if (_currentMusic.State == SoundState.Playing)
                    _currentMusic.Stop();
            PlayMusic(startMusic, loopMusic);
        }
    }

    private void PlayMusic(Music startMusic, Music loopMusic)
    {
        _currentMusic = startMusic.SoundEffectInstance;

        var length = (float)startMusic.SoundEffect.Duration.TotalMilliseconds;
        _currentMusic.IsLooped = false;
        _currentMusic.Play();

        Timer.Instance.CreateTimer(length, (s, o) =>
        {
            _currentMusic = loopMusic.SoundEffectInstance;
            _currentMusic.IsLooped = true;
            _currentMusic.Play();
        });
    }

    public void StopMusic()
    {
        _currentMusic.Stop();
    }
}