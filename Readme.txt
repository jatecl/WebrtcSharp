一定要使用cmd，不能用powershell

安装python2.7，并加入path

下载depot_tools：
git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git
depot_tools的PATH变量要在python的前面。（注意，系统path在用户path的前面）
如果有报字符集错误，找到脚本对应的位置修改字符集即可。

设置系统变量
set DEPOT_TOOLS_WIN_TOOLCHAIN=0

设置系统代理，地址和端口号按实际的来
set http_proxy=127.0.0.1:53374
set https_proxy=127.0.0.1:53374
设置git代理：
git config --global http.proxy 127.0.0.1:53374
git config --global https.proxy 127.0.0.1:53374

设置git
git config --global user.name "my name"
git config --global user.email "my_email@address"
git config --global core.autocrlf false
git config --global core.filemode false
git config --global branch.autosetupmerge always
git config --global branch.autosetuprebase always
让git允许长文件名
git config --system core.longpaths true

在下载源前先运行：
gclient

然后下载源码：
mkdir webrtc
cd webrtc
fetch --nohooks webrtc
gclient sync

然后拉取目标版本（目前使用m76）
cd src
git checkout branch-heads/m76
git pull origin branch-heads/m76
gclient sync

建立Release项目文件：
gn gen out/Default --args="is_debug=false target_cpu=\"x64\" is_clang=false rtc_include_tests=false" --ide=vs2017

编译Release
ninja -C out/Default

如果需要调试，就要编译debug版本

建立DEBUG项目文件：
gn gen out/Debug --args="is_debug=true target_cpu=\"x64\" is_clang=false rtc_include_tests=false" --ide=vs2017

编译Debug
ninja -C out/Debug