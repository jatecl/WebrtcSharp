/**
 * WebRTC 简单通信协议
 */

import { EventEmitter } from "events";

const RTCPeerConnection = (window.PeerConnection || window.RTCPeerConnection || window.webkitPeerConnection || window.webkitRTCPeerConnection || window.mozRTCPeerConnection);
const RTCIceCandidate = (window.RTCIceCandidate || window.mozRTCIceCandidate);
const RTCSessionDescription = (window.RTCSessionDescription || window.mozRTCSessionDescription); 

var firefox = navigator.userAgent.match(/firefox/i);


/**
 * WebSocket 连接
 */
export class RtcSocket extends EventEmitter {
    /**
     * 创建一个连接
     * @param {String} server ws服务地址
     * @param {String} id 当前用户id
     */
    constructor(server, id, token, info) {
        super();
        this.server = server;
        this.id = id;
        this.token = token;
        this.info = info;
        this.setState(new RtcSocketStoped(), true);
    }
    /**
     * ws服务地址
     */
    server;
    /**
     * 状态
     */
    state;
    /**
     * 自己的ID
     */
    id;
    /**
     * 登录用的token
     */
    token;
    /**
     * 附加信息
     */
    info;
    /**
     * 消息分发器
     */
    filters = new Map();
    /**
     * 连接中
     */
    connecting = false;
    /**
     * 设置消息处理器
     * @param {String} key 消息类型
     * @param {Object} val 消息处理器
     */
    setFilter(key, val) {
        if (this.filters.has(key)) throw new Error(`分发器${key}已存在`);
        this.filters.set(key, val);
    }
    /**
     * 移除消息处理器
     * @param {String} key 消息类型
     */
    removeFilter(key) {
        this.filters.remove(key);
    }
    /**
     * 设置状态
     * @param {Object} state 状态
     */
    setState(state, noemit) {
        if (state == this.state) return;
        if (this.state && this.state.clear) this.state.clear();
        state.socket = this;
        this.state = state;
        state.start && state.start();
        if (!noemit) {
            this.emit(state.kind, state);
            this.emit("changed", state);
        }
    }
    /**
     * 收到服务器信息
     * @param {Object} msg 信息
     */
    onmessage(msg) {
        if (this.state.kind == "connected"
            && msg.kind
            && this.filters.has(msg.kind)) {
            return this.filters.get(msg.kind).onmessage(msg);
        }
        this.state.onmessage && this.state.onmessage(msg);
    }
    /**
     * 收到关闭消息
     */
    onclose() {
        this.state.onclose && this.state.onclose();
        this.setState(new RtcSocketStoped());
    }
    /**
     * 发送消息
     * @param {Object} msg 消息
     */
    send(msg) {
        if (!this.state.link) return false;
        this.state.link.send(JSON.stringify(msg));
        return true;
    }
    /**
     * 开始连接
     */
    async connect() {
        if (this.connecting) return;
        this.connecting = true;
        while (this.connecting) {
            //尝试连接
            this.state.connect && this.state.connect();
            //等待连接关闭
            await new Promise(cs => this.once("close", cs));
            //1秒后重试
            if (this.connecting) await new Promise(cs => setTimeout(cs, 1000));
        }
    }
    /**
     * 结束连接
     */
    close() {
        this.connecting = false;
        this.onclose();
    }
}
/**
 * websocket未连接状态
 */
class RtcSocketStoped {
    /**
     * 连接
     */
    socket;
    /**
     * 类型
     */
    kind = "close";
    /**
     * 开始连接
     */
    connect() {
        this.socket.setState(new RtcSocketConnecting());
    }
}
/**
 * 正在连接状态
 */
class RtcSocketConnecting {
    /**
     * 状态设置时被调用
     */
    start() {
        this.link = new WebSocket(this.socket.server);
        //如果在连接中断开了，直接设置为连接失败
        this.link.onclose = () => this.socket.onclose();
        //对认证消息进行响应
        this.link.onmessage = e => this.socket.onmessage(JSON.parse(e.data));
        //发送认证消息
        this.link.onopen = () => {
            this.socket.setState(new RtcSocketLogin(this.link));
        };
    }
    /**
     * 类型
     */
    kind = "connecting";
    /**
     * 连接
     */
    socket;
    /**
     * @type {WebSocket}
     */
    link;
}
/**
 * 正在登录状态
 */
