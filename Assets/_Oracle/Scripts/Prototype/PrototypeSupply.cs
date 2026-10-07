using UnityEngine;

namespace Oracle.Prototype
{
    public sealed class PrototypeSupply : MonoBehaviour
    {
        public bool IsCollected { get; private set; }

        public void Collect()
        {
            if (IsCollected)
            {
                return;
            }

            IsCollected = true;
            gameObject.SetActive(false);
        }
    }
}
