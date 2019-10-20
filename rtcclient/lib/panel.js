import React from "react";
import PropTypes from 'prop-types';


function checkUrl(url) {
    if (!url || !url.length || url.charAt(0) != '/') throw new Error("url应该以“/”开头");
    if (url[url.length - 1] == "/") throw new Error("url不能以“/”结尾");
}


//匹配url，判断par是不是chi的父目录
function isParentUrl(par, chi) {
    return par.length < chi.length
            && chi.charAt(par.length) == "/"
            && chi.substr(0, par.length) == par;
}

//匹配url，判断par是不是chi的父目录，或者是同一个目录
function matchUrl(par, chi) {
    return chi == par || isParentUrl(par, chi);
}

//常规状态
class PanelNormal {
    _url = null;            //当前url

    constructor(url) {
        this._url = url;
    }

    isHidding() {
        return false;
    }

    isShowing(url) {
        return matchUrl(url, this._url);
    }

    kill() { return true; }

    check() { }
}

//正在执行动画的状态
class PanelAnimating {
    _hide = null;           //正在隐藏中的上一个url
    _last = null;           //正在进行的动画。应该有kill函数用于中止动画
    _url = null;            //当前url

    constructor(url, hide, last) {
        this._url = url;
        this._hide = hide;
        this._last = last;
    }

    isHidding(url) {
        return matchUrl(url, this._hide);
    }

    isShowing(url) {
        return matchUrl(url, this._url);
    }

    kill() {
        if (this._last) {
            if (this._last instanceof Function) this._last();
            else this._last.kill();
        }
        return true;
    }

    check() {  }
}

//正在查找需要隐藏和显示组件的状态
class PanelFinding {
    _hide = null;           //正在隐藏中的上一个url
    _animation = null;      //动画回调缓存
    _url =  null;
    
    _showing = null;        //找到的即将被显示的组件
    _hiding = null;         //找到的即将被隐藏的组件

    _cross = null;          //_url和_hide的公共目录
    
    constructor(url, hide, ani) {
        this._url = url;
        this._hide = hide;
        this._animation = ani;

        //计算公共目录
        var index = 0, splitChar = "/".charCodeAt(0);
        var surl = url + "/";
        var shide = hide + "/";
        for (var i = 0; i < surl.length && i < shide.length; ++i) {
            var code = surl.charCodeAt(i);
            if (code != shide.charCodeAt(i)) break;
            if (code == splitChar) index = i;
        }
        this._cross = surl.substr(0, index + 1);
    }

    isHidding(url, panel) {
        if (matchUrl(url, this._hide)) {
            if (url.indexOf("/", this._cross.length) < 0) {
                if (this._hiding) throw new Error(url + "被重复定义，请保证url的唯一性");
                this._hiding = panel;
            }
            return true;
        }
        return false;
    }

    isShowing(url, panel) {
        if (matchUrl(url, this._url)) {
            if (!matchUrl(url, this._hide) && url.indexOf("/", this._cross.length) < 0) {
                if (this._showing) throw new Error(url + "被重复定义，请保证url的唯一性");
                this._showing = panel;
            };
            return true;
        }
        return false;
    }

    check(_onchange) {
        if (this._hiding != this._showing) {
            var state = this; //缓存当前状态
            var waiting = true;//确认回调执行顺序
            var last = this._animation(this._hiding && this._hiding.getRoot()
                , this._showing && this._showing.getRoot()
                , () => {
                    _onchange(state, new PanelNormal(this._url)); //当前state是否有效，由_onchange里面的逻辑保障
                    waiting = false;
                });
            if (waiting) { //如果回调没有被执行，代表进入了动画模式
                state = new PanelAnimating(this._url, this._hide, last);
                _onchange(this, state);
            }
        }
        else _onchange(this, new PanelNormal(this._url));
    }

    kill() {
        return false;
    }
}

/**
 * url状态切换
 */
class PanelState {
    state = null;

    constructor(url) {
        this.state = new PanelNormal(url);
    }

    /**
     * 跳转url
     * @param {String} url 要跳转的url
     * @param {Function} ani 跳转动画回调，参数为(被隐藏的组件，被显示的组件，动画完成后的回调)
     */
    go(url, ani) {
        if (url == this._url) return;
        checkUrl(url);
        if (!ani) ani = this.defaultAnimation;
        if (!ani) throw new Error("请配置默认的过渡动画");
        //if (!this.state.kill()) throw new Error("在上一次跳转完成前不能继续跳转");
        this.state.kill();
        this._setState(this.state, new PanelFinding(url, this.state._url, ani));
        if (this._onurlchange) this._onurlchange(url);
    }

    /**
     * 获得当前URL
     */
    getUrl() { return this.state._url; }

    //设置状态
    _setState = (oldState, newState) => {
        if (newState == null
            || oldState != this.state
            || oldState == newState) return;
        this.state = newState;
        if (this._onchange) this._onchange();
    };

    //检查组件显示隐藏的返回结果
    _checkGoing() {
        this.state.check(this._setState);
    }

    //判断组件是否应该被显示
    _match(panel, props_url) {
        var url = panel.context.url + "/" + props_url;
        if (this.state.isShowing(url, panel)) return 1;
        else if (this.state.isHidding(url, panel)) return 2;
    }

    /**
     * 默认的切换动画（淡入淡出）
     */
    defaultAnimation = (toHide, toShow, done) => done;
}

/**
 * 指定url的承载对象
 */
export class Panel extends React.Component {
    componentWillMount() {
        this.setState({
            match: this.context.panel._match(this, this.props.url)
        });
    }

    getRoot() {
        if (this.props.root) {
            if (this.props.root instanceof Function) return this.props.root();
            return this.props.root;
        } 
        return this.refs.root && this.refs.root.refs.root;
    }

    getChildContext() {
        return { 
            panel: this.context.panel,
            url: this.context.url + "/" + this.props.url
        };
    }

    componentWillReceiveProps(props) {
        this.setState({
            match: this.context.panel._match(this, props.url)
        });
    }

    render() {
        if (!this.state.match) return null;
        if (!this.props.component) {
            var children = this.props.children;
            return children ? React.Children.only(children) : null;
        }
        else {
            var Component = this.props.component;
            return <Component ref="root" panel={this.context.panel} data={this.props.data} />
        }
    }
}

Panel.contextTypes = {
    panel: PropTypes.object,
    url: PropTypes.string
};

Panel.childContextTypes = {
    panel: PropTypes.object,
    url: PropTypes.string
};

/**
 * url筛选的Context
 */
export class PanelContext extends React.Component {
    constructor(props) {
        super(props);
        var url = props.url || "/";
        checkUrl(url);
        this.panel = new PanelState(url);
        if (props.animation) this.panel.defaultAnimation = props.animation;
        this.panel._onchange = this.handlePanelChange;
        this.panel._onurlchange = this.handleUrlChange;
    }
    
    handlePanelChange = () => {
        this.setState({
            url: this.panel.url,
            version: ++this.version
        });
    };

    handleUrlChange = (url) => {
        if (this.props.onChange) this.props.onChange(url);
    };

    panel = null;
    version = 0;

    getChildContext() {
        return {
            panel: this.panel,
            url: ""
        };
    }

    componentDidUpdate() {
        this.panel._checkGoing();
    }

    render() {
        var children = this.props.children;
        return children ? React.Children.only(children) : null;
    }
}

PanelContext.childContextTypes = {
    panel: PropTypes.object,
    url: PropTypes.string
};
