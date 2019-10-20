/**
 * 一个css3过度和动画的简单封装，支持的尺寸单位是rem
 */

//判断当前浏览器的prefix
var prefix = "";
if (/MSIE/i.test(navigator.userAgent)) prefix = "-ms-"; //IE10也许不需要
else if (/AppleWebkit/i.test(navigator.userAgent)) prefix = "-webkit-";
else if (/Gecko\/\d+/i.test(navigator.userAgent)) prefix = "-moz-";
else if (/OPR/.test(navigator.userAgent)) prefix = "-o-"; //貌似现在的opera都支持-webkit-了。

//检查前缀
var fix = { "animation": 1, "transform": 1, "perspective": 1, "transition": 1 };
var _cssLower = function (n) {
    return n.replace(/[A-Z]/g, function (o) { return "-" + o.toLowerCase(); });
};
var p = prefix ? function (prop) {
    prop = _cssLower(prop);
    if (fix[prop.split('-')[0]]) return prefix + prop;
    return prop;
} : function (prop) { return _cssLower(prop); };

//css3动画
var _Keyframes = function () {
    this._list = {};
    this._css = {};
};
//添加动画定义
_Keyframes.prototype.add = function (key, process) {
    this._list[key] = process; //应该做一些简单的输入检查
    return this;
};
//添加普通css定义
_Keyframes.prototype.css = function (key, values) {
    this._css[key] = values;
    return this;
};
//输出到css标签
//输出单个css块
var createCssItem = function (prop) {
    var propstr = "";
    for (var i in prop) {
        propstr += p(i) + ":" + prop[i] + ";";
    }
    return propstr;
}
//输出一个动画定义
var createSteps = function (key, steps) {
    var stepstr = "";
    for (var i in steps) {
        stepstr += "\t" + i + "{" + createCssItem(steps[i]) + "}\r\n";
    }
    return "@" + prefix + "keyframes " + key + "{\r\n" + stepstr + "}\r\n";
}
//输出所有的css到一个style标签
var createStyleElement = function (csstxt, id) {
    var css = document.createElement("style");
    css.type = "text/css";
    if (id) css.id = id;
    document.getElementsByTagName("head")[0].appendChild(css);
    if (css.styleSheet) css.styleSheet.cssText = csstxt;
    else (function (e) {
        css.childNodes.length > 0
            ? css.firstChild.nodeValue !== e.nodeValue && css.replaceChild(e, css.firstChild)
            : css.appendChild(e);
    })(document.createTextNode(csstxt));
}
_Keyframes.prototype.toString = function () {
    var _cssLines = "";
    for (var i in this._list) {
        _cssLines += createSteps(i, this._list[i]);
    }
    for (var i in this._css) {
        _cssLines += i + "{" + createCssItem(this._css[i]) + "}\r\n";
    }
    return _cssLines;
};
//输出到css标签
_Keyframes.prototype.flush = function (id) {
    var _cssLines = this.toString();
    this._list = {};
    this._css = {};
    createStyleElement(_cssLines, id);
    return this;
};

//css3 matrix，还需要改进，设置子属性的时候能读取父属性就好了
var _Matrix = function () {
    this._value = "";
};
_Matrix.prototype.matrix = function (a, b, c, d, x, y) {
    this._value = 'matrix(' + [a, b, c, d, x, y].join() + ")";
    return this;
};
_Matrix.prototype.matrix3d = function (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) {
    this._value = 'matrix3d(' + [a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p].join() + ")";
    return this;
};
var _checkMatrixType = function (obj) {
    if (typeof obj != "object") obj = {};
    return obj;
};
var _setMatrixItem = function (key, value) {
    var m = _checkMatrixType(this._value);
    if (m[key] != value) m[key] = value;
    if (this._value != m) this._value = m;
};
var _setMatrixItemPart = function (key, subkey, value) {
    var m = _checkMatrixType(this._value);
    var sm = _checkMatrixType(m[key]);
    if (sm[subkey] != value) sm[subkey] = value;
    if (m[key] != sm) m[key] = sm;
    if (this._value != m) this._value = m;
};

