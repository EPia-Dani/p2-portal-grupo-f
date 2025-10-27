using Hittables;
using Player;
using UnityEngine;

namespace Hittables
{
    public class HittableCube : Hittable
    {

        protected override void OnHit()
        {
            Debug.Log("HittableCube was hit!");
        }
    }

}