﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MonobehaviorHandler
{
    private static MonobehaviorHandler _instance;
    public static MonobehaviorHandler GetMonobeharior()
    {
        if (_instance == null)
            _instance = new MonobehaviorHandler();
        return _instance;
    }

    private Dictionary<string, MonoBehaviour> _behaviorDict;

    private MonobehaviorHandler()
    {
        _behaviorDict = new Dictionary<string, MonoBehaviour>();
    }

    public T GetObject<T>(string id) where T : MonoBehaviour
    {
        if (_instance._behaviorDict.ContainsKey(id))
        {
            T obj = (T)_instance._behaviorDict[id];
            if (obj != null)
                return obj;
            _instance._behaviorDict.Remove(id);
        }
        T found = GameObject.Find(id).GetComponent<T>();
        _instance._behaviorDict.Add(id, found);
        return found;
    }
}

