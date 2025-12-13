using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yoru.ChoMiniEngine
{
    internal sealed class ChoMiniInstallerRule
    {
        public Type InstallerType;
        public readonly List<(Func<bool> Condition, string Key)> Entries
            = new List<(Func<bool>, string)>();

        public void Add(Func<bool> Condition, string Key)
        {
            Entries.Add((Condition, Key));
        }
        
    }

    public sealed class ChoMiniInstallerRuleBuilder
    {
        private readonly ChoMiniInstallerRule _rule;
        private Func<bool> _pendingCondition;

        internal ChoMiniInstallerRuleBuilder(ChoMiniInstallerRule rule)
        {
            _rule = rule;
        }

        public ChoMiniInstallerRuleBuilder When(Func<bool> condition)
        {
            _pendingCondition = condition;
            return this;
        }

        public ChoMiniInstallerRuleBuilder Select(string key)
        {
            if (_pendingCondition == null)
                throw new Exception("Select() called without When()");

            if (string.IsNullOrEmpty(key))
                throw new Exception("Installer key cannot be null or empty");

            _rule.Entries.Add((_pendingCondition, key));
            _pendingCondition = null;
            return this;
        }
    }
}
