/*
 * ParticleManager.cs is part of Cosmetris.
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cosmetris.Render.Particle;

public class ParticleManager
{
    private readonly int _maxParticles = 500;

    private readonly Particle[] _particles;
    private int _particleCount;

    public ParticleManager()
    {
        _particles = new Particle[_maxParticles];
    }

    public static ParticleManager Instance { get; } = new();

    public void AddParticle(Particle particle)
    {
        if (_particleCount >= _maxParticles)
        {
            // Remove the oldest particle
            _particles[0] = null;
            for (var i = 1; i < _maxParticles; i++) _particles[i - 1] = _particles[i];
            _particleCount--;
        }

        _particles[_particleCount] = particle;
        _particleCount++;
    }

    public void AddParticles(List<Particle> particles)
    {
        foreach (var particle in particles) AddParticle(particle);
    }

    public void RemoveParticle(Particle particle)
    {
        for (var i = 0; i < _particleCount; i++)
            if (_particles[i] == particle)
            {
                _particles[i] = null;
                for (var j = i + 1; j < _particleCount; j++) _particles[j - 1] = _particles[j];
                _particleCount--;
                break;
            }
    }

    public void Update(GameTime gameTime)
    {
        for (var i = 0; i < _particleCount; i++)
        {
            var particle = _particles[i];
            particle.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        for (var i = 0; i < _particleCount; i++)
        {
            var particle = _particles[i];
            particle.Draw(spriteBatch, gameTime);
        }
    }

    public void ClearParticles()
    {
        _particleCount = 0;
    }
}