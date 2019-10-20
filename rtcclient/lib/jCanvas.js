/**
 * Created by Jat on 2015/9/17.
 */
/**
 * @property 类型方法
 */
var cls = {
    copy: function (obj) {
        if (arguments.length > 1) {
            for (var i = 1; i < arguments.length; ++i) {
                for (var k in arguments[i]) obj[k] = arguments[i][k];
            }
        }
        return obj;
    },
    or: function (a, b, c) {
        for (var i = 0; i < b.length; ++i) {
            var oi = a[b[i] + c];
            if (oi) return oi;
        }
    },
    //事件模版
    events: {
        /**
         * @method 注册事件
         * @param event_name 时间名称
         * @param callback 事件回调
         */
        on: function (event_name, callback) {
            if (!this._events) this._events = {};
            if (!this._events[event_name]) this._events[event_name] = [];
            this._events[event_name].push(callback);
            return this;
        },
        /**
         * @method 注销事件
         * @param event_name 事件名称
         * @param callback 要注销的回调。如果为空则注销该事件的所有回调。
         */
        off: function (event_name, callback) {
            if (!this._events) return this;
            var el = this._events[event_name];
            if (!el) return this;
            if (!callback || (el.length == 1 && el[0] == callback)) {
                this._events[event_name] = [];
                delete this._events[event_name];
            }
            else {
                for (var i = 0; i < el.length; ++i) {
                    if (el[i] == callback) {
                        el.splice(i, 1);
                        break;
                    }
                }
            }
            return this;
        },
        /**
         * @method 触发事件
         * @param event_name 触发的事件名称
         */
        trigger: function (event_name) {
            if (!this._events) return this;
            var el = this._events[event_name];
            if (!el || !el.length) return this;
            var args = [];
            for (var i = 1; i < arguments.length; ++i) {
                args.push(arguments[i]);
            }
            for (var i = 0; i < el.length; ++i) {
                el[i].apply(this, args);
            }
            return this;
        }
    },
    /**
     * @method 为对象添加属性支持（单方法，传参设置当前值，无参返回当前值）
     */
    createSetter: function (name, defaultValue, setFunction) {
        return function (v) {
            if (arguments.length) {
                if (!this._properties) this._properties = {};
                else if (v == this._properties[name]) return this;
                if (setFunction) setFunction.call(this, v, name);
                this._properties[name] = v;
                return this;
            }
            if (!this._properties || !(name in this._properties)) return defaultValue;
            return this._properties[name];
        };
    },
    /**
     * @method 为对象添加属性支持（单方法，传参设置当前值，无参返回当前值）
     */
    createMethod: function (classRef, name, defaultValue, setFunction) {
        classRef.prototype[name] = this.createSetter(name, defaultValue, setFunction);
        return classRef;
    },
    /**
     * @method 为对象添加事件支持
     */
    createEventSupport: function (classRef) {
        this.copy(classRef.prototype, this.events);
        return classRef;
    }
};

var jCanvas = {};
//shapeObj----------------------------------
//下列参数在设置了matrix后将无效（Opacity除外）
var shapeObjProperties = {
    scaleX: 1,
    scaleY: 1,
    rotate: 0,
    skewX: 0,
    skewY: 0,
    left: 0,
    top: 0,
    opacity: 1
};
var shapeObjFields = {
    zIndex: 0,
    shadowColor: null,
    shadowOffsetX: 0,
    shadowOffsetY: 0,
    shadowBlur: 0,
    compositeOperation: null
};

//呈现对象
var shapeObj = (function () {
    var t = function () { };
    //事件支持
    cls.createEventSupport(t);
    //属性
    var _setFunction = function (v, name) {
        this._ischanged = true;
        if (name) {
            this.trigger("changed", name, v);
            this.trigger(name, v);
        }
        if (this._parent) this._parent.triggerChanged();
    };
    t.prototype.triggerChanged = _setFunction;
    for (var i in shapeObjFields) cls.createMethod(t, i, shapeObjFields[i], _setFunction);

    //matrix
    //属性
    var _setMatrix = function (v, name) {
        this.triggerChanged(v, name);
        if (!this._hasTransform) this._hasTransform = true;
    };
    for (var i in shapeObjProperties) cls.createMethod(t, i, shapeObjProperties[i], _setMatrix);
    //scale
    t.prototype.scale = function (v) {
        if (!arguments.length) return this.scaleX();
        this.scaleX(v);
        this.scaleY(v);
        this.trigger("scale", v);
        return this;
    };
    //skew
    t.prototype.skew = function (v) {
        if (!arguments.length) return this.skewX();
        this.skewX(v);
        this.skewY(v);
        this.trigger("skew", v);
        return this;
    };
    cls.createMethod(t, 'matrix', null, _setMatrix);

    return t;
})();

