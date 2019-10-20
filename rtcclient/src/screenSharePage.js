
import React from "react";
import UUID from "uuid";
import { RtcCall, RtcSocket, LocalMedia } from "../rtc/rtcCall";
import { Link } from "react-router-dom";
import { RtcLinkView, RtcLocalView } from "./rtcLinkView";
/**
 * 屏幕分享页面
 */
export class ScreenSharePage extends React.Component {
    constructor(props) {
        super(props)

        var create = this.props.match.params.action == "create";
        this.socket = new RtcSocket(
            "ws://localhost:8124/", 
            UUID.v4(), 
            null, 
            {
                name: "Hello",
                type: create ? "device" : "web"
            });
        this.media = new LocalMedia();
        this.call = new RtcCall(this.socket, this.media, [{
                urls: "turn:ice.relywisdom.com:3478",
                credential: "dicrecord",
                username: "dicrecord"
            }]);
        this.call.on("query", (query, info) => {
            query.video = !create;
            query.audio = !create;
        });

        this.call.on("call", link => {
            if (create) { //统计连接人数
                link.on("closed", () => this.setState({ number: this.call.remotes.size }));
                this.setState({ number: this.call.remotes.size });
            }
            else {
                this.setState({ link });
            }
        });
    }
    socket;
    call;
    media;
    async componentDidMount() {
        var create = this.props.match.params.action == "create";
        if (create) { //选择录屏，打开录屏
            var devices = await this.call.local.video.getDevices();
            for (var i = 0; i < devices.length; ++i) {
                var o = devices[i];
                if (o.label == "screen-capture-recorder") {
                    this.call.local.video.source = o.deviceId;
                    this.cameraIndex = i;
                    break;
                }
            }
            this.call.local.open();
        }
        this.socket.connect();
        this.call.join(this.props.match.params.id || "test-room", create);
    }
    componentWillUnmount() {
        this.call.close()
        this.socket.close()
    }
    cameraIndex = 0;
    handleSwitchCamera = async () => {
        var list = await this.call.local.video.getDevices();
        if (!list.length) return;
        ++this.cameraIndex;
        if (this.cameraIndex >= list.length) this.cameraIndex = 0;
        await this.call.local.video.open(list[this.cameraIndex].deviceId);
    };
    render() {
        var create = this.props.match.params.action == "create";
        return <div className="vbox rela" ref="root">
            {create ? <RtcLocalView media={this.call && this.call.local} /> : this.state && this.state.link ? <RtcLinkView link={this.state && this.state.link} /> : null}
            {create ? <div className="abs btn-group" role="group" style={{ right: 10, top: 10 }}>
                <button className="btn btn-secondary" disabled>{(this.state && this.state.number) || 0}人观看</button>
                <button onClick={this.handleSwitchCamera} className="btn btn-info">切换摄像头</button>
            </div> : null}
        </div>
    }
}

/**
 * 创建屏幕分享页
 */
export class CreateScreenSharePage extends React.Component {
    id = UUID.v4();
    render() {
        return <div className="container-fluid">
            <div className="jumbotron jumbotron-fluid" style={{ background: "none" }}>
                <h1 className="display-4">分享你的屏幕</h1>
                <p className="lead">请复制下面的地址给你的好友</p>
                <hr className="my-4" />
                <p>
                    <code>{`${location.href.split("#")[0].split("?")[0]}#/share/view/${this.id}`}</code>
                </p>
                <Link to={`/share/create/${this.id}`} className="btn btn-primary btn-lg">开始分享屏幕</Link>
            </div>
        </div>;
    }
}
