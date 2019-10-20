import React from "react";
import { RtcCall, RtcSocket } from "../rtc/rtcCall";
import { Redirect } from "react-router-dom";
import UUID from "uuid";
import { RtcLinkView } from "./rtcLinkView";

/**
 * 屏幕分享页面
 */
export class RtcLivePage extends React.Component {
    constructor(props) {
        super(props);
        this.state = { links: [] };

        var deviceCache = [];
        this.media = new LocalMedia();
        var params = this.props.match.params;
        for (var id of params.id.split(',')) {
            let socket = new RtcSocket(
                "ws://localhost:8124/",
                UUID.v4() + "/" + id + "/" + params.type,
                null,
                {
                    name: "live player",
                    type: "web"
                });
            let call = new RtcCall(this.socket, this.media, [{
                urls: "turn:ice.relywisdom.com:3478",
                credential: "dicrecord",
                username: "dicrecord"
            }]);
            call.on("query", (query, info) => {
                query.video = true;
                query.audio = true;
            });
            call.on("call", device => {
                device.on("addtrack", stream => {
                    if (deviceCache.indexOf(device) >= 0) return;
                    if (stream.getVideoTracks().length == 0 && stream.getAudioTracks().length == 0) return;
                    deviceCache = [...deviceCache, device];
                    this.setState({ links: deviceCache });
                });
                device.on("closed", () => {
                    if (deviceCache.indexOf(device) < 0) return;
                    deviceCache = deviceCache.filter(d => d.target != device.target);
                    this.setState({ links: deviceCache });
                });
            });
            this.calls.push(call);
            this.sockets.push(socket);
        }
    }
    socket;
    calls = [];
    async componentDidMount() {
        for (var s of this.sockets) await s.connect();
        for (var c of this.calls) c.join("live", false);
        window.addEventListener("resize", this._resizeWindow)
    }
    _resizeWindow = () => {
        this.setState({ resize: new Date().getTime() })
        window.removeEventListener("resize", this._resizeWindow)
    }
    componentWillUnmount() {
        for (var c of this.calls) c.close()
        for (var s of this.sockets) s.close()
        this.calls.length = 0;
        this.sockets.length = 0;
    }
    render() {
        var w = "100%";
        if (this.state && this.state.links) w = 100 / Math.ceil(Math.sqrt(this.state.links.length)) + "%";
        return <div className="vbox rela" ref="root">
            <div className="vbox" style={{ left: 10, top: 10, right: 10, bottom: 10, position: "absolute", alignContent: "stretch", flexWrap: "wrap" }}>
                {this.state && this.state.links.map((item, i) => <div
                    key={item.target} className="fbox rela"
                    style={{ width: w, minWidth: w, boxSizing: "border-box", border: "1px solid rgba(0,0,0,0)" }}>
                    <RtcLinkView link={item} />
                </div>)}
            </div>
        </div>
    }
}

/**
 * 创建屏幕分享页
 */
export class CreateLivePage extends React.Component {
    handleStart = () => {
        if (!this.refs.idinput.value) return;
        this.setState({ id: this.refs.idinput.value, type: this.refs.typeinput.value });
    };
    render() {
        if (this.state && this.state.id) return <Redirect to={`/live/${encodeURIComponent(this.state.id)}/${encodeURIComponent(this.state.type || "")}`} />
        return <div className="container-fluid">
            <div className="jumbotron jumbotron-fluid" style={{ background: "none" }}>
                <h1 className="display-4">播放URL</h1>
                <p className="lead">请输入URL</p>
                <hr className="my-4" />
                <div className="input-group mb-3" style={{ maxWidth: 480 }}>
                    <input type="text" ref="idinput" className="form-control" placeholder="URL" aria-label="URL" aria-describedby="button-addon2" />
                    <input type="text" style={{ width: 80, flex: "none" }} defaultValue="tcp" ref="typeinput" className="form-control" placeholder="选项" aria-label="选项" aria-describedby="button-addon2" />
                    <div className="input-group-append">
                        <button className="btn btn-outline-secondary" onClick={this.handleStart} type="button" id="button-addon2">开始</button>
                    </div>
                </div>
            </div>
        </div>;
    }
}