//容器
var asGroup = (function () {
    //添加
    var append = function () {
        for (var i = 0; i < arguments.length; ++i) {
            var o = arguments[i];
            if (o._parent == this) continue;
            if (o._parent) o.remove();
            o._parent = this;
            if (!this._children) this._children = {};
            this._children[o._id] = o;
            o.trigger("append", o);
            this.trigger("appendChild", o);
        }
        this.triggerChanged();
        return this;
    };
    //删除
    var removeChildren = function () {
        if (!this._children) return this;
        for (var i = 0; i < arguments.length; ++i) {
            var o = arguments[i];
            if (o._parent != this) continue;
            delete this._children[o._id];
            o.trigger("remove", o);
            o._parent = undefined;
            this.trigger("removeChild", o);
        }
        this.triggerChanged();
        return this;
    };
    var removeAllChildren = function () {
        if (!this._children) return this;
        var list = [];
        for (var i in this._children) list.push(this._children[i]);
        var t = this;
        list.forEach(function (o) { t.removeChildren(o); });
        return this;
    };
    //返回方法
    return function (classRef) {
        classRef.prototype.append = append;
        classRef.prototype.removeChildren = removeChildren;
        classRef.prototype.removeAllChildren = removeAllChildren;
    };
})();

//新建类型
jCanvas.createShapeType = (function () {
    var _idCount = 0;
    var t = function () {
        this._id = ++_idCount;
    };
    cls.copy(t.prototype, shapeObj.prototype);
    //移除
    t.prototype.remove = function () {
        if (this._parent) this._parent.removeChildren(this);
        return this;
    };
    //添加到
    t.prototype.appendTo = function (par) {
        par.append(this);
        return this;
    };
    //新建类型
    return function (name, attrs, asgroup) {
        //创建类型
        var ref = function (prop) {
            t.call(this);
            for (var i in prop) {
                this[i](prop[i]);
            }
        };
        cls.copy(ref.prototype, t.prototype);
        if (asgroup) asGroup(ref);
        //创建类型方法
        if (attrs) for (var i in attrs) {
            cls.createMethod(ref, i, attrs[i], ref.prototype.triggerChanged);
        }
        //返回类型
        ref.prototype.name = function () {
            return name;
        };
        //得到当前所有值
        ref.prototype.values = function () {
            return cls.copy({}, shapeObjProperties, shapeObjFields, attrs, this._properties);
        };
        //克隆对像
        ref.prototype.clone = function (par) {
            var pops = this.values();
            var obj = new ref(pops);
            if (par) par.append(obj);
            var children = this._children;
            if (children) {
                for (var c in children) {
                    if (children.hasOwnProperty(c)) {
                        children[c].clone(obj);
                    }
                }
            }
            return obj;
        };
        //返回工厂方法
        jCanvas[name] = function (prop) {
            return new ref(prop);
        };
    };
})();

//字体枚举
jCanvas.fontWeight = {
    normal: 'normal',
    bold: 'bold',
    bolder: 'bolder',
    lighter: 'lighter'
};
jCanvas.fontStyle = {
    normal: 'normal',
    italic: 'italic',
    oblique: 'oblique'
};
jCanvas.fontVariant = {
    normal: 'normal',
    smallCaps: 'small-caps',
    'small-caps': 'small-caps'
};
jCanvas.textBaseline = {
    top: "top",
    bottom: "bottom",
    middle: "middle",
    alphabetic: "alphabetic",
    hanging: "hanging",
    ideographic: "ideographic"
};
jCanvas.textAlign = {
    start: "start",
    end: "end",
    center: "center",
    left: "left",
    right: "right"
};

