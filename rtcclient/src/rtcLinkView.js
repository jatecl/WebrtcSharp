import React from "react";

class VideoPlayer extends React.Component {
    componentDidMount() {
        this.setStream(this.props.stream);
    }
    componentWillReceiveProps(props) {
        this.setStream(props.stream);
    }
    srcObject;
    setStream(stream) {
        if (this.srcObject != stream) {
            this.srcObject = stream;
            this.refs.device.srcObject = stream;
        }
    }
    render() {
        return <video autoPlay muted={this.props.muted} controls ref="device" className="full" style={{ background: "#000" }} />;
    }
}

/**
 * 通话显示
 */
export class RtcLinkView extends React.Component {
    constructor(props) {
        super(props);
        this.state = { link: props.link, stream: this.refreshStream(props.link) };
        props.link.on("addtrack", this._onaddtrack);
    }
    componentWillReceiveProps(props) {
        this.setChannel(props.link);
    }
    setChannel(link) {
        if (this.state.link != link) {
            if (this.state.link) this.state.link.removeListener("addtrack", this._onaddtrack);
            if(link) link.on("addtrack", this._onaddtrack);
            this.setState({ link, stream: this.refreshStream(link) });
        }
    }
    refreshStream(link) {
        if (!link || !link.stream) return null;
        var stream = link.stream;
        if (stream.getVideoTracks().length == 0 && stream.getAudioTracks().length == 0) stream = null;
        return stream;
    }
    _onaddtrack = () => {
        this.setState({ stream: this.refreshStream(this.state.link) });
    };
    componentWillUnmount() {
        if (this.state.link) this.state.link.removeListener("addtrack", this._onaddtrack);
    }
    render() {
        return this.state && this.state.stream 
            ? <VideoPlayer stream={this.state.stream} /> 
            : <div className="full" style={{ background: "#000" }}><div className="center">Loading</div></div>
    }
}

/**
 * 本地视频显示
 */
export class RtcLocalView extends React.Component {
    constructor(props) {
        super(props);
        this.state = { media: props.media, stream: props.media && props.media.stream };
        if (props.media) props.media.on("changed", this._createMediaStream);
    }
    /**
     * 创建播放流
     */
    _createMediaStream = async () => {
        if (!this.state.media) return;
        try {
            let stream = new MediaStream();
            for (var key in this.state.media.all) {
                var track = await this.state.media.all[key].getTrack();
                if (track) stream.addTrack(track);
            }
            this.setState({ stream });
        }
        catch (e) {
            console.error(e);
        }
    };
    componentWillReceiveProps(props) {
        this.setMedia(props.media);
    }
    setMedia(media) {
        if (this.state.media != media) {
            if (this.state.media) this.state.media.removeListener("changed", this._createMediaStream);
            if (media) media.on("changed", this._createMediaStream);
            this.setState({ media, stream: media && media.stream });
        }
    }
    componentWillUnmount() {
        if (this.state.media) this.state.media.removeListener("changed", this._createMediaStream);
    }
    render() {
        return this.state && this.state.stream 
            ? <VideoPlayer stream={this.state.stream} muted={true} /> 
            : <div className="full" style={{ background: "#000" }}><div className="center">Loading</div></div>
    }
}