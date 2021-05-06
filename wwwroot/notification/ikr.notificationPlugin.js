(function ($) {
    var opt
    $.fn.ikrNotificationSetup = function (options) {
        var defaultSettings = $.extend({
            BeforeSeenColor: "#2E467C",
            AfterSeenColor: "#ccc"
        }, options);
        $(".ikrNoti_Button").css({
            "background": defaultSettings.BeforeSeenColor
        });
        var parentId = $(this).attr("id");
        if ($.trim(parentId) != "" && parentId.length > 0) {
            $("#" + parentId).append("<div class='ikrNoti_Counter'></div>" +
                "<div class='ikrNoti_Button'></div>" +
                "<div class='ikrNotifications'>" +
                "<h3>Уведомления (<span class='notiCounterOnHead'>0</span>)</h3>" +
                "<div class='ikrNotificationItems'>" +
                "</div>" +
                "<div class='ikrSeeAll'><a href='#'>Посмотреть все</a></div>" +
                "</div>");

            $('#' + parentId + ' .ikrNoti_Counter')
                .css({ opacity: 0 })
                .text(0)
                .css({ top: '-10px' })
                .animate({ top: '-2px', opacity: 1 }, 500);

            $('#' + parentId + ' .ikrNoti_Button').click(function () {
                opt.NotificationList.forEach(function (item) {
                    item.isRead = true
                    let json = JSON.stringify(item);
                    console.log(json)
                    let request = new XMLHttpRequest();
                    request.open("PUT", "/notificationSeen", true);
                    request.setRequestHeader("Content-Type", "application/json");
                    request.send(json);
                });   

                $('#' + parentId + ' .ikrNotifications').fadeToggle('fast', 'linear', function () {
                    if ($('#' + parentId + ' .ikrNotifications').is(':hidden')) {
                        $('#' + parentId + ' .ikrNoti_Button').css('background-color', defaultSettings.AfterSeenColor);
                    }
                    else $('#' + parentId + ' .ikrNoti_Button').css('background-color', defaultSettings.BeforeSeenColor);
                });
                $('#' + parentId + ' .ikrNoti_Counter').fadeOut('slow');
                return false;
            });
            $(document).click(function () {
                $('#' + parentId + ' .ikrNotifications').hide();
                if ($('#' + parentId + ' .ikrNoti_Counter').is(':hidden')) {
                    $('#' + parentId + ' .ikrNoti_Button').css('background-color', defaultSettings.AfterSeenColor);
                }
            });
            $('#' + parentId + ' .ikrNotifications').click(function () {
                return false;
            });

            $("#" + parentId).css({
                position: "relative"
            });
        }
    };
    $.fn.ikrNotificationCount = function (options) {
        opt = options
        var defaultSettings = $.extend({
            NotificationList: [],
            ListTitlePropName: "",
            ListBodyPropName: "",
            ControllerName: "Notifications",
            ActionName: "AllNotifications"
        }, options);
        var parentId = $(this).attr("id");
        if ($.trim(parentId) != "" && parentId.length > 0) {
            $("#" + parentId + " .ikrNotifications .ikrSeeAll").click(function () {
                window.open('../' + defaultSettings.ControllerName + '/' + defaultSettings.ActionName + '', '_blank');
            });

            var totalUnReadNoti = defaultSettings.NotificationList.filter(x => x.isRead == false).length;
            $('#' + parentId + ' .ikrNoti_Counter').text(totalUnReadNoti);
            $('#' + parentId + ' .notiCounterOnHead').text(totalUnReadNoti);
            if (defaultSettings.NotificationList.length > 0) {
                $.map(defaultSettings.NotificationList, function (item) {
                    head = item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)].substring(0, 2);
                    console.log(item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)])
                    if (head == "Уд") {
                        var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                        $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' Id=" + item.id + ">" +
                            "<h5 class='ikrNotificationTitle'> " + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                            "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                            "<a title='Заглушить участника' href=''><i class='fa fa-user-slash' onclick='memberMuting(this)'></i></a>" +
                            "<a title='Удалить уведомление' href=''><i class='fas fa-trash-alt' onclick='deleteNoti(this)'></i></a>");
                    } else if (head == "Ва") {
                        var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                        $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' Id=" + item.id + ">" +
                            "<h5 class='ikrNotificationTitle'> " + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                            "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                            "<a title='Заглушить участника' href=''><i class='fa fa-user-slash' onclick='memberMuting(this'></i></a>" +
                            "<a title='Заглушить группу' href=''><i class='fas fa-volume-mute' onclick='partyMuting(this)'></i></a>" +
                            "<a title='Удалить уведомление' href=''><i class='fas fa-trash-alt' onclick='deleteNoti(this)'></i></a>" +
                            "<a title='Принять' href=''><i class='fas fa-thumbs-up'></a>");
                    } else {
                        var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                        $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' Id=" + item.id + ">" +
                            "<h5 class='ikrNotificationTitle'> " + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                            "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                            "<a title='Заглушить участника' href=''><i class='fa fa-user-slash' onclick='memberMuting(this)'></i></a>" +
                            "<a title='Заглушить группу' href=''><i class='fas fa-volume-mute' onclick='partyMuting(this)'></i></a>" +
                            "<a title='Удалить уведомление' href=''><i class='fas fa-trash-alt' onclick='deleteNoti(this)'></i></a>");
                    }
                    $("#" + parentId + " .ikrNotificationItems .ikrSingleNotiDiv[Id=" + item.id + "]").click(function () {
                        if ($.trim(item.url) != "") {
                            window.location.href = item.url;
                        }
                    });
                });
            }
        }
    };
}(jQuery));

function partyMuting(element) {
    str = $(element).parent().parent()[0].innerText
    let result = str.match(/@(?<=[-])[^\s]+@[^\s]+\.[^\s]+|(?<=[:])[^\s]+/);
    if ($(element).attr('data-icon') == 'volume-up') {
        $(element).parent().attr('title', 'Включить группу')
        $(element).attr('data-icon', 'volume-mute')
        let request = new XMLHttpRequest();
        request.open("GET", "/Notifications/Mute?act=False&name=" + result[0], true);
        request.send();
    }
    else {
        $(element).attr('data-icon', 'volume-up')
        $(element).parent().attr('title', 'Заглушить')
        let request = new XMLHttpRequest();
        request.open("GET", "/Notifications/Mute?act=True&name=" + result[0], true);
        request.send();
    }
}

function memberMuting(element) {
    str = $(element).parent().parent()[0].innerText
    let result = str.match(/(?<=[-])[^\s]+@[^\s]+\.[^\s]+|(?<=[:])[^\s]+/);
    if ($(element).attr('data-icon') == 'user-slash') {
        $(element).parent().attr('title', 'Включить участника')
        $(element).attr('data-icon', 'user')
        let request = new XMLHttpRequest();
        request.open("GET", "/Notifications/Mute?act=False&name=" + result[0], true);
        request.send();
    }
    else {
        $(element).parent().attr('title', 'Заглушить участника')
        $(element).attr('data-icon', 'user-slash')
        let request = new XMLHttpRequest();
        request.open("GET", "/Notifications/Mute?act=True&name=" + result[0], true);
        request.send();
    }
}

function deleteNoti(element) {
    notiId = $(element).parent().parent()[0].id
    let request = new XMLHttpRequest();
    request.open("GET", "/Notifications/Delete/" + notiId + "?act=False", true);
    request.send();
}

function ikrLowerFirstLetter(value) {
    return value.charAt(0).toLowerCase() + value.slice(1);
}