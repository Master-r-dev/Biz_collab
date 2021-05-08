(function ($) {
    var opt
    $.fn.ikrNotificationSetup = function (options) {
        var defaultSettings = $.extend({
            BeforeSeenColor: "#ff00c8",
            AfterSeenColor: "#20ada8"
        }, options);
        $(".ikrNoti_Button").css({
            "background": defaultSettings.BeforeSeenColor
        });
        var parentId = $(this).attr("id");
        if ($.trim(parentId) != "" && parentId.length > 0) {
            $("#" + parentId).append("<div class='ikrNoti_Counter'></div>" +
                "<div class='ikrNoti_Button'></div>" +
                "<div class='ikrNotifications'>" +
                "<h3 charset='windows - 1251'>Уведомления (<span class='notiCounterOnHead'>0</span>)</h3>" +
                "<div class='ikrNotificationItems'>" +
                "</div>" +
                "<div class='ikrSeeAll' charset='windows - 1251'><a href='#'>Посмотреть все</a></div>" +
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
                    let mutedName = item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)].match(/(?<=[-])[^\s]+@[^\s]+\.[^\s]+|(?<=[:])[^\s]+/);
                    let mutedGroup = item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)].match(/@(?<=[-])[^\s]+@[^\s]+\.[^\s]+|(?<=[:])[^\s]+/);
                    let muted = this.localStorage.getItem("mutedNames")
                    if (head == "Уд" && muted.includes(mutedName[0])) {
                        var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                        $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' Id=" + item.id + ">" +
                            "<h5 class='ikrNotificationTitle'> " + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                            "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                            "<div class='ikrNofiCreatedDate'>" + new Date(item.createdDate).toLocaleString('en-GB') + "</div>");
                        if (muted.includes(mutedName[0])) {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title = 'Включить участника' href = '' >" +
                                "<i class= 'fa fa-user' onclick = 'memberMuting(this)'></i></a>");
                        } else {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title = 'Заглушить участника' href = '' >" +
                                "<i class= 'fa fa-user-slash' onclick = 'memberMuting(this)'></i></a>");
                        }
                        $("#" + parentId + " .ikrNotificationItems").append("<a title='Удалить уведомление' href=''>" +
                            "<i class='fas fa-trash-alt' onclick='deleteNoti(this)'></i></a > ");
                    } else if (head == "Ва") {
                        var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                        $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' Id=" + item.id + ">" +
                            "<h5 class='ikrNotificationTitle'> " + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                            "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                            "<div class='ikrNofiCreatedDate'>" + new Date(item.createdDate).toLocaleString('en-GB') + "</div>");
                        if (muted.includes(mutedName[0])) {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title = 'Включить участника' href = '' >" +
                                "<i class= 'fa fa-user' onclick = 'memberMuting(this)'></i></a>");
                        } else {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title = 'Заглушить участника' href = '' >" +
                                "<i class= 'fa fa-user-slash' onclick = 'memberMuting(this)'></i></a>");
                        }
                        if (muted.includes(mutedGroup[0])) {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title='Включить группу' href=''>" +
                                "<i class='fas fa-volume-up' onclick='partyMuting(this)'></i></a > ");
                        } else {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title='Заглушить группу' href=''>" +
                                "<i class='fas fa-volume-mute' onclick='partyMuting(this, true)'></i></a > ");
                        }

                        $("#" + parentId + " .ikrNotificationItems").append("<a title='Удалить уведомление' href=''>" +
                            "<i class='fas fa-trash-alt' onclick='deleteNoti(this)'></i></a > " +
                            "<a title='Принять' href=''><i class='fas fa-thumbs-up' onclick='acceptInvite(this)'></a>");
                    } else if (head == "Тр") {
                        var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                        $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' Id=" + item.id + ">" +
                            "<h5 class='ikrNotificationTitle'> " + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                            "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                            "<div class='ikrNofiCreatedDate'>" + new Date(item.createdDate).toLocaleString('en-GB') + "</div>");
                        if (muted.includes(mutedGroup[0])) {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title='Включить группу' href=''>" +
                                "<i class='fas fa-volume-up' onclick='partyMuting(this)'></i></a > ");
                        } else {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title='Заглушить группу' href=''>" +
                                "<i class='fas fa-volume-mute' onclick='partyMuting(this)'></i></a > ");
                        }

                        $("#" + parentId + " .ikrNotificationItems").append("<a title='Удалить уведомление' href=''>" +
                            "<i class='fas fa-trash-alt' onclick='deleteNoti(this)'></i></a >");
                    } else {
                        var className = item.isRead ? "" : " ikrSingleNotiDivUnReadColor";
                        $("#" + parentId + " .ikrNotificationItems").append("<div class='ikrSingleNotiDiv" + className + "' Id=" + item.id + ">" +
                            "<h5 class='ikrNotificationTitle'> " + item[ikrLowerFirstLetter(defaultSettings.ListTitlePropName)] + "</h5>" +
                            "<div class='ikrNotificationBody'>" + item[ikrLowerFirstLetter(defaultSettings.ListBodyPropName)] + "</div>" +
                            "<div class='ikrNofiCreatedDate'>" + new Date(item.createdDate).toLocaleString('en-GB') + "</div>");
                        if (muted.includes(mutedName[0])) {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title = 'Включить участника' href = '' >" +
                                "<i class= 'fa fa-user' onclick = 'memberMuting(this)'></i></a>");
                        } else {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title = 'Заглушить участника' href = '' >" +
                                "<i class= 'fa fa-user-slash' onclick = 'memberMuting(this)'></i></a>");
                        }
                        if (muted.includes(mutedGroup[0])) {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title='Включить группу' href=''>" +
                                "<i class='fas fa-volume-up' onclick='partyMuting(this)'></i></a > ");
                        } else {
                            $("#" + parentId + " .ikrNotificationItems").append("<a title='Заглушить группу' href=''>" +
                                "<i class='fas fa-volume-mute' onclick='partyMuting(this)'></i></a > ");
                        }
                        $("#" + parentId + " .ikrNotificationItems").append("<a title='Удалить уведомление' href=''>" +
                            "<i class='fas fa-trash-alt' onclick='deleteNoti(this)'></i></a > ");
                    }

                    $("#" + parentId + " .ikrNotificationTitle .ikrNotificationBody .ikrSingleNotiDiv[Id=" + item.id + "]").click(function () {
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
    } else {
        $(element).attr('data-icon', 'volume-up')
        $(element).parent().attr('title', 'Заглушить')
        let request = new XMLHttpRequest();
        request.open("GET", "/Notifications/Mute?act=True&name=" + result[0], true);
        request.send();
    }
}

