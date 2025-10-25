using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum EEventType
{
}

public enum EEventTypeWithParam
{
}

public enum EEventAsyncType
{
}

public enum EEventAsyncTypeWithParam
{
}

public class EventManager : BaseSingleton<EventManager>
{
    private Dictionary<EEventType, Action> eventDictionary = new Dictionary<EEventType, Action>();
    private Dictionary<EEventTypeWithParam, Delegate> eventParamDictionary = new Dictionary<EEventTypeWithParam, Delegate>();

    // ─────────────────────────────
    // sync Event (Without Param)
    // ─────────────────────────────

    public void SubscribeToEvent(EEventType eventType, Action listener)
    {
        if (eventDictionary.TryGetValue(eventType, out Action thisEvent))
        {
            thisEvent += listener;
            eventDictionary[eventType] = thisEvent;
        }
        else
        {
            eventDictionary.Add(eventType, listener);
        }
    }

    public void UnsubscribeFromEvent(EEventType eventType, Action listener)
    {
        if (eventDictionary.TryGetValue(eventType, out Action thisEvent))
        {
            thisEvent -= listener;
            if (thisEvent == null)
            {
                eventDictionary.Remove(eventType);
            }
            else
            {
                eventDictionary[eventType] = thisEvent;
            }
        }
    }

    public void TriggerEvent(EEventType eventType)
    {
        if (eventDictionary.TryGetValue(eventType, out Action thisEvent))
        {
            thisEvent?.Invoke();
        }
    }

    // ─────────────────────────────
    // sync Event (With Param)
    // ─────────────────────────────

    public void SubscribeToEvent<T>(EEventTypeWithParam eventType, Action<T> listener)
    {
        if (eventParamDictionary.TryGetValue(eventType, out Delegate thisEvent))
        {
            eventParamDictionary[eventType] = Delegate.Combine(thisEvent, listener);
        }
        else
        {
            eventParamDictionary.Add(eventType, listener);
        }
    }

    public void UnsubscribeFromEvent<T>(EEventTypeWithParam eventType, Action<T> listener)
    {
        if (eventParamDictionary.TryGetValue(eventType, out Delegate thisEvent))
        {
            Delegate newDelegate = Delegate.Remove(thisEvent, listener);
            if (newDelegate == null)
            {
                eventParamDictionary.Remove(eventType);
            }
            else
            {
                eventParamDictionary[eventType] = newDelegate;
            }
        }
    }

    public void TriggerEvent<T>(EEventTypeWithParam eventType, T param)
    {
        if (eventParamDictionary.TryGetValue(eventType, out Delegate thisEvent))
        {
            if (thisEvent is Action<T> action)
            {
                action.Invoke(param);
            }
        }
    }

    public EEventType ParseEventType(string typeStr) =>
    Enum.TryParse(typeStr, out EEventType type) ? type : throw new Exception($"Unknown Type: {typeStr}");
}