class RtcSocketLogin {
    /**
     * 连上服务器后，启动登录
     * @param {WebSocket} link web socket
     */
    constructor(link) {
        this.link = link;
    }
    /**
     * 类型
     */
    kind = "login";
    /**
     * 连接
     */
    socket;
    /**
     * @type {WebSocket}
     */
    link;
    /**
     * 状态设置后，初始化
     */
    start() {
        this.socket.send({
            action: "login",
            id: this.socket.id,
            token: this.socket.token,
            info: this.socket.info
        });
        this._timer = setTimeout(this._timeout, 10000);
    }
    /**
     * 登录失败定时器
     */
    _timer;
    /**
     * 登录成功时，清除失败定时器
     */
    _clearTimer() {
        if (this._timer) {
            clearTimeout(this._timer);
            this._timer = 0;
        }
    }
    /**
     * 收到服务器消息
     * @param {Object} msg 服务器端消息
     */
    onmessage(msg) {
        if (msg.action == "login_success") {
            this._clearTimer();
            this.socket.setState(new RtcSocketConnected(this.link));
        }
        else if (msg.action == "login_error") {
            this.onclose();
        }
    }
    /**
     * 如果连接关闭
     */
    onclose = () => {
        this._clearTimer();
    };
    /**
     * 如果登录超时
     */
    _timeout = () => {
        this._timer = 0;
        this.link.close();
    };
}
/**
 * 连接成功状态
 */
class RtcSocketConnected {
    /**
     * 创建连接成功状态
     * @param {WebSocket} link web socket
     */
    constructor(link) {
        this.link = link;
    }
    /**
     * 类型
     */
    kind = "connected";
    /**
     * 连接
     */
    socket;
    /**
     * @type {WebSocket}
     */
    link;
}
/**
 * 通话
 */
export class RtcCall extends EventEmitter {
    /**
     * ICE服务器
     */
    iceServers;
    /**
     * 本地媒体源
     */
    local;
    /**
     * 连接
     */
    socket;
    /**
     * 是否可以发送加入事件
     */
    _canbejoin = false;
    /**
     * 创建一个通话
     * @param {RtcSocket} socket 连接
     * @param {LocalStream} local_media 本地媒体源
     * @param {Array<IceServer>}} iceServers ICE服务器
     */
    constructor(socket, local_media, iceServers) {
        super();
        this.socket = socket;
        this.local = local_media;
        this.iceServers = iceServers;
        this.socket.setFilter("webrtc", this);
        this.socket.on("connected", this._connected);
        this.socket.on("changed", this._socketchanged);
        this.local.on("changed", this._localChanged);
    }
    /**
     * 状态改变时发生
     */
    _socketchanged = () => {
        this._canbejoin = false;
    };
    /**
     * 连接成功时
     */
    _connected = () => {
        if (!this.remotes.size) return this._startCall();
        let list = [];
        for (let id of this.remotes.keys()) list.push(id);
        this.socket.send({
            kind: "webrtc",
            action: "offeline",
            list
        });
    };
    /**
     * 通话列表
     */
    remotes = new Map();
    /**
     * 房间索引
     */
    rooms = new Map();
    /**
     * 加入房间
     * @param {String} id 房间id
     */
    join(id) {
        if (this.rooms.has(id)) return false;
        this.rooms.set(id, true);
        if (this._canbejoin) {
            this.socket.send({
                kind: "webrtc",
                action: "join",
                list: [id]
            });
        }
        return true;
    }
    /**
     * 收到的消息
     * @param {Object} msg 消息
     */
    onmessage(msg) {
        if (msg.action == "join") {
            this._callMedia(msg, true);
        }
        else if (msg.action == "leave") {
            this._closeMedia(msg.id);
        }
        else if (msg.action == "offeline") {
            for (let id of msg.list) {
                this._closeMedia(id);
            }
            this._startCall();
        }
        else {
            if (!msg.from) return;
            if (msg.action == "query") {
                this._callMedia(msg, false);
            }
            let now = this.remotes.get(msg.from);
            if (now) now.onmessage(msg);
        }
    }
    /**
     * 关掉通话
     * @param {String} id 通话id
     */
    _closeMedia(id) {
        let now = this.remotes.get(id);
        if (!now) return;
        this.remotes.delete(id);
        now.close();
    }
    /**
     * 开始通话
     * @param {Object}} msg 通话配置
     * @param {Boolean} master 是否主叫
     */
    _callMedia(msg, master) {
        let { from, version, info } = msg;
        let now = this.remotes.get(from);
        if (!now || now.version != version) {
            if (now) now.close();
            now = new RemoteMedia(master, this, from, version, info);
            this.remotes.set(from, now);
            now.connect();
            this.emit("call", now);
        }
    }
    /**
     * 本地媒体变化时，改变推送方式
     */
    _localChanged = () => {
        for (let remote of this.remotes.values()) remote.localChanged();
    };
    /**
     * 开始实际加入房间
     */
    _startCall() {
        this._canbejoin = true;
        let list = [];
        for (let r of this.rooms.keys()) list.push(r);
        if (!list.length) return;
        this.socket.send({
            kind: "webrtc",
            action: "join",
            list
        });
    }
    /**
     * 结束通话
     */
    close() {
        for (let c of this.rooms.keys()) this.socket.send({
            kind: "webrtc",
            action: "leave",
            id: c
        });
        this.rooms.clear();
        let list = [];
        for (let c of this.remotes.keys()) list.push(c);
        for (let c of list) this._closeMedia(c); 
    }
}
/**
 * 某种类型的媒体流来源
 */
