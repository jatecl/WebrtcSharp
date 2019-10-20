import http from 'http';
import express from 'express';
import { listenServer } from './rtcServer';

process.on('uncaughtException', err => console.log(err));

var port = process.env.PORT || 8124;
var app = express();
var server = http.createServer(app);

//rtc 服务器
listenServer(server, (id, token, data) => {
    return data ? data : {
        name: "untitled"
    };
});
//启动服务
server.listen(port);
console.log("Server started at " + port + " @ " + new Date());
