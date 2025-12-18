using System;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{

    public interface IChoMiniInstaller
    {
        List<NodeSource> BuildNodeSources(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options
        );
    }





    // ======================================================
    // String Installer (예문용 단순형)
    // ======================================================
    public sealed class ChoMiniStringInstaller : IChoMiniInstaller
    {
        private string[] _lines;

        // ------------------------------
        // Bind
        // ------------------------------
        public void Bind(string[] lines)
        {
            if (lines == null)
                throw new ArgumentNullException(nameof(lines));

            _lines = lines;
        }

        // ------------------------------
        // BuildNodeSources
        // ------------------------------
        public List<NodeSource> BuildNodeSources(
            ChoMiniLifetimeScope scope,
            ChoMiniOptions options)
        {
            Debug.Log("[Installer] BuildNodeSources: String");

            if (_lines == null)
                throw new InvalidOperationException(
                    "Bind() must be called before BuildNodeSources()"
                );

            List<NodeSource> result = new List<NodeSource>();

            foreach (string line in _lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                // 한 줄 = 한 step
                List<object> items = new List<object>
            {
                line
            };

                result.Add(new NodeSource(items));
            }

            Debug.Log("[Installer] NodeSource Steps = " + result.Count);
            return result;
        }
    }


}