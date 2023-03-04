using Fluid;
using System;
using System.Collections.Generic;

namespace ObjectLiquefier
{
    public class TemplateCache {
        // a simple cache backed up by a dictionary will suffice for now
        private readonly Dictionary<string, IFluidTemplate> cache = new();

        public void Drop() => cache.Clear();

        public IFluidTemplate? this[string i] {
            get => cache.TryGetValue(i, out var v) ? v : null;
            set => cache[i] = value!;
        }
    }
}
