# WebrtcSharp
Webrtc .Net API


  ```C#
    var factory = new PeerConnectionFactory();
    var configuration = new RTCConfiguration();
    configuration.AddServer("stun:stun.l.google.com:19302");
    var connection = factory.CreatePeerConnection(configuration);
    connection.IceCandidate += iceCandidate =>
    {
        connection.AddIceCandidate(iceCandidate);
    };
    var offer = await connection.CreateOffer();
    // any more...
  ```

Demo
  首次运行时，要更新node_modules
  ```
    cd rtcclient
    start npm install
    cd ..\rtcserver
    start npm install
  ```
  运行起信令服务器和客户端：
  ```
    cd rtcclient
    start npm test
    cd ..\rtcserver
    start npm test
  ```
  打开浏览器 http://localhost:8080/#/share/view/test-room
  然后打开并运行 ScreenShare\ScreenShare.csproj