function acceptInvite(element) {
    str = $(element).parent().parent()[0].innerText
    let result = str.match(/@(?<=[-])[^\s]+@[^\s]+\.[^\s]+|(?<=[:])[^\s]+/);
    let request = new XMLHttpRequest();
    request.open("GET", "/Notifications/Accept/" + $(element).parent().parent().attr("id") + "?name=" + result[0], true);
    request.send();
}

function memberMuting(element) {
    str = $(element).parent().parent()[0].innerText
    let result = str.match(/(?<=[-])[^\s]+@[^\s]+\.[^\s]+|(?<=[:])[^\s]+/);
    if ($(element).attr('data-icon') == 'user-slash') {
        $(element).parent().attr('title', 'Включить участника')
        $(element).attr('data-icon', 'user')
        let request = new XMLHttpRequest();
        request.open("GET", "/Notifications/Mute?act=True&name=" + result[0], true);
        request.send();
    } else {
        $(element).parent().attr('title', 'Заглушить участника')
        $(element).attr('data-icon', 'user-slash')
        let request = new XMLHttpRequest();
        request.open("GET", "/Notifications/Mute?act=False&name=" + result[0], true);
        request.send();
    }
}

function deleteNoti(element, f) {
    notiId = $(element).parent().parent()[0].id
    console.log(notiId = $(element).parent().parent()[0].innerText)
    let request = new XMLHttpRequest();
    request.open("GET", "/Notifications/Delete/" + notiId + "?act=" + f, true);
    request.send();
}

function ikrLowerFirstLetter(value) {
    return value.charAt(0).toLowerCase() + value.slice(1);
}