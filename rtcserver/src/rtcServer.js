import { Server } from "ws";
import UUID from "uuid";

class RtcClientLogin {
    constructor(client, context) {
        this.client = client;
        this.context = context;
        this.timer = setTimeout(this._timeout, 10000);
    }
    client;
    context;
    timer;
    kind = "login";
    _timeout = () => {
        this.client.close();
    };
    _login(message) {
        if (!message.id) return false;
        if (!this.context.login) return message.data;
        return this.context.login(message.id, message.token, message.data);
    }
    onmessage(message) {
        if (message.action == "login") {
            let info = this._login(message);
            if (!info) return this.client.send({ action: "login_error" });
            this.client.id = message.id;
            this.client.info = info;
            this.client.send({ action: "login_success" });
            this.client.state = new RtcClientWorking(this.client, this.context);
            clearTimeout(this.timer);
        }
        else {
            this.client.send({ action: "login_error" });
        }
    }
    onclose() { }
}

class RtcClientWorking {
    constructor(client, context) {
        this.client = client;
        this.context = context;
        this.context.addClient(client);
    }
    client;
    context;
    kind = "working";
    onmessage(message) {
        this.context.receive(message, this.client);
    }
    onclose() {
        this.context.removeClient(this.client);
    }
}

class RtcClientClosed {
    kind = "closed";
    onmessage() { }
    onclose() { }
}

/**
 * 等待登录的队列
 */
export class RtcClient {

    /**
     * WebSocket连接
     */
    socket;
    /**
     * 标识
     */
    id;
    /**
     * 唯一标识
     */
    version = UUID.v4();
    /**
     * 当前状态
     */
    state;
    /**
     * 加入了的房间列表
     */
    rooms = new Map();
    /**
     * 初始化
     * @param {WebSocket} socket WebSocket连接
     */
    constructor(socket, context) {
        this.socket = socket;
        this.state = new RtcClientLogin(this, context);
        this.socket.on("message", this._ondata);
        this.socket.on("close", this._onclose);
    }
    close() {
        this.socket.close();
    }
    /**
     * 连接关闭
     */
    _onclose = () => {
        this.state.onclose();
        for (let c of this.rooms.values()) c.leave(this);
        this.rooms.clear();
        this.state = new RtcClientClosed();
    };

    /**
     * 收到数据
     */
    _ondata = async data => {
        var jdata = JSON.parse(data);
        this.state.onmessage(jdata);
    };

    /**
     * 发送数据到终端
     * @param {Object} data 要发送的数据
     */
    send(data) {
        var str = JSON.stringify(data);
        this.socket.send(str);
    }
}

class RtcRoom {
    constructor(id) {
        this.id = id;
    }
    id;
    clients = new Map();
    join(client) {
        var now = this.clients.get(client.id);
        if (now == client) return;
        if (now) {
            now.rooms.delete(this.id);
            this.clients.delete(now.id);
        }
        let list = [];
        for (let c of this.clients.values()) {
            c.send({
                kind: "webrtc",
                action: "join",
                from: client.id,
                version: client.version,
                info: client.info
            });
            list.push(client.id);
        }
        client.rooms.set(this.id, this);
        this.clients.set(client.id, client);
    }
    leave(client) {
        var now = this.clients.get(client.id);
        if (now != client) return;
        client.rooms.delete(this.id);
        this.clients.delete(client.id);
        for (let c of this.clients.values()) {
            c.send({
                kind: "webrtc",
                action: "leave",
                id: client.id
            });
        }
    }
}

export class RtcServer {
    clients = new Map();
    rooms = new Map();
    addClient(client) {
        let now = this.clients.get(client.id);
        if (now === client) return;
        if (now) now.close();
        this.clients.set(client.id, client);
    }
    removeClient(client) {
        let now = this.clients.get(client.id);
        if (now !== client) return;
        this.clients.delete(client.id);
    }
    receive = (message, from) => {
        if (message.action == "join") {
            for (let id of message.list) {
                let room = this.rooms.get(id);
                if (!room) {
                    room = new RtcRoom(id);
                    this.rooms.set(id, room);
                }
                room.join(from);
            }
        }
        else if (message.action == "leave") {
            let room = this.rooms.get(message.id);
            if (!room) return;
            room.leave(from);
        }
        else if (message.action == "offeline") {
            let list = [];
            for (let id of message.list) {
                if (!this.clients.has(id)) list.push(id);
            }
            from.send({
                kind: "webrtc",
                action: "offeline",
                list
            });
        }
        else {
            if (!message.to || !this.clients.has(message.to)) return;
            let msg = { ...message, from: from.id };
            if (msg.action == "query") {
                msg.info = from.info;
                msg.version = from.version;
            }
            this.clients.get(message.to).send(msg);
        }
    };
}

/**
 * 监听服务
 * @param {Number|WebServer} server 服务或者服务端口
 * @param {Function} checker 检查用户是否能够登录的接口
 */
export function listenServer(server, checker) {
    let rtcServer = new RtcServer();
    rtcServer.login = checker;
    var ws = new Server((typeof server === 'number') ? { port: server } : { server: server });
    ws.on("connection", (socket) => {
        //创建承载器
        new RtcClient(socket, rtcServer);
    });
}