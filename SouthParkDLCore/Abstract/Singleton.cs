using System;

namespace SouthParkDLCore.Abstract
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static readonly Lazy<T> lazy = new Lazy<T>(() => new T());
        public static T Instance
        {
            get
            {
                return lazy.Value;
            }
        }
    }
}