class LocalMediaSource extends EventEmitter {
    constructor(kind) {
        super();
        this.kind = kind;
    }
    /**
     * 媒体源描述
     */
    source;
    /**
     * 当前媒体流
     */
    _track;
    /**
     * 媒体类型
     */
    kind;
    /**
     * 
     */
    isEnabled = true;
    /**
     * 设置当前媒体源是否可用
     * @param {Boolean} enabled 设置当前媒体源是否可用
     */
    setEnabled(enabled) {
        if (enabled != this.isEnabled) {
            this.isEnabled = enabled;
            if (!enabled) this._justClose();
            this.emit("enabled", enabled);
        }
    }
    /**
     * 创建媒体通道
     */
    async getTrack() {
        if (!this._track && this.isEnabled) await this.open();
        return this._track;
    }
    /**
     * 打开媒体流
     * @param {String} source 媒体源
     */
    async open(source) {
        try {
            if (source) {
                if (source != this.source) this._justClose(true);
                this.source = source;
            }
            else if (!this.source) {
                var vss = await this.getDevices();
                if (vss.length) this.source = vss[0].deviceId;
            }
            if (!this.isEnabled) {
                this._justClose();
                return false;
            }
            if (this._track) return true;
            var opt = {};
            opt[this.kind] = { optional: [{ sourceId: this.source }] };
            var stream = await this.getUserMedia(opt);
            this._track = stream.getTracks()[0];
            if (firefox) this._track._stream = stream;
            this.emit("changed", true);
            this.emit("open");
            return true;
        }
        catch (e) {
            return false;
        }
    }
    /**
     * 拉取媒体流
     * @param {Object} opts 媒体选项
     */
    async getUserMedia(opts) {
        if (navigator.mediaDevices) {
            var pro = navigator.mediaDevices.getUserMedia(opts);
            return await pro;
        }
        else {
            return await new Promise((cs, ce) => {
                if (navigator.getUserMedia) navigator.getUserMedia(opts, cs, ce);
                else if (navigator.mozGetUserMedia) navigator.mozGetUserMedia(opts, cs, ce);
                else if (navigator.webkitGetUserMedia) navigator.webkitGetUserMedia(opts, cs, ce);
                else ce(new Error("不支持的api"));
            });
        }
    }
    /**
     * 获得所有媒体设备
     */
    async getDevices() {
        let deviceInfos = await navigator.mediaDevices.enumerateDevices();
        var input = this.kind + "input";
        deviceInfos = deviceInfos.filter(o => o.kind == input);
        return deviceInfos;
    }
    /**
     * 关闭并通知变化
     */
    _justClose(nochange) {
        if (this._track) {
            this._track.stop()
            this._track = null;
            if (!nochange) this.emit("changed", false);
        }
    }
    /**
     * 关闭媒体流
     */
    close() {
        this._justClose();
        this.emit("close");
    }
    /**
     * 是否可用
     */
    async usable() {
        if (!this.isEnabled) return false;
        var devs = await this.getDevices();
        return devs.length > 0;
    }
}
/**
 * 多媒体源
 */