//常用的属性
//边框和填充
var _fills = {
    fill: null,
    strokeWidth: 0,
    lineCap: "round",
    lineJoin: "round",
    miterLimit: 0,
    stroke: null
};

//线段
jCanvas.createShapeType('line', cls.copy({
    x1: 0, y1: 0,
    x2: 0, y2: 0
}, _fills));

//路径
jCanvas.createShapeType('path', cls.copy({
    path: null
}, _fills));
//圆形
jCanvas.createShapeType('circle', cls.copy({
    cx: 0,
    cy: 0,
    r: 0
}, _fills));
//矩形
jCanvas.createShapeType('rect', cls.copy({
    x: 0,
    y: 0,
    width: 0,
    height: 0
}, _fills));
//多边形
jCanvas.createShapeType('polygon', cls.copy({
    path: null,
    close: false
}, _fills));
//图片
jCanvas.createShapeType('image', {
    src: null,
    width: 0,
    height: 0,
    x: 0,
    y: 0,
    clipw: 0,
    cliph: 0,
    cx: 0,
    cy: 0
});

//组合
jCanvas.createShapeType('group', null, true);

//文字
jCanvas.createShapeType("text", cls.copy({
    text: null,
    fontSize: null,
    fontWeight: null,
    fontStyle: null,
    fontFamily: null,
    lineHeight: null,
    fontVariant: null,
    textAlign: null,
    textBaseline: null,
    width: null
}, _fills));


//zIndex排序
var sortedChildren = function () {
    this._l = [];
    this._c = {};
};
sortedChildren.prototype.append = function (o, i) {
    var oi = i || o.zIndex();
    var a = this._c[oi];
    if (!a) {
        a = { i: oi, l: [] };
        this._c[oi] = a;
        this._l.push(a);
        this._l.sort(function (a, b) { return a.i - b.i; });
    }
    a.l.push(o);
};
sortedChildren.prototype.remove = function (o, i) {
    var oi = i || o.zIndex();
    var a = this._c[oi];
    if (!a) return;
    var ix = a.l.indexOf(o);
    if (ix >= 0) {
        a.l.splice(ix, 1);
        if (!a.l.length) {
            delete this._c[oi];
            ix = this._l.indexOf(a);
            if (ix >= 0) this._l.splice(ix, 1);
        }
    }
};
sortedChildren.prototype.each = function (callback, e) {
    for (var i = 0; i < this._l.length; ++i) {
        var l = this._l[i].l;
        for (var j = 0; j < l.length; ++j) callback(l[j], e);
    }
};

//定时器实现
var make_timer = (function () {
    var aframe = cls.or(window, 'r,webkitR,msR,mozR'.split(','), 'equestAnimationFrame');
    var cframe = cls.or(window, 'c,webkitC,msC,mozC'.split(','), 'ancelAnimationFrame') || clearInterval;
    //aframe = null;
    //cframe = clearInterval;
    //自动刷新
    var _startTimer = function () {
        if (this._killTimer) return;

        //*
        //使用多媒体定时器
        var timer, that = this;
        var ticker = function () {
            if (!that._killTimer) return;
            if (that._killTimer) {
                that._objectNumber = 0;
                if (that._ticker) that._ticker(); //调用刷新
            }
            if (aframe) timer = aframe(ticker);
        };
        if (!aframe) timer = setInterval(ticker, 16);
        this._killTimer = function () {
            if (timer) cframe(timer);
            this._killTimer = undefined;
        };
        //必须在_killTimer后面调用
        ticker();
    };
    //通知上层变化
    var triggerChanged = function () {
        this._ischanged = true;
        this._killNumber = 0;
        this._startTimer();
    };

    return function (t) {
        t.prototype.triggerChanged = triggerChanged;
        t.prototype._startTimer = _startTimer;
    }
})();

