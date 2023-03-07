using System;
using System.Collections.Generic;

namespace ObjectLiquefier
{
    public static class ClassHierarchyEnumerator
    {
        public static IEnumerable<Type> GetClassHierarchy(this object o) 
            => o.GetType().GetClassHierarchy();    

        public static IEnumerable<Type> GetClassHierarchy(this Type t) {
            for (; t != null; t = t.BaseType)
                yield return t;
            yield break;
        }
    }
}
