using UnityEngine;
using UnityCommon;
using System;

namespace UnityCommon
{
    public static class TypeCache<T>
    {
        public static readonly Type Type;

        static TypeCache()
        {
            Type = typeof(T);
        }
    }

    public static class TypeCache<TSrc, TDst>
    {
        public static readonly bool IsAssignableFrom;

        static TypeCache()
        {
            IsAssignableFrom = TypeCache<TSrc>.Type.IsAssignableFrom(TypeCache<TDst>.Type);
        }
    }
}