//画布实现
jCanvas.stage = (function () {
    //数据更改事件
    var _zChanged = function (v) {
        var p = this._parent._sortedChs;
        p.remove(this);
        p.append(this, v);
    };
    //删除显示
    var _removeSorted = function (o) {
        o.off("zIndex", _zChanged);
        o.off("appendChild", _appendChild);
        o.off("remove", _remove);

        if (o._children) {
            if (o._sortedChs) delete o._sortedChs;
            for (var i in o._children) _removeSorted(o._children[i]);
        }
    };
    var _remove = function () {
        var p = this._parent._sortedChs;
        if (p) p.remove(this);
        _removeSorted(this);
    };
    //添加显示
    var _appendChild = function (o) {
        var p = o._parent;
        if (!p._sortedChs) p._sortedChs = new sortedChildren();
        p._sortedChs.append(o);

        o.on("zIndex", _zChanged);
        o.on("appendChild", _appendChild);
        o.on("remove", _remove);

        var e = o._children;
        if (!e) return;
        for (var i in e) _appendChild(e[i]);
    };
    //canvas类的构造函数
    var t = function (ele) {
        if (!(this instanceof t)) return new t(ele);
        if (!ele) ele = document.createElement('canvas');
        this.context = ele.getContext('2d');
        this.canvas = ele;
        this._killNumber = 0;

        //添加显示
        this.on("appendChild", _appendChild);
    };
    cls.createEventSupport(t);
    asGroup(t);
    //添加到div
    t.prototype.appendTo = function (par) {
        if (par.append) par.append(this.canvas);
        else par.appendChild(this.canvas);
        return this;
    };
    //呈现线条和填充
    //todo：支持渐变填充和图片填充
    var _strokeJ = function (ctx, prop) {
        var sw = prop.strokeWidth(), s = prop.stroke();
        if (sw && s) {
            var cap = prop.lineCap();
            var join = prop.lineJoin();
            var miter = prop.miterLimit() || 0;
            if (cap && cap != ctx.lineCap) ctx.lineCap = cap;
            if (join && join != ctx.lineJoin) ctx.lineJoin = join;
            if (miter != ctx.miterLimit) ctx.miterLimit = miter;
            if (sw != ctx.lineWidth) ctx.lineWidth = sw;
            if (s != ctx.strokeStyle) ctx.strokeStyle = s;
            return true;
        }
        return false;
    };
    var _fillJ = function (ctx, prop) {
        var f = prop.fill();
        if (f) {
            if (f != ctx.fillStyle) ctx.fillStyle = f;
            return true;
        }
        return false;
    };
    var _strokeFill = function (ctx, prop) {
        if (_fillJ(ctx, prop)) ctx.fill();
        if (_strokeJ(ctx, prop)) ctx.stroke();
    };
    //绘制
    var draw = {
        //绘制线条
        line: function (ctx, prop) {
            if (prop.x1() == prop.x2() && prop.y1() == prop.y2()) return;
            ctx.beginPath();
            ctx.moveTo(prop.x1(), prop.y1());
            ctx.lineTo(prop.x2(), prop.y2());
            _strokeFill(ctx, prop);
        },
        //路径
        path: (function () {
            //path解析
            var pathDef = {
                M: 2,
                L: 2,
                C: 6,
                S: 6,
                Q: 4,
                T: 4,
                A: 5,
                Z: 0
            };

            var canvasPathDef = {
                M: 'moveTo',
                L: "lineTo",
                C: 'bezierCurveTo',
                S: 'bezierCurveTo',
                Q: 'quadraticCurveTo',
                T: 'quadraticCurveTo',
                A: 'arcTo',
                Z: 'closePath'
            };

            //路径解析,todo:还没有处理HV命令，没有处理小写字母
            function canvasPath(str) {
                var com = [];
                if (!str) return com;
                var idx = 0;
                var readCmd = function () {
                    while (idx < str.length) {
                        var cmd = str.charCodeAt(idx);
                        if ((cmd >= 65 && cmd <= 90) || (cmd >= 97 && cmd <= 122)) {
                            var scmd = str.charAt(idx);
                            ++idx;
                            return scmd;
                        }
                        ++idx;
                    }
                };
                var readNumber = function () {
                    var num = "";
                    while (idx < str.length) {
                        var cmd = str.charCodeAt(idx);
                        if ((cmd >= 45 && cmd <= 46) || (cmd >= 48 && cmd <= 57)) {
                            num += str.charAt(idx);
                        } else if (num) {
                            return parseFloat(num);
                        }
                        ++idx;
                    }
                };
                while (true) {
                    var cmd = readCmd();
                    if (!cmd) break;
                    var ucmd = cmd.toUpperCase();

                    if (ucmd != cmd) throw new Error("还不能处理小写");

                    //处理函数
                    var sf = canvasPathDef[ucmd];
                    if (!sf) throw new Error("未知的命令：" + cmd);

                    //参数列表
                    var len = pathDef[ucmd];
                    var ps = [];
                    for (var i = 0; i < len; ++i) {
                        ps.push(readNumber());
                    }

                    //处理
                    com.push([sf, ps]);
                }

                return com;
            }

            return function (ctx, prop) {
                ctx.beginPath();
                var p = prop.path();
                if (!prop._cache || prop._cache.path != p) {
                    prop._cache = {
                        path: p,
                        commands: canvasPath(p)
                    };
                }
                var cm = prop._cache.commands;
                for (var i = 0; i < cm.length; ++i) {
                    ctx[cm[i][0]].apply(ctx, cm[i][1]);
                }
                _strokeFill(ctx, prop);
            };
        })(),
        //圆形
        circle: function (ctx, prop) {
            ctx.beginPath();
            ctx.arc(prop.cx(), prop.cy(), prop.r(), 0, Math.PI, false);
            ctx.arc(prop.cx(), prop.cy(), prop.r(), Math.PI, Math.PI * 2, false);
            _strokeFill(ctx, prop);
        },
        //矩形
        rect: function (ctx, prop) {
            ctx.beginPath();
            ctx.moveTo(prop.x(), prop.y());
            ctx.lineTo(prop.x() + prop.width(), prop.y());
            ctx.lineTo(prop.x() + prop.width(), prop.y() + prop.height());
            ctx.lineTo(prop.x(), prop.y() + prop.height());
            ctx.closePath();
            _strokeFill(ctx, prop);
        },
        //多边形
        polygon: function (ctx, prop) {
            var p = prop.path();

            if (!p || p.length < 4) return;
            ctx.beginPath();
            ctx.moveTo(p[0], p[1]);
            for (var i = 3; i < p.length; i += 2) {
                ctx.lineTo(p[i - 1], p[i]);
            }
            if (prop.close()) ctx.closePath();
            _strokeFill(ctx, prop);
        },
        //图片
        image: function (ctx, prop) {
            var img;
            if (!prop._cache || prop._cache.src != prop.src()) {
                if (typeof prop.src() == 'string') {
                    img = new Image();
                    img.onload = function () {
                        prop.triggerChanged();
                    };
                    img.src = prop.src();
                } else img = prop.src(); //支持直接出入图片
                prop._cache = {
                    src: prop.src(),
                    image: img
                };
            } else img = prop._cache.image;
            var w = prop.width();
            var h = prop.height();
            if ((w || h) && img.width && img.height) {
                if (w && !h) h = w * img.height / img.width;
                if (!w && h) w = h * img.width / img.height;
            }
            var cw = prop.clipw(), ch = prop.cliph();
            if (w && h) try {

                if (cw || ch) {
                    ctx.drawImage(img, prop.cx(), prop.cy(), cw, ch, prop.x(), prop.y(), w, h);
                } else {
                    ctx.drawImage(img, prop.x(), prop.y(), w, h);
                }
            } catch (e) { }
            else try {
                if (cw || ch) {
                    ctx.drawImage(img, prop.cx(), prop.cy(), cw, ch, prop.x(), prop.y());
                } else {
                    ctx.drawImage(img, prop.x(), prop.y());
                }
            } catch (e) { }
        },
        //组合
        group: function (ctx, prop) {
            prop._sortedChs && prop._sortedChs.each(function (oi, e) { e._drawItem(ctx, oi); }, this);
        },
        //文字
        text: function (ctx, prop) {
            if (!prop.text()) return;

            //拼接字体样式
            var s = '';
            var tmp = prop.fontStyle();
            if (tmp && (tmp in jCanvas.fontStyle)) s += tmp + " ";

            tmp = prop.fontVariant();
            if (tmp && (tmp in jCanvas.fontVariant)) s += tmp + " ";

            tmp = prop.fontWeight();
            if (tmp && (tmp in jCanvas.fontWeight)) s += tmp + " ";

            var fsize = (prop.fontSize() || 10);
            if (typeof fsize === "number") fsize += 'px';
            s += fsize;


            var lh = prop.lineHeight();
            if (lh) {
                if (typeof lh === "number") lh += 'px';
                s += "/" + lh;
            }

            tmp = (prop.fontFamily() || 'Arial');
            if (/[^a-zA-Z0-9]/.test(tmp)) tmp = "'" + tmp + "'";
            s += " " + tmp;

            ctx.font = s;
            ctx.textAlign = prop.textAlign();
            ctx.textBaseline = prop.textBaseline();

            if (!prop.width()) {
                var metrics = ctx.measureText(prop.text());
                prop.width(metrics.width);
            }
            if (_strokeJ(ctx, prop)) {
                if (prop.width()) ctx.strokeText(prop.text(), 0, 0, prop.width());
                else ctx.strokeText(prop.text(), 0, 0);
            }
            if (_fillJ(ctx, prop)) {
                if (prop.width()) ctx.fillText(prop.text(), 0, 0, prop.width());
                else ctx.fillText(prop.text(), 0, 0);
            }
        }
    };
    //开放绘制函数，方便扩展
    t.draws = draw;
    t.strokeFill = _strokeFill;
    //绘制对象
    t.prototype._drawItem = function (ctx, prop) {
        var opt = prop.opacity();
        if (opt == 0) return;

        var tr = prop._hasTransform;
        var sh = prop.shadowColor() && prop.shadowBlur();
        if (tr || sh) ctx.save();
        var preOpt;
        if (tr) {
            //matrix
            ctx.transform(prop.scaleX(),
                prop.skewY() && Math.tan(prop.skewY() * Math.PI / 180),
                prop.skewX() && Math.tan(prop.skewX() * Math.PI / 180),
                prop.scaleY(),
                prop.left(),
                prop.top());
            prop.rotate() && ctx.rotate(prop.rotate() / 180 * Math.PI);
            if (prop.matrix()) {
                ctx.transform.apply(ctx, prop.matrix());
            }
            preOpt = ctx.globalAlpha;
            if (opt < 1) ctx.globalAlpha = preOpt * opt;
        }
        if (sh) {
            ctx.shadowColor = prop.shadowColor();
            ctx.shadowOffsetX = prop.shadowOffsetX();
            ctx.shadowOffsetY = prop.shadowOffsetY();
            ctx.shadowBlur = prop.shadowBlur();
        }

        var cpo;
        if (prop.compositeOperation()) {
            cpo = ctx.globalCompositeOperation;
            ctx.globalCompositeOperation = prop.compositeOperation();
        }
        //todo：check unsurported types
        draw[prop.name()].call(this, ctx, prop);

        if (tr || sh) ctx.restore();
        if (tr && opt < 1) ctx.globalAlpha = preOpt;
        if (cpo) ctx.globalCompositeOperation = cpo;

        prop._ischanged = false;
    };
    //整体绘制
    t.prototype.flush = function (force) {
        if (!force && !this._ischanged) {
            return;
            /*
            if (this._killNumber > 60) {
                if (this._killTimer) this._killTimer();
            }
            else ++this._killNumber;
            */
            return;
        }
        this._ischanged = false;

        if (!this._keep) this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
        this._sortedChs && this._sortedChs.each(function (oi, e) { e._drawItem(e.context, oi); }, this);
        this.trigger("draw");
    };
    //重置大小
    t.prototype.resize = function (w, h) {
        this.canvas.width = w;
        this.canvas.height = h;
        this.flush(true);
        return this;
    };
    //刷新
    t.prototype._ticker = function () {
        this.flush();
    };
    make_timer(t);
    //加载资源
    t.prototype.load = function (urls, progress) {
        if (!urls || !urls.length) {
            if (progress) progress(0, 0);
        }
        var loadedCount = 0;
        var loaded = function () {
            ++loadedCount;
            if (progress) progress(urls.length, loadedCount, this);
        };
        if (!this._resourceMap) this._resourceMap = {};
        for (var i = 0; i < urls.length; ++i) {
            if (!urls[i]) return loaded();
            var img = new Image();
            this._resourceMap[urls[i]] = img;
            img.onload = loaded;
            img.onabort = loaded;
            img.onerror = loaded;
            img.src = urls[i];
        }
    };
    return t;
})();

export default jCanvas;