using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Yoru.ChoMiniEngine.Examples
{
    public sealed class ChoMiniGameObjectInstaller : IChoMiniInstaller
    {
        private SceneSequence _sequence;

        public void Bind(SceneSequence sequence)
        {
            _sequence = sequence
                ?? throw new ArgumentNullException(nameof(sequence));
        }

        public List<NodeSource> BuildNodeSources(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options)
        {
            var result = new List<NodeSource>();

            foreach (var g in _sequence.Groups)
            {
                if (g.Items.Count == 0) continue;
                result.Add(new NodeSource(g.Items, g.Tags));
            }

            return result;
        }
    }
}
