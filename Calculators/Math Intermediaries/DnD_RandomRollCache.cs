using QuizCanners.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dungeons_and_Dragons.Tables
{

    public class RandomRollCache
    {
        private readonly Dictionary<int, RollResult> _cachedResults = new();

        public RollResult Get(RanDndSeed seed, Func<RollResult> defaultGettter) 
        {
            if (_cachedResults.TryGetValue(seed.Value, out var result)) 
            {
                return result;
            }

            result = defaultGettter();

            const int MAX_CACHE_SIZE = 512;

            if (_cachedResults.Count > MAX_CACHE_SIZE) 
            {
                _cachedResults.Clear();
                if (Application.isEditor)
                {
                    Debug.LogWarning("Roll Result Cache > {0} Clearing".F(MAX_CACHE_SIZE));
                }
            }
            
            _cachedResults.Add(seed.Value, result);

            return result;
        }

    }
}