var _checkMatrixValue = function (v, unit) {
    if ((typeof v != "string") && unit) return v + unit();
    return v;
};
//maxtrix 的函数
var matrixKeys = {};
var _defineMatrixMethodOnce = function (key, defaultValue, afterFix, unit) {
    var newName = key + afterFix;
    matrixKeys[newName] = defaultValue;
    _Matrix.prototype[newName] = function (v) {
        if (!arguments.length) {
            if (typeof this._value == "string") return defaultValue;
            if (typeof this._value[key] == "string") return defaultValue;
            if (newName in this._value[key]) return this._value[key][newName];
            return defaultValue;
        }
        _setMatrixItemPart.call(this, key, newName, _checkMatrixValue(v, unit));
        return this;
    };
};
var _defineMatrixMethod = function (key, defaultValue, unit, no3d, nounitcount3d) {
    if (!unit) unit = () =>  "";
    matrixKeys[key] = defaultValue;
    _Matrix.prototype[key] = function () {
        if (!arguments.length) {
            if (typeof this._value == "string") return defaultValue;
            if (key in this._value) return this._value[key];
            return defaultValue;
        }
        var args = [];
        for (var i = 0; i < arguments.length; ++i) args.push(_checkMatrixValue(arguments[i], unit));
        if (args.length == 1) {
            if (typeof args[0] == "string") args = args[0].split(",");
            if (args.length == 1 && key != "rotate") args.push(args[0]);
        }
        _setMatrixItem.call(this, key, args.join());
        return this;
    };
    if (!no3d) {
        var n3d = key + n3d;
        if (!nounitcount3d) nounitcount3d = 0;
        matrixKeys[n3d] = defaultValue;
        _Matrix.prototype[n3d] = function () {
            if (!arguments.length) {
                if (typeof this._value == "string") return defaultValue;
                if (n3d in this._value) return this._value[n3d];
                return defaultValue;
            }
            var args = [];
            for (var i = 0; i < arguments.length; ++i) args.push(i < nounitcount3d ? v : _checkMatrixValue(arguments[i], unit));
            _setMatrixItem.call(this, n3d, args.join());
            return this;
        };
        _defineMatrixMethodOnce(key, defaultValue, "Z", unit);
    }
    _defineMatrixMethodOnce(key, defaultValue, "X", unit);
    _defineMatrixMethodOnce(key, defaultValue, "Y", unit);
};
_defineMatrixMethod("translate", 0, () => jMove.sizeUnit());
_defineMatrixMethod("scale", 1);
_defineMatrixMethod("rotate", 0, () => "deg", 0, 3);
_defineMatrixMethod("skew", 0, () => "deg", 1);
//变成css的样式
_Matrix.prototype.toString = function () {
    if (typeof this._value == "string") return this._value;
    var ret = [];
    for (var i in this._value) {
        var oi = this._value[i];
        if (typeof oi == 'string') ret.push(i + "(" + oi + ")");
        else {
            var part = [];
            for (var j in oi) part.push(j + "(" + oi[j] + ")");
            ret.push(part.join(" "));
        }
    }
    return ret.join(" ");
};
//解析css样式
_Matrix.prototype.fromString = function (str) {
    if (!str) return this;
    var matches = str.match(/[a-zA-Z0-9]+\([^\)]+\)/ig);
    if (!matches) return this;
    for (var i = 0; i < matches.length; ++i) {
        var mi = /([a-zA-Z0-9]+)\(([^\)]+)\)/ig.exec(matches[i]);
        var foo = mi[1];
        var vals = mi[2].split(',');
        for (var j = 0; j < vals.length; ++j) {
            vals[j] = vals[j].replace(/^(\s+)/, "").replace(/(\s+)$/, "");
        }
        if (foo in this) this[foo].apply(this, vals);
    }
    return this;
};
//todo: get Value
_Matrix.prototype.get = function (key) {
    for (var sk in this._value) {
        if (sk == key) return this._value[key];
        if (key.indexOf(sk) == 0) return this._value[sk][key];
    }
    return 0;
};
//应用到对象
_Matrix.prototype.flush = function (node) {
    if (!node) node = this._node;
    if (!node) return;
    jMove.css(node, "transform", this.toString());
    return this;
};

