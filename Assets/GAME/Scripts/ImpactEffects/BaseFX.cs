using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace EternalKeep
{
    public class BaseFX : MonoBehaviour
    {

        float lifetime;

        IObjectPool<BaseFX> fxPool;


        public void HideFXAfterLifetimeOver()
        {
            StartCoroutine(HideFXAfterLifetimeOver(lifetime));
        }
        IEnumerator HideFXAfterLifetimeOver(float delay)
        {
            yield return new WaitForSeconds(delay);
            fxPool.Release(this);
        }

        public void SetFXObjectPool(IObjectPool<BaseFX> objectPool)
        {
            fxPool = objectPool;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            //rigidBodySelf.position = position;
            //Debug.Log("Projectile POsition SET = " + position);
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            //rigidBodySelf.rotation = rotation;
        }
        
        public void SetLifetime(float value)
        {
            lifetime = value;
        }
    }


}

