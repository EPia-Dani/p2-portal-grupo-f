using Hittables;
using Player;
using UnityEngine;

namespace Hittables
{
    public class HittableSphere : Hittable
    {
        protected override void OnHit()
        {
            Debug.Log("HittableSphere was hit!");
        }
    }
}
