using System;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 右手接觸偵測橋接器：
    /// 掛載於角色右手上，持有觸發碰撞體 (Trigger Collider)，
    /// 當雙方在伸手或等待階段接觸時，通報判定元件。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HandContactBridge : MonoBehaviour
    {
        public HandshakeParticipant Owner { get; private set; }
        public Collider HandCollider { get; private set; }

        public event Action<HandContactBridge> OnTriggerContact;

        public void Initialize(HandshakeParticipant owner)
        {
            Owner = owner;
            HandCollider = GetComponent<Collider>();
            if (HandCollider != null)
            {
                HandCollider.isTrigger = true;
            }

            // 確保觸發事件運作：附加 kinematic Rigidbody
            var rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            var otherBridge = other.GetComponent<HandContactBridge>();
            if (otherBridge != null && otherBridge.Owner != null && otherBridge.Owner != Owner)
            {
                OnTriggerContact?.Invoke(otherBridge);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            var otherBridge = other.GetComponent<HandContactBridge>();
            if (otherBridge != null && otherBridge.Owner != null && otherBridge.Owner != Owner)
            {
                OnTriggerContact?.Invoke(otherBridge);
            }
        }
    }
}