//遍历对象
var _each = function (o, callback) {
    if (o == null || o == undefined) return;
    if (!("length" in o)) callback(0, o);
    else {
        for (var i = 0; i < o.length; ++i) _each(o[i], callback);
    }
};

//输出的对象
function jMove(node) {
    return new _MoveGroup(node);
};
jMove.sizeUnit = () => "rem";
jMove.style = createStyleElement;
jMove.prefix = function () { return prefix; };
jMove.each = _each;
//变形
jMove.matrix = function () {
    return new _Matrix();
};

//css3动画定义。如果传入了key与steps，那么这个动画定义将被立刻flush到style标签中
jMove.keyframes = function (key, steps) {
    var fr = new _Keyframes();
    if (key && steps) {
        fr.add(key, steps);
        fr.flush();
    }
    return fr;
};
//设置css值
jMove.cssName = p;
var _cssMap = {
    x: 'translateX',
    y: 'translateY',
    o: 'opacity',
    s: 'scale'
};
var _cssUnits = {
    left: () => jMove.sizeUnit(),
    top: () => jMove.sizeUnit(),
    width: () => jMove.sizeUnit(),
    height: () => jMove.sizeUnit(),
    "border-width": () => jMove.sizeUnit()
};
var _cssName = function (n) {
    if (n in _cssMap) return _cssMap[n];
    return n;
};
var _getStyle = function (obj, key) {
    var cur = window.getComputedStyle ? getComputedStyle(obj) : obj.currentStyle || obj.style;
    if (cur) {
        if (cur.getPropertyValue) return parseFloat(cur.getPropertyValue(key)) || 0;
        return parseFloat(obj.style[key]) || 0;
    }
    return 0;
};
jMove.css = function (e, key, value) {
    var isObj = (typeof key == "object");
    var noVal = !isObj && arguments.length < 3;
    var ret;
    _each(e, function (i, o) {
        if (isObj) {
            var tr;
            for (var iik in key) {
                var ik = _cssName(iik);
                if (ik in matrixKeys) {
                    if (!tr) tr = new _Matrix().fromString(o.style.getPropertyValue(p("transform")));
                    tr[ik](key[iik]);
                } else {
                    var sk = p(ik);
                    var sv = key[iik];
                    if (_cssUnits[sk] && typeof sv == "number") sv = sv + _cssUnits[sk]();
                    o.style.setProperty(sk, sv, "");
                }
            }
            if (tr) tr.flush(o);
        } else {
            key = _cssName(key);
            if (key in matrixKeys) {
                var ma = new _Matrix().fromString(o.style.getPropertyValue(p("transform")));
                if (noVal) {
                    ret = ma.get(key);
                    return ret;
                }
                ma[key](value).flush(o);
            } else {
                var sk = p(key);
                if (noVal) return ret = _getStyle(o, sk);
                if (_cssUnits[sk] && typeof value == "number") value = value + _cssUnits[sk]();
                o.style.setProperty(sk, value, "");
            }
        }
    });
    return ret || jMove;
};
//预定义的time functions
var time_funcs = {
    in$: 'ease-in',
    out: 'ease-out',
    inOut: 'ease-in-out',
    linear: 'linear',
    ease: 'ease',
    snap: 'cubic-bezier(0,1,.5,1)',
    quadIn: 'cubic-bezier(0.550,0.085,0.680,0.530)',
    quadOut: 'cubic-bezier(0.250,0.460,0.450,0.940)',
    quadInOut: 'cubic-bezier(0.455,0.030,0.515,0.955)',
    cubicIn: 'cubic-bezier(0.550,0.055,0.675,0.190)',
    cubicOut: 'cubic-bezier(0.215,0.610,0.355,1.000)',
    cubicInOut: 'cubic-bezier(0.645,0.045,0.355,1.000)',
    quartIn: 'cubic-bezier(0.895,0.030,0.685,0.220)',
    quartOut: 'cubic-bezier(0.165,0.840,0.440,1.000)',
    quartInOut: 'cubic-bezier(0.770,0.000,0.175,1.000)',
    quintIn: 'cubic-bezier(0.755,0.050,0.855,0.060)',
    quintOut: 'cubic-bezier(0.230,1.000,0.320,1.000)',
    quintInOut: 'cubic-bezier(0.860,0.000,0.070,1.000)',
    sineIn: 'cubic-bezier(0.470,0.000,0.745,0.715)',
    sineOut: 'cubic-bezier(0.390,0.575,0.565,1.000)',
    sineInOut: 'cubic-bezier(0.445,0.050,0.550,0.950)',
    expoIn: 'cubic-bezier(0.950,0.050,0.795,0.035)',
    expoOut: 'cubic-bezier(0.190,1.000,0.220,1.000)',
    expoInOut: 'cubic-bezier(1.000,0.000,0.000,1.000)',
    circIn: 'cubic-bezier(0.600,0.040,0.980,0.335)',
    circOut: 'cubic-bezier(0.075,0.820,0.165,1.000)',
    circInOut: 'cubic-bezier(0.785,0.135,0.150,0.860)',
    backIn: 'cubic-bezier(0.600,-0.280,0.735,0.045)',
    backOut: 'cubic-bezier(0.175,0.885,0.320,1.275)',
    backInOut: 'cubic-bezier(0.680,-0.550,0.265,1.550)'
};
//预定义的time functions
jMove.ease = time_funcs;

