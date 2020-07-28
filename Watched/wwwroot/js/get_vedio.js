//获取视频流代码块
var canvas = document.getElementById("canvas"), //取得canvas实例
    context = canvas.getContext("2d"), //取得2D画板
    video = document.getElementById("video"),//取得视频标签
    videoObj = { "video": true }, //设置获取视频
    errBack = function (error) {
        console.log("Video capture error: ", error.code);
    }; //设置错误回发信息

if (navigator.getUserMedia) { // 标准获取视频语法
    navigator.getUserMedia(videoObj, function (stream) {
        video.src = stream;
        video.play();
    }, errBack);
} else if (navigator.webkitGetUserMedia) { // Webkit内核语法
    navigator.webkitGetUserMedia(videoObj, function (stream) {
        video.src = window.webkitURL.createObjectURL(stream);
        var data = window.webkitURL.createObjectURL(stream);
        video.play();
    }, errBack);
}
else if (navigator.mozGetUserMedia) { // 火狐内核语法
    navigator.mozGetUserMedia(videoObj, function (stream) {
        video.src = window.URL.createObjectURL(stream);
        video.play();
    }, errBack);
}
//执行定时程序
window.setInterval(function () {
    context.drawImage(video, 0, 0, 320, 240);
    var type = 'jpg';
    var imgData = canvas.toDataURL(type);　　　　　　　　　　　　//使用localResizeIMG3压缩图像.
    lrz(imgData, {
        quality: 0.1,      //压缩率             
        done: function (results) {
            var data = results;
            chat.server.sendImage(data.base64);
            //var reader = new FileReader();
            // $("#canvas2").attr("src", data.base64);
        }
    });
}, 500)