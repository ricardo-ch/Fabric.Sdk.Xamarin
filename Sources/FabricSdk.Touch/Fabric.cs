﻿using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;

namespace FabricSdk
{
    public sealed class Fabric : IFabric
    {
        private static readonly Lazy<Fabric> LazyInstance = new Lazy<Fabric>(() => new Fabric());

        public static IFabric Instance => LazyInstance.Value;

        public ICollection<IKit> Kits { get; } = new List<IKit>();

        public event EventHandler BeforeInitialize;
        public event EventHandler AfterInitialize;

        private Fabric()
        {
        }

        public bool Debug
        {
            get { return Bindings.FabricSdk.Fabric.SharedSDK().Debug; }
            set { Bindings.FabricSdk.Fabric.SharedSDK().Debug = value; }
        }

        internal void Initialize()
        {
            BeforeInitialize?.Invoke(this, new EventArgs());
            var nativeKits = new List<NSObject>();
            foreach (var kit in Kits)
                nativeKits.Add(kit.ToNative());
            Bindings.FabricSdk.Fabric.With(nativeKits.ToArray());
            AfterInitialize?.Invoke(this, new EventArgs());
        }
    }

    public static class Initializer
    {
        private static readonly object InitializeLock = new object();
        private static bool _initialized;

        public static void Initialize(this IFabric fabric)
        {
            if (_initialized) return;
            lock (InitializeLock)
            {
                if (_initialized) return;

                var instance = fabric as Fabric;

                if (instance == null) return;

                instance.Initialize();

                _initialized = true;
            }
        }
    }
}