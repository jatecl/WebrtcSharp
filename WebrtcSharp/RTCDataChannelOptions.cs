namespace WebrtcSharp
{
    /// <summary>
    /// 数据通道选项
    /// </summary>
    public class RTCDataChannelOptions
    {
        // Deprecated. Reliability is assumed, and channel will be unreliable if
        // maxRetransmitTime or MaxRetransmits is set.
        public bool Reliable { get; set; } = true;
        // True if ordered delivery is required.
        public bool Ordered { get; set; } = true;
        // The max period of time in milliseconds in which retransmissions will be
        // sent. After this time, no more retransmissions will be sent.
        //
        // Cannot be set along with |maxRetransmits|.
        // This is called |maxPacketLifeTime| in the WebRTC JS API.
        public int? MaxRetransmitTime { get; set; }
        // The max number of retransmissions.
        //
        // Cannot be set along with |maxRetransmitTime|.
        public int? MaxRetransmits { get; set; }
        // This is set by the application and opaque to the WebRTC implementation.
        public string Protocol { get; set; }
        // True if the channel has been externally negotiated and we do not send an
        // in-band signalling in the form of an "open" message. If this is true, |id|
        // below must be set; otherwise it should be unset and will be negotiated
        // in-band.
        public bool Negotiated { get; set; } = false;
        // The stream id, or SID, for SCTP data channels. -1 if unset (see above).
        public int Id { get; set; } = -1;
    }
}