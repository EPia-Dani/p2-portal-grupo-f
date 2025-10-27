using System;
using Core.EventBus;
using Player;
using UnityEngine;

namespace Hittables
{
    public abstract class Hittable : MonoBehaviour
    {
        protected virtual void Start()
        {
            EventBusVoid<Hittable, PlayerEventsEnum>.Subscribe(this, PlayerEventsEnum.Hittable, OnHit);
        }

        private void OnDestroy()
        {
            EventBusVoid<Hittable, PlayerEventsEnum>.Unsubscribe(this, PlayerEventsEnum.Hittable, OnHit);
        }

        protected abstract void OnHit();
    }
}

