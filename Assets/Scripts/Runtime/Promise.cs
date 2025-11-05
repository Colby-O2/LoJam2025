using UnityEngine;

namespace LJ2025
{
    public class Promise<T>
    {
        private T _value;
        private bool _isResolved = false;
        private System.Action<T> _then = null;

        public bool IsResolved() => _isResolved;

        public void Resolve(T value = default)
        {
            _value = value;
            _isResolved = true;
            
            _then?.Invoke(_value);
        }

        public Promise Then(System.Action<T> func)
        {
            Promise p = new Promise();
            _then = (T) =>
            {
                func(T);
                p.Resolve();
            };
            return p;
        }
        
        public Promise Then(System.Func<T, bool> func)
        {
            Promise p = new Promise();
            _then = (T) =>
            {
                if (func(T) == Promise.Continue) p.Resolve();
            };
            return p;
        }
        
        public Promise<U> Then<U>(System.Func<T, Promise<U>> func)
        {
            var chained = new Promise<U>();
            _then = (tValue) =>
            {
                Promise<U> p = func(tValue);
                p?.Then((uValue) =>
                {
                    chained.Resolve(uValue);
                });
            };
            
            return chained;
        }
    }

    public class Promise : Promise<int>
    {
        public static readonly bool Continue = true;
        public static readonly bool Break = false;
    }
}
