# WebrtcSharp
Webrtc .Net API


  ```C#
    var factory = new PeerConnectionFactory();
    var configuration = new RTCConfiguration();
    configuration.AddServer("stun:stun.l.google.com:19302");
    var observe = new PeerConnectionObserve();
    var connection = factory.CreatePeerConnection(configuration, observe);
    observe.IceCandidate += iceCandidate =>
    {
        connection.AddIceCandidate(iceCandidate);
    };
    var offer = connection.CreateOffer();
	// any more...
  ```