export class LocalMedia extends EventEmitter {
    constructor() {
        super();
        this.all = { video: this.video, audio: this.audio };
        this.video.on("changed", () => this._mediaChanged(this.video));
        this.audio.on("changed", () => this._mediaChanged(this.audio));
    }
    /**
     * 视频源
     */
    video = new LocalMediaSource("video");
    /**
     * 音频源
     */
    audio = new LocalMediaSource("audio");
    /**
     * 所有媒体源
     */
    all;
    /**
     * 打开媒体
     */
    async open() {
        for (var key in this.all) await this.all[key].open();
    }
    /**
     * 媒体发生变化时发生
     */
    _mediaChanged = async source => {
        this.emit("changed", source)
    };
}
/**
 * 远程媒体
 */
class RemoteMedia extends EventEmitter {
    /**
     * 创建远程媒体
     * @param {Boolean} master 是否主叫
     * @param {RtcCall} call 通话管理器
     * @param {String} id 对方id
     * @param {String} version 对方版本
     * @param {Object} info 对方附加信息
     */
    constructor(master, call, id, version, info) {
        super();
        this.master = master;
        this.call = call;
        this.socket = call.socket;
        this.id = id;
        this.version = version;
        this.info = info;
    }
    /**
     * 是否为主叫
     */
    master; 
    /**
     * 通话
     */
    call;
    /**
     * 连接
     */
    socket;
    /**
     * 对方id
     */
    id;
    /**
     * 对方版本编号
     */
    version;
    /**
     * 对方信息
     */
    info;
    /**
     * 流缓存
     */
    stream;
    /**
     * P2P连接
     */
    connection;
    /**
     * 收到新消息
     * @param {Object} msg 消息
     */
    onmessage(msg) {
        this.connection && this.connection.onmessage(msg);
    }
    /**
     * 是否可以重连
     */
    _connecting() {
        return this.socket.state.kind == "connected"
            && this.call.remotes.has(this.id);
    }
    /**
     * 连接对方并保持
     */
    async connect() {
        while (this._connecting()) {
            this.connection = new MediaConnection(this);
            await new Promise(cs => this.connection.once("state_closed", cs));
        }
        this.emit("closed");
    }
    /**
     * 本地媒体变化时被调用
     */
    localChanged() {
        this.connection.tryReoffer();
    }
    /**
     * 结束远程P2P连接
     */
    close() {
        this.connection.close();
    }
}
/**
 * P2P连接
 */
