// 这里是注册集线器调用的方法,和1.0不同的是需要chat.client后注册,1.0则不需要
var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
var chat = $.connection.getMessage;
chat.client.broadcastMessage = function (name) {
    // HTML编码的显示名称和消息。
    var encodedMsg = $('<div />').text(name).html();
    // 将消息添加到该页。
    $('#messsagebox').append('<li>' + encodedMsg + '</li>');
};

//获取图片数据,并实时显示
chat.client.showimage = function (data) {
    if ($("#" + data.id).length <= 0) {
        var html = '<div style="float: left; border: double" id="div' + data.id + '">\
                                <img id="'+ data.id + '" width="320" height="240">\
                                <br />\
                                <span>用户'+ data.id + '</span>\
                                </div>'
        $("#contextdiv").append(html)
    }
    $("#" + data.id).attr("src", data.data);
}
// 获取用户名称。
$('#username').html(prompt('请输入您的名称:', ''));
// 设置初始焦点到消息输入框。
$('#message').focus();

// 启动连接,这里和1.0也有区别
$.connection.hub.start().done(function () {
    $('#send').click(function () {
        var message = $('#username').html() + ":" + $('#message').val()
        // 这里是调用服务器的方法,同样,首字母小写
        chat.server.sendMessage(message);
        // 清空输入框的文字并给焦点.
        $('#message').val('').focus();
    });
});