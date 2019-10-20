import React from "react";
import UUID from "uuid";
import { RtcCall, RtcSocket, LocalMedia } from "../rtc/rtcCall";
import { Link } from "react-router-dom";
import { RtcLinkView, RtcLocalView } from "./rtcLinkView";
/**
 * 屏幕分享页面
 */
export class RtcCallPage extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            links: [],
            video: props.match.params.type == "video"
        };
        this.socket = new RtcSocket(
            "ws://localhost:8124/", 
            UUID.v4(), 
            null, 
            {
                name: "Hello",
                type: "web"
            });
        this.media = new LocalMedia();
        this.call = new RtcCall(this.socket, this.media, [{
                urls: "turn:ice.relywisdom.com:3478",
                credential: "dicrecord",
                username: "dicrecord"
            }]);
        this.call.on("query", (query, info) => {
            query.video = true;
            query.audio = true;
        });
        this.call.on("call", link => {
            this.setState({ links: [...this.state.links.filter(l => l.id != link.id), link] });
            link.on("closed", () => {
                this.setState({ links: this.state.links.filter(d => d != link) });
            });
        });
        this.media.video.on("enabled", enabled => {
            this.setState({ video: enabled });
        });
    }
    socket;
    call;
    media;
    cameraIndex = 0;
    async componentDidMount() {
        window.addEventListener("resize", this._resizeWindow);
        this.socket.connect();
        this.call.join(this.props.match.params.id || "test-room", true);
    }
    _resizeWindow = () => {
        this.setState({ resize: new Date().getTime() })
    }
    componentWillUnmount() {
        window.removeEventListener("resize", this._resizeWindow);
        this.call.close()
        this.socket.close()
    }
    handleSwitchCamera = async () => {
        var list = await this.media.video.getDevices();
        if (!list.length) return;
        ++this.cameraIndex;
        if (this.cameraIndex >= list.length) this.cameraIndex = 0;
        await this.media.video.open(list[this.cameraIndex].deviceId);
    };
    handleHangup = () => {
        this.call.close();
    };
    handleOpenOrCloseCamera = () => {
        var source = this.media.video;
        source.setEnabled(!source.isEnabled);
        if (source.isEnabled) source.open();
    }
    render() {
        var w = Math.sqrt(((this.state && this.state.links && this.state.links.length) || 0) + 1);
        if (window.innerWidth > window.innerHeight) w = Math.ceil(w);
        else w = Math.floor(w);
        w = 100 / Math.max(1, w) + "%";
        return <div className="vbox rela" ref="root">
            <div className="vbox" style={{ left: 10, top: 10, right: 10, bottom: 10, position: "absolute", alignContent: "stretch", flexWrap: "wrap" }}>
                <div className="fbox rela"
                    style={{ width: w, minWidth: w, boxSizing: "border-box", border: "1px solid rgba(0,0,0,0)" }}>
                    <RtcLocalView media={this.media} />
                </div>
                {this.state && this.state.links.map((item, i) => <div
                    key={item.id} className="fbox rela"
                    style={{ width: w, minWidth: w, boxSizing: "border-box", border: "1px solid rgba(0,0,0,0)" }}>
                    <RtcLinkView link={item} />
                </div>)}
                {this.state.isHangUped
                    ? <div className="center" style={{ padding: "5px 10px", background: "#fff" }}>电话已挂断</div>
                    : <div className="abs btn-group" role="group" style={{ right: 10, top: 10 }}>
                        {this.state.video ? <button onClick={this.handleSwitchCamera} className="btn btn-primary">切换摄像头</button> : null}
                        <button onClick={this.handleOpenOrCloseCamera} className="btn btn-info">{this.state.video ? "关闭摄像头" : "打开摄像头"}</button>
                        <button onClick={this.handleHangup} className="btn btn-danger">挂断电话</button>
                    </div>}
            </div>
        </div>
    }
}

/**
 * 创建屏幕分享页
 */
export class CreateCallPage extends React.Component {
    id = UUID.v4();
    render() {
        return <div className="container-fluid">
            <div className="jumbotron jumbotron-fluid" style={{ background: "none" }}>
                <h1 className="display-4">多媒体通话</h1>
                <p className="lead">请复制下面的地址给你的好友</p>
                <hr className="my-4" />
                <p>
                    视频通话：<code>{`${location.href.split("#")[0].split("?")[0]}#/call/${this.id}/video`}</code>
                </p>
                <p>
                    音频通话：<code>{`${location.href.split("#")[0].split("?")[0]}#/call/${this.id}/audio`}</code>
                </p>
                <div className="btn-group" role="group">
                    <Link to={`/call/${this.id}/video`} className="btn btn-primary btn-lg">视频通话</Link>
                    <Link to={`/call/${this.id}/audio`} className="btn btn-secondary btn-lg">音频通话</Link>
                </div>
            </div>
        </div>;
    }
}