class MediaConnection extends EventEmitter {
    /**
     * 远程媒体
     */
    remote;
    /**
     * 当前状态
     */
    state;
    /**
     * 轨道缓存
     */
    _tracks = {};
    /**
     * 超时检查
     */
    _timer;
    /**
     * candidate缓存
     */
    _candidates = [];
    /**
     * 连接
     */
    get socket() {
        return this.remote.call.socket;
    }
    /**
     * 本地媒体
     */
    get local() {
        return this.remote.call.local;
    }
    /**
     * 通话管理器
     */
    get call() {
        return this.remote.call;
    }
    /**
     * 创建一个P2P连接
     * @param {RemoteMedia}} remote 远程媒体
     */
    constructor(remote) {
        super();
        this.remote = remote;
        this.connection = new RTCPeerConnection({ iceServers: this.call.iceServers });
        //当需要发送candidate的时候
        this.connection.onicecandidate = evt => {
            if (!evt.candidate) return;
            this.socket.send({
                kind: "webrtc",
                action: "candidate",
                to: this.remote.id,
                candidate: evt.candidate
            });
        };
        //如果支持新的api，当收到track
        if (this.connection.getSenders) this.connection.ontrack = evt => {
            this._tracks[evt.track.kind] = evt.track;
            this._emitAddTrack();
        };
        //如果不支持新的api，当收到stream时
        else this.connection.onaddstream = evt => {
            for (var t of evt.stream.getTracks()) this._tracks[t.kind] = t;
            this._emitAddTrack();
        };
        //当媒体流被移除时
        this.connection.onremovestream = evt => {
            var deletes = [];
            for (var t of evt.stream.getTracks()) {
                if (this._tracks[t.kind] == t) deletes.push(t.kind);
            }
            for (var del of deletes) delete this._tracks[del];
            if (deletes.length) this._emitAddTrack();
        };
        //当连接状态发生改变时
        this.connection.oniceconnectionstatechange = () => {
            var state = this.connection.iceConnectionState;
            //new, checking, connected, completed, failed, disconnected, closed;
            switch (state) {
                case "connected":
                case "completed":
                case "failed":
                case "disconnected":
                case "closed":
                    this._candidates = [];
                    break;
            }
            this.emit("statechanged", state);
            this.emit("state_" + state);
        };
        //检查超时
        this._timer = setTimeout(this._timeoutchecker, 10000);
        this.once("statechanged", this._clearchecker);
        //初始化状态
        this.resetState(true);
    }
    /**
     * 初始化状态
     * @param {Boolean} clear 是否超时重置
     */
    resetState(clear) {
        if (this.remote.master) this.setState(new ConnectionQuery(clear));
        else this.setState(new ConnectionWaitForQuery());
    }
    /**
     * 收到消息
     * @param {Object} msg 消息
     */
    onmessage(msg) {
        if (msg.action == "candidate") {
            let candidate = new RTCIceCandidate(msg.candidate);
            if (this._candidates !== null) this._candidates.push(candidate);
            else this.connection.addIceCandidate(candidate);
        }
        else {
            this.state.onmessage && this.state.onmessage(msg);
        }
    }
    /**
     * 取消超时检查
     */
    _clearchecker = () => {
        if (!this._timer) return;
        clearTimeout(this._timer);
        this._timer = 0;
    };
    /**
     * 触发超时检查
     */
    _timeoutchecker = () => {
        this._timer = 0;
        this.connection.close();
    };
    /**
     * 重新发送offer
     */
    tryReoffer() {
        var state = this.connection.iceConnectionState;
        if (state == "connected" || state == "completed") {
            this.setState(new ConnectionQuery(true));
        }
    }
    /**
     * 触发stream事件
     */
    _emitAddTrack = () => {
        var stream = new MediaStream();
        for (var key in this._tracks) stream.addTrack(this._tracks[key]);
        this.remote.stream = stream;
        this.remote.emit('addtrack', stream);
    };
    /**
     * 设置当前状态
     * @param {Object} state 当前状态
     */
    setState(state) {
        if (state == this.state) return;
        if (this.state && this.state.clear) this.state.clear();
        state.connection = this;
        this.state = state;
        state.start && state.start();
    }
    /**
     * 发送缓存的candidates
     */
    _sendCachedCandidates() {
        if (this._candidates === null) return;
        for (let c of this._candidates) this.connection.addIceCandidate(c);
        this._candidates = null;
    }
    /**
     * 创建offer
     * @param {Object} query 对方要求
     */
    async createOffer() {
        let offer = await this.connection.createOffer({ iceRestart: true });
        if (!offer) throw new Error("创建offer失败");
        await this.connection.setLocalDescription(offer);
        this._sendCachedCandidates();
        return { sdp: offer.sdp, type: offer.type };
    }
    /**
     * 创建answer
     * @param {Object} offer 对方offer
     */
    async createAnswer(offer) {
        await this.connection.setRemoteDescription(new RTCSessionDescription(offer));
        let answer = await this.connection.createAnswer();
        if (!answer) throw new Error("创建answer失败");
        await this.connection.setLocalDescription(answer);
        this._sendCachedCandidates();
        return { sdp: answer.sdp, type: answer.type };
    }
    /**
     * 设置对方回传的answer
     * @param {Object} answer 对方answer
     */
    async setAnswer(answer) {
        await this.connection.setRemoteDescription(new RTCSessionDescription(answer));
    }
    /**
     * 根据对方要求和自己的实际情况设置要发送的媒体
     * @param {Object} query 对方要求
     */
    async setLocalMedia(query) {
        let ret = {};
        //如果支持新的api
        if (this.connection.getSenders) {
            let senders = this.connection.getSenders();
            for (var key in this.local.all) {
                let exists = senders.filter(sd => (!sd.track && key == "video") || (sd.track && sd.track.kind == key));
                if (!query || query[key]) {
                    var track = await this.local.all[key].getTrack();
                    if (track) {
                        if (exists.length) {
                            for (let sender of exists) {
                                if (sender.track == track) continue;
                                sender.replaceTrack(track);
                            }
                        }
                        else {
                            if (firefox) this.connection.addTrack(track, track._stream);
                            else this.connection.addTrack(track);
                        }
                        ret[key] = true;
                        //如果已经设置或者替换，就继续
                        continue;
                    }
                }
                for (let sender of exists) {
                    if (sender.track) {
                        try { this.connection.removeTrack(sender.track); }
                        catch (e) {  }
                    }
                }
            }
        }
        //如果只支持旧的api
        else {
            var exists = this.connection.getLocalStreams();
            for (var i of exists) this.connection.removeStream(i);
            for (var key in this.local.all) {
                if (query && !query[key]) continue;
                var track = await this.local.all[key].getTrack();
                if (!track) continue;
                var stream = new MediaStream();
                stream.addTrack(track);
                this.connection.addStream(stream);
                ret[key] = true;
            }
        }
        return ret;
    }
    /**
     * 关闭P2P连接
     */
    close() {
        this.connection.close();
    }
}
/**
 * 连接状态
 */
