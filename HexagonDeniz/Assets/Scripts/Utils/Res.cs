using System;
using UnityEngine;

namespace HexDeniz
{
    public static class Res
    {
        public static GameObject LoadGameObject(string path)
        {
            var obj = Resources.Load(path) as GameObject;
            if (obj == null)
                throw new NullReferenceException($"Could not locate GameObject at Resources/{path}");

            return obj;
        }
    }
}