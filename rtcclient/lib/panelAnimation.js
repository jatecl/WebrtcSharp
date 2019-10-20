import jTimeline from "./jTimelineCss";
import jMove from "./jMove";

/**
 * 预定义的过场动画
 */
export let PanelAnimation = {
    //淡入
    fade(toHide, toShow, done) {
        var line = jTimeline();
        if (toHide) line.to(toHide, 0.4, { opacity: 0 }, 0);
        if (toShow) line.fromTo(toShow, 0.4, {opacity:0},{opacity:1},0);
        line.callback(done);
        line.play();
    },
    //直接显示
    done(toHide, toShow, done) {
        done();
    },
    //从右边进来
    right(toHide, toShow, done) {
        var line = jTimeline();
        if (toHide) line.to(jTimeline.temp(p => jMove.css(toHide, "x", p + "%")), 0.4, { val: -100 }, 0);
        if (toShow) line.to(jTimeline.temp(p => jMove.css(toShow, "x", p + "%"), 100), 0.4, { val: 0 }, 0);
        line.callback(done);
        line.play();
    },
    //从左边进来
    left(toHide, toShow, done) {
        var line = jTimeline();
        if (toHide) line.to(jTimeline.temp(p => jMove.css(toHide, "x", p + "%")), 0.4, { val: 100 }, 0);
        if (toShow) line.to(jTimeline.temp(p => jMove.css(toShow, "x", p + "%"), -100), 0.4, { val: 0 }, 0);
        line.callback(done);
        line.play();
    },
    //从下面进来
    bottom(toHide, toShow, done) {
        var line = jTimeline();
        if (toHide) line.to(jTimeline.temp(p => jMove.css(toHide, "y", p + "%")), 0.4, { val: -100 }, 0);
        if (toShow) line.to(jTimeline.temp(p => jMove.css(toShow, "y", p + "%"), 100), 0.4, { val: 0 }, 0);
        line.callback(done);
        line.play();
    },
    //从上面进来
    top(toHide, toShow, done) {
        var line = jTimeline();
        if (toHide) line.to(jTimeline.temp(p => jMove.css(toHide, "y", p + "%")), 0.4, { val: 100 }, 0);
        if (toShow) line.to(jTimeline.temp(p => jMove.css(toShow, "y", p + "%"), -100), 0.4, { val: 0 }, 0);
        line.callback(done);
        line.play();
    }
};