using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    [Serializable]
    public struct CombatNetworkMessage
    {
        public uint SequenceId;
        public string SenderId;
        public float Timestamp;
        public CombatActionIntent Intent;

        public override string ToString()
        {
            return $"Seq#{SequenceId} [{SenderId}] At {Timestamp:F2}s (Atk:{Intent.Attack}, Guard:{Intent.GuardHold}, Move:{Intent.MoveInput})";
        }
    }

    /// <summary>
    /// 不良網路模擬通道：提供延遲 (Latency)、抖動 (Jitter) 與丟包 (Packet Loss) 測試支援
    /// </summary>
    public class SimulatedNetworkChannel
    {
        private class QueuedPacket
        {
            public CombatNetworkMessage Message;
            public float DeliverTime;
        }

        public float LatencyMs = 50f;      // 基礎單向延遲 (毫秒)
        public float JitterMs = 10f;       // 延遲隨機抖動 (毫秒)
        [Range(0f, 1f)]
        public float PacketLossRate = 0f;  // 丟包率 (0.0 ~ 1.0)

        private readonly List<QueuedPacket> _packetQueue = new List<QueuedPacket>();
        private uint _nextSeq = 1;
        private uint _droppedCount = 0;
        private uint _sentCount = 0;

        public uint TotalSent => _sentCount;
        public uint TotalDropped => _droppedCount;

        public event Action<CombatNetworkMessage> OnPacketDelivered;
        public event Action<CombatNetworkMessage> OnPacketDropped;

        public void SendPacket(string senderId, CombatActionIntent intent)
        {
            _sentCount++;

            // 模擬丟包
            if (PacketLossRate > 0.001f && UnityEngine.Random.value < PacketLossRate)
            {
                _droppedCount++;
                var droppedMsg = new CombatNetworkMessage
                {
                    SequenceId = _nextSeq++,
                    SenderId = senderId,
                    Timestamp = Time.time,
                    Intent = intent
                };
                OnPacketDropped?.Invoke(droppedMsg);
                return;
            }

            // 計算延遲與抖動
            float jitterOffset = UnityEngine.Random.Range(-JitterMs, JitterMs);
            float totalDelay = Mathf.Max(0f, (LatencyMs + jitterOffset) / 1000f);

            var packet = new QueuedPacket
            {
                Message = new CombatNetworkMessage
                {
                    SequenceId = _nextSeq++,
                    SenderId = senderId,
                    Timestamp = Time.time,
                    Intent = intent
                },
                DeliverTime = Time.time + totalDelay
            };

            _packetQueue.Add(packet);
        }

        public void UpdateQueue()
        {
            float now = Time.time;
            for (int i = _packetQueue.Count - 1; i >= 0; i--)
            {
                if (now >= _packetQueue[i].DeliverTime)
                {
                    var msg = _packetQueue[i].Message;
                    _packetQueue.RemoveAt(i);
                    OnPacketDelivered?.Invoke(msg);
                }
            }
        }

        public void ResetStats()
        {
            _packetQueue.Clear();
            _nextSeq = 1;
            _sentCount = 0;
            _droppedCount = 0;
        }
    }
}
