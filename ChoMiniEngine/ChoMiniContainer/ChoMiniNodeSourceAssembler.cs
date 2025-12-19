using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yoru.ChoMiniEngine
{
    public sealed class ChoMiniNodeSourceAssembler
    {
        private readonly IReadOnlyList<BootRule> _installerRules;
        private readonly ChoMiniOptions _options;
        private readonly Dictionary<(Type installerType, object? key), object> _bindings;

        public ChoMiniNodeSourceAssembler(
            IReadOnlyList<BootRule> installerRules,
            ChoMiniOptions options,
            Dictionary<(Type, object?), object> bindings)
        {
            _installerRules = installerRules;
            _options = options;
            _bindings = bindings;
        }

        public List<NodeSource> Assemble()
        {
            var allSequences = new List<List<NodeSource>>();
            var installerTypes = new HashSet<Type>();

            foreach (var kv in _bindings)
                installerTypes.Add(kv.Key.installerType);

            foreach (var installerType in installerTypes)
            {
                var seq = BuildInstallerSequence(installerType);
                if (seq != null && seq.Count > 0)
                    allSequences.Add(seq);
            }

            return MergeByStep(allSequences);
        }

        // --------------------------------------------------
        // Single installer
        // --------------------------------------------------
        private List<NodeSource> BuildInstallerSequence(Type installerType)
        {
            object key = null;

            foreach (var pair in _options.DebugPairs())
            {
                if (_bindings.ContainsKey((installerType, pair.Value)))
                {
                    key = pair.Value;
                    break;
                }
            }

            object resource;
            try
            {
                resource = Resolve(installerType, key);
            }
            catch
            {
                return null;
            }

            var installer =
                (IChoMiniInstaller)Activator.CreateInstance(installerType);

            installerType.GetMethod("Bind")
                         .Invoke(installer, new[] { resource });

            return installer.BuildNodeSources(null, _options);
        }

        private object Resolve(Type installerType, object key)
        {
            if (_bindings.TryGetValue((installerType, key), out var obj))
                return obj;

            if (_bindings.TryGetValue((installerType, null), out obj))
                return obj;

            throw new KeyNotFoundException();
        }

        // --------------------------------------------------
        // Step merge
        // --------------------------------------------------
        private List<NodeSource> MergeByStep(List<List<NodeSource>> sequences)
        {
            var result = new List<NodeSource>();
            int maxSteps = 0;

            foreach (var seq in sequences)
                maxSteps = Math.Max(maxSteps, seq.Count);

            for (int i = 0; i < maxSteps; i++)
            {
                var items = new List<object>();
                var tags = new HashSet<string>();

                foreach (var seq in sequences)
                {
                    if (i >= seq.Count) continue;

                    var src = seq[i];
                    items.AddRange(src.Items);
                    foreach (var tag in src.Tags)
                        tags.Add(tag);
                }

                if (items.Count > 0)
                    result.Add(new NodeSource(items, tags));
            }

            return result;
        }
    }
}
