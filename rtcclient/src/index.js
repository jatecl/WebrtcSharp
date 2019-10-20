import React from "react";
import ReactDOM from "react-dom";
import { HashRouter, Route, Link } from "react-router-dom";
import { ScreenSharePage, CreateScreenSharePage } from "./screenSharePage";
import { RtcCallPage, CreateCallPage } from "./rtcCallPage";
import { RtcLivePage, CreateLivePage } from "./rtcLivePage";

/**
 * 主页配置
 */
class StartUpPage extends React.Component {
    render() {
        return <HashRouter>
            <div className="vfull">
                <nav className="navbar navbar-expand-lg navbar-light bg-light">
                    <a className="navbar-brand" href="#">Webrtc测试</a>
                    <button className="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarSupportedContent"
                        aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                        <span className="navbar-toggler-icon"></span>
                    </button>

                    <div className="collapse navbar-collapse" id="navbarSupportedContent">
                        <ul className="navbar-nav mr-auto">
                            <li className="nav-item">
                                <Link className="nav-link" to="/">概述 <span className="sr-only">(current)</span></Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/call">视频通话</Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/share">屏幕分享</Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/live">播放rtsp视频</Link>
                            </li>
                        </ul>
                    </div>
                </nav>
                <div className="vbox">
                    <Route path="/" exact component={IndexPage} />
                    <Route path="/call" exact component={CreateCallPage} />
                    <Route path="/call/:id/:type" component={RtcCallPage} />
                    <Route path="/share" exact component={CreateScreenSharePage} />
                    <Route path="/share/:action/:id" component={ScreenSharePage} />
                    <Route path="/live" exact component={CreateLivePage} />
                    <Route path="/live/:id/:type" component={RtcLivePage} />
                </div>
            </div>
        </HashRouter>
    }
}

/**
 * 概述页
 */
class IndexPage extends React.Component {
    render() {
        return <div className="container-fluid">
            <div className="jumbotron jumbotron-fluid" style={{ background: "none" }}>
                <h1 className="display-4">WebRTC与协议</h1>
                <hr className="my-4" />
                <p className="lead">WebRTC与通话协议测试，包含了视频通话，屏幕共享，P2P无中心直播等内容</p>
            </div>
        </div>;
    }
}

ReactDOM.render(<StartUpPage />, document.getElementById("client_area"));