//css3动画默认属性
var aniDefauts = ['fadein', '0.4s', 'ease', '0s', '1', 'normal', 'both'];
//css3动画
jMove.animation = function (e, ani, state) {
    var aniList = ani.split(" ").filter(function (o) { return !!o; });
    if (aniList.length > 2 && (aniList[2] in time_funcs)) {
        aniList[2] = time_funcs[aniList[2]];
    }
    for (var i = aniList.length; i < aniDefauts.length; ++i) {
        aniList.push(aniDefauts[i]);
    }
    jMove.css(e, "animation", aniList.join(" "));

    if (state) jMove.css(e, "animation-play-state", state);
    return jMove;
};

//动画样式可用
jMove.animationReady = function (onready) {
    return jMove.transitionReady(onready);
};

//css3过渡
jMove.transition = function (e, trans) {
    //处理参数
    var arr = [];
    var onready = [];
    for (var i = 1; i < arguments.length; ++i) {
        var o = arguments[i];
        if (o instanceof Function) onready.push(o);
        else if (typeof o == "string") {
            var oarr = o.split(" ").filter(function (x) { return !!x; });
            if (oarr.length > 2 && (oarr[2] in time_funcs)) {
                oarr[2] = time_funcs[oarr[2]];
                o = oarr.join(" ");
            }
            arr.push(o);
        }
    }
    //回调
    if (onready.length) {
        jMove.transitionReady(function () {
            jMove.css(e, "transition", arr.join(","));
            onready.forEach(function (o) {
                o();
            });
        });
    } else {
        jMove.css(e, "transition", arr.join(","));
    }
    return jMove;
};

//过度样式可用
jMove.transitionReady = function (onready) {
    if (!onready) return jMove;
    var rq = window.requestAnimationFrame || window.webkitRequestAnimationFrame || window.oRequestAnimationFrame || window.mozRequestAnimationFrame || window.msRequestAnimationFrame;
    var isIe = /msie|trident/ig.test(navigator.userAgent);
    if (!rq) setTimeout(onready, 50);
    else if (isIe) rq(onready);
    else rq(function () { rq(onready); });
    return jMove;
};