class IConnectionState {
    /**
     * P2P连接
     */
    connection;
    /**
     * 是否要设置超时
     */
    _clear = true;
    /**
     * 状态初始化
     */
    start() {
        if (this._clear) this._timer = setTimeout(this._timeout, 10000);
    }
    /**
     * 超时计时器
     */
    _timer;
    /**
     * 结束时，清理超时计时器
     */
    clear() {
        if (!this._timer) return;
        clearTimeout(this._timer);
        this._timer = 0;
    }
    /**
     * 触发超时，重置状态
     */
    _timeout = () => {
        this._timer = 0;
        this.connection.resetState(true);
    };
    /**
     * 远程媒体
     */
    get remote() {
        return this.connection.remote;
    }
    /**
     * 通话管理器
     */
    get call() {
        return this.connection.remote.call;
    }
    /**
     * 信令连接
     */
    get socket() {
        return this.connection.remote.call.socket;
    }
}
/**
 * 等待查询状态
 */
class ConnectionWaitForQuery {
    /**
     * 收到消息
     * @param {Object} msg 消息
     */
    async onmessage(msg) {
        if (msg.action == "query") {
            let media = await this.connection.setLocalMedia(msg.query);
            if (!media.video || !media.audio) this.connection.setState(new ConnectionMedia(media));
            else this.connection.setState(new ConnectionOffer());
        }
    }
}
/**
 * 发送查询并等待query和media的状态
 */
class ConnectionQuery extends IConnectionState {
    /**
     * 创建状态
     * @param {Boolean} clear 是否设置超时
     */
    constructor(clear) {
        super();
        this._clear = clear;
    }
    /**
     * 初始化
     */
    start() {
        super.start();
        let query = {};
        this.call.emit("query", query, this.remote);
        this.socket.send({
            kind: "webrtc",
            action: "query",
            to: this.remote.id,
            query
        });
    }
    /**
     * 收到消息
     * @param {Object} msg 消息
     */
    async onmessage(msg) {
        if (msg.action == "media") {
            let media = await this.connection.setLocalMedia(msg.query);
            let rm = msg.media;
            if (media.video || (media.audio && !rm.video)) this.connection.setState(new ConnectionOffer());
            else {
                this.socket.send({
                    kind: "webrtc",
                    action: "require",
                    to: this.remote.id
                });
            }
        }
        else if (msg.action == "offer") {
            let answer = await this.connection.createAnswer(msg.offer);
            this.socket.send({
                kind: "webrtc",
                action: "answer",
                to: this.remote.id,
                answer
            });
            this.connection.setState(new ConnectionWaitForQuery());
        }
    }
}
/**
 * 
 */
class ConnectionMedia extends IConnectionState {
    constructor(media) {
        super();
        this.media = media;
    }
    media;
    start() {
        super.start();
        let query = {};
        this.call.emit("query", query, this.remote);
        this.socket.send({
            kind: "webrtc",
            action: "media",
            to: this.remote.id,
            media: this.media,
            query,
        });
    }
    /**
     * 收到消息
     * @param {Object} msg 消息
     */
    async onmessage(msg) {
        if (msg.action == "offer") {
            let answer = await this.connection.createAnswer(msg.offer);
            this.socket.send({
                kind: "webrtc",
                action: "answer",
                to: this.remote.id,
                answer
            });
            this.connection.setState(new ConnectionWaitForQuery());
        }
        else if (msg.action == "require") {
            this.connection.setState(new ConnectionOffer());
        }
    }
}
/**
 * 接收查询，并发送查询与offer然后等待answer
 */
class ConnectionOffer extends IConnectionState {
    /**
     * 初始化
     */
    async start() {
        super.start();
        let offer = await this.connection.createOffer();
        this.socket.send({
            kind: "webrtc",
            action: "offer",
            to: this.remote.id,
            offer
        });
    }
    /**
     * 收到消息
     * @param {Object} msg 消息
     */
    async onmessage(msg) {
        if (msg.action == "answer") {
            await this.connection.setAnswer(msg.answer);
            this.connection.setState(new ConnectionWaitForQuery());
        }
    }
}