//从当前状态移动到指定状态 settings = { delay: 0, ease: "ease", now: false }
var _to = function (settings, end) {
    var s = settings || {};
    if (typeof s == "function") {
        if (!end) end = s;
        s = {};
    } else if (typeof s == "string") {
        s = { ease: s };
    } else if (typeof s == "number") {
        s = { delay: s };
    }
    if (end) s.end = end;
    return s;
};
//从当前状态移动到指定状态 settings = { delay: 0, ease: "ease", now: false }
jMove.to = function (node, duration, to, settings, end) {
    var s = _to(settings, end);
    var start = function () {
        jMove.transition(node, "all " + duration + "s " + (s.ease || "ease") + " " + (s.delay || 0) + "s");
        jMove.css(node, to);
        if (s.end) setTimeout(s.end, ((s.delay || 0) + duration) * 1000);
    };
    if (s.now) start();
    else jMove.transitionReady(start);
    return jMove;
};
var _current_css = function (o, from) {
    var to = {};
    var tr;
    for (var k in from) {
        var ki = _cssName(k);
        if (ki in matrixKeys) {
            if (!tr) tr = {};
            tr[ki] = true;
            continue;
        }
        var sk = p(ki);
        var pv = o.style.getPropertyValue(sk);
        to[ki] = pv;
    }
    if (tr) {
        var pv = new _Matrix().fromString(o.style.getPropertyValue(p("transform")));
        for (var k in tr) {
            to[k] = pv[k]();
        }
    }
    return to;
};
var _css_decode1 = function (o) {
    var ret = {};
    for (var k in o) ret[p(k)] = o[k];
    return ret;
};
//从某一个状态开始动画到当前状态 settings = { delay: 0, ease: "ease" }
jMove.from = function (node, duration, from, settings, end) {
    _each(node, function (i, o) {
        return jMove.fromTo(o, duration, from, _current_css(o, _css_decode1(from)), settings, end);
    });
};
//从指定状态移动到指定状态 settings = { delay: 0, ease: "ease" }
jMove.fromTo = function (node, duration, from, to, settings, end) {
    if (settings && settings.now) settings.now = false;
    jMove.transition(node, "none");
    jMove.css(node, from);
    return jMove.to(node, duration, to, settings, end);
};

//单独属性的动画支持
var _Move = function (node) {
    this._node = node;
    this._duration = 0;
    this._timeScale = 1;
};
//把属性key从当前状态移动到to状态 settings = { delay: 0, ease: "ease" }
_Move.prototype.to = function (duration, to, settings) {
    to = _css_decode1(to);
    var s = _to(settings);
    for (var key in to) {
        if (!this._map) {
            this._map = {};
            this._to = {};
        }

        var skey = _cssName(key);
        if (skey in matrixKeys) skey = "transform";
        s.duration = duration;
        this._map[skey] = s;

        this._to[key] = to[key];
        this._duration = Math.max(duration + (s.delay || 0), this._duration);
    }
    return this._duration;
};
//把属性key从from状态移动到当前状态 settings = { delay: 0, ease: "ease" }
_Move.prototype.from = function (duration, from, settings) {
    from = _css_decode1(from);
    var to = _current_css(this._node, from);
    if (this._to) {
        for (var i in to) {
            if (i in this._to) to[i] = this._to[i];
        }
    }
    return this.fromTo(duration, from, to, settings);
};
//把属性key从from状态移动到当前状态 settings = { delay: 0, ease: "ease" }
_Move.prototype.fromTo = function (duration, from, to, settings) {
    from = _css_decode1(from);
    for (var key in from) {
        if (!this._from) this._from = {};
        this._from[key] = from[key];
    }
    return this.to(duration, to, settings);
};
//时间缩放
_Move.prototype.timeScale = function (v) {
    if (!arguments.length) return this._timeScale;
    this._timeScale = v;
};
//输出
_Move.prototype.flush = function (now) {
    if (this._from) jMove.css(this._node, this._from);
    if (this._map) {
        var jtr = [];
        for (var k in this._map) {
            var s = this._map[k];
            var ease = s.ease || "ease";
            if (ease in time_funcs) ease = time_funcs[ease];
            jtr.push([p(k),
            s.duration * this._timeScale + "s",
                ease,
            (s.delay || 0) * this._timeScale + "s"].join(" "));
        }

        var me = this;
        var start = function () {
            jMove.css(me._node, me._to);
            jMove.css(me._node, "transition", jtr.join());
        };

        if (now) start();
        else jMove.transitionReady(start);
    }
};
//终止执行
_Move.prototype.kill = function (isend) {
    if (!isend) {
        //所有参与动画的css
        var allkeys = {};
        if (this._from) for (var k in this._from) allkeys[k] = true;
        if (this._to) for (var k in this._to) allkeys[k] = true;
        //处理transform
        var keys = {};
        for (var k in allkeys) {
            if (k in matrixKeys) keys["transform"] = true;
            else keys[k] = true;
        }
        //当前样式
        var cur = getComputedStyle(this._node);
        var setVal = {};

        //ie下background等属性无法得到
        if (/msie|trident/ig.test(navigator.userAgent)) {
            //对相关的属性进行索引
            var temp = {}, tall = {}, kmap = {};
            for (var k in keys) kmap[p(k)] = true;
            for (var sk = 0; sk < cur.length; ++sk) {
                var ck = cur.item(sk);
                if (kmap[ck]) temp[ck] = cur.getPropertyValue(ck);
                for (var pk in kmap) {
                    if (!ck.indexOf(pk)) {
                        if (!tall[pk]) tall[pk] = {};
                        tall[pk][ck] = cur.getPropertyValue(ck);
                    }
                }
            }
            //如果没有相关属性，就设置所有子属性
            for (var pk in kmap) {
                if (!(pk in temp)) {
                    if (tall[pk]) for (var spk in tall[pk]) setVal[spk] = tall[pk][spk];
                } else {
                    setVal[pk] = temp[pk];
                }
            }
        } else {
            for (var k in keys) {
                setVal[k] = cur.getPropertyValue(p(k));
            }
        }
        jMove.css(this._node, setVal);
    }

    jMove.transition(this._node, "none");
};

//单独属性的动画支持
var _MoveGroup = function (node) {
    this._list = [];
    this._timers = {};
    var me = this;
    _each(node, function (i, o) {
        me._list.push(new _Move(o));
    });
    this._timeScale = 1;
    jMove.transitionReady(function () {
        if (!me._flushed) me.flush();
    });
};
//把属性key从当前状态移动到to状态 settings = { delay: 0, ease: "ease" }
_MoveGroup.prototype.to = function (duration, to, settings) {
    this._list.forEach(function (o) { o.to(duration, to, settings); });
    return this;
};
//把属性key从from状态移动到当前状态 settings = { delay: 0, ease: "ease" }
_MoveGroup.prototype.from = function (duration, from, settings) {
    this._list.forEach(function (o) { o.from(duration, from, settings); });
    return this;
};
//把属性key从from状态移动到当前状态 settings = { delay: 0, ease: "ease" }
_MoveGroup.prototype.fromTo = function (duration, from, to, settings) {
    this._list.forEach(function (o) { o.fromTo(duration, from, to, settings); });
    return this;
};
//时间缩放
_MoveGroup.prototype.timeScale = function (v) {
    if (!arguments.length) return this._timeScale;
    this._list.forEach(function (o) { o.timeScale(v); });
    this._timeScale = v;
    return this;
};
//总长度，不受timeScale影响
_MoveGroup.prototype.duration = function () {
    var dur = 0;
    this._list.forEach(function (o) { dur = Math.max(o._duration, dur); });
    return dur; //不受timeScale影响
};
//回调函数
_MoveGroup.prototype.addCallback = function (callback, time) {
    if (!callback) return;
    if (arguments.length == 1) time = this.duration();
    if (!this._callback) this._callback = [];
    this._callback.push([callback, time]);
};
//输出
_MoveGroup.prototype.flush = function (end, now) {
    if (this._flushed) this.kill();
    this._flushed = true;
    this._list.forEach(function (o) { o.flush(now); });
    if (end) this.addCallback(end);
    var me = this;
    var timeout = function (foo, delay) {
        var timer = setTimeout(function () {
            delete me._timers[timer];
            foo();
        }, delay * me._timeScale * 1000);
        me._timers[timer] = [foo, delay];
    };
    if (this._callback) {
        for (var i = 0; i < this._callback.length; ++i) {
            timeout(this._callback[i][0], this._callback[i][1]);
        }
    }
    timeout(function () {
        me._list.forEach(function (o) { o.kill(1); });
    }, this.duration());
    return this;
};
//终止执行
_MoveGroup.prototype.kill = function (isend) {
    this._list.forEach(function (o) { o.kill(isend); });
    var calls = [];
    for (var timer in this._timers) {
        clearTimeout(timer);
        if (isend) calls.push(this._timers[timer]);
    }
    //按时间排序，顺序执行
    if (isend) {
        calls.sort(function (a, b) { return a[1] - b[1]; });
        for (var i = 0; i < calls.length; ++i) calls[i][0]();
    }
    this._timers = {};
};

export default jMove;
