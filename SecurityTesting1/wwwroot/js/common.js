function escapeHtml(unsafe) {
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/"/g, "&quot;")
         .replace(/'/g, "&#039;");
}

function bytesToSize(bytes) {
    let sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    if (bytes == 0) return '0 Byte';
    let i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
    return Math.round(bytes / Math.pow(1024, i), 2) + ' ' + sizes[i];
};

function makeId(length) {
    let text = "";
    let possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    for (let i = 0; i < length; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));

    return text;
}

function humanizeDuration(durationString) {
    if (durationString == null) {
        console.log('Argument "durationString" is null.');
        return '';
    }

    if (typeof durationString === 'undefined') {
        console.log('Argument "durationString" is undefined.');
        return '';
    }

    let parts = durationString.split(':');
    let days = 0;
    let hours = 0;
    let minutes = 0;
    let seconds = 0;

    if (parts.length > 4) {
        return '?';
    }

    if (parts.length < 3) {
        return '?';
    }

    if (parts.length == 4) {
        days = Number.parseInt(parts[0]);
        hours = Number.parseInt(parts[1]);
        minutes = Number.parseInt(parts[2]);
        seconds = Math.round(parseFloat(parts[3]));
    } else {
        hours = Number.parseInt(parts[0]);
        minutes = Number.parseInt(parts[1]);
        seconds = Math.round(parseFloat(parts[2]));
    }

    let result = '';
    if (days > 0) {
        result += days + 'd';
    }

    if (days > 0 || hours > 0) {
        result += ' ' + hours + 'h';
    }

    if (days > 0 || hours > 0 || minutes > 0) {
        result += ' ' + minutes + 'm';
    }

    if (days > 0 || hours > 0 || minutes > 0 || seconds > 0) {
        result += ' ' + seconds + 's';
    }

    return result;
}

function isInt(value) {
    var x;
    if (isNaN(value)) {
        return false;
    }
    x = parseFloat(value);
    return (x | 0) === x;
}

function setCookie(name, value, minutes) {
    var expires = "";
    if (minutes) {
        var date = new Date();
        date.setTime(date.getTime() + (minutes * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}

function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function eraseCookie(name) {
    document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}

function parseIsoDate(dateIsoString) {
    //TODO: error checking
    var parts = dateIsoString.split('T').join('-').split(':').join('-').split('-');
    var date = {
        year: parseInt(parts[0]),
        month: parseInt(parts[1]),
        day: parseInt(parts[2]),
        hour: parseInt(parts[3]),
        minute: parseInt(parts[4]),
        second: parseInt(parts[5]),
    };

    return date;
}

function formatIsoDate(dateIsoString, format) {
    //TODO: add support for other formats
    //

    var date = parseIsoDate(dateIsoString);

    var output = date.year.toString() + '-' + date.month.toString().padStart(2, '0') + '-' + date.day.toString().padStart(2, '0') + ' ';
    if (date.hour == 0) {
        output += '12' + ':' + (date.minute).toString().padStart(2, '0') + ' AM';
    } else if (date.hour == 12) {
        output += '12' + ':' + (date.minute).toString().padStart(2, '0') + ' PM';
    } else if (date.hour > 12) {
        output += (date.hour - 12).toString().padStart(2, '0') + ':' + (date.minute).toString().padStart(2, '0') + ' PM';
    } else {
        output += (date.hour).toString().padStart(2, '0') + ':' + (date.minute).toString().padStart(2, '0') + ' AM';
    }

    return output;
}

function parseList(tagsString) {
    // Remove blanks and spaces from the string
    tagsString = tagsString.replace(/\s+/g, "");

    // Split the string by comma or semicolon
    let arr = tagsString.split(/[;,]/);

    // Return the array
    return arr;
}

function renewJwt(renewUrl, accessDeniedUrl, signOutUrl, renewInMinutes, callback = null) {
    $.ajax({
        url: renewUrl,
        data: {
        },
        headers: {
            'Authorization': 'Bearer ' + getCookie('token')
        },
        type: 'PUT',
        crossDomain: true,
        dataType: 'json',
        cache: false,
        contentType: "application/json",
        success: function (data) {
            setCookie('token', data.token, data.expiresInMinutes);

            if (typeof callback === 'function') {
                callback();
            }

            setTimeout(function () {
                renewJwt(renewUrl, accessDeniedUrl, signOutUrl, renewInMinutes, callback);
            }, renewInMinutes * 60 * 1000);
        },
        complete: function (jqXHR, textStatus) {
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (pageUnloading || jqXHR.statusText == "abort") {
                return;
            }

            if (jqXHR.status === 401) {
                //Most likely token expired.
                window.location.href = signOutUrl;
            } else if (jqXHR.status === 403) {
                //Access denied.
                window.location.href = accessDeniedUrl;
            } else if (jqXHR.status === 400) {
                console.log(jqXHR.responseText);
                alert('An error has occurred. Please try again. Contact your System Administrator if the issue persists.');
                window.location.href = accessDeniedUrl;
            } else {
                console.log('Error sending data: ' + errorThrown);
                alert('An error has occurred. Please try again. Contact your System Administrator if the issue persists.');
                window.location.href = accessDeniedUrl;
            }
        }
    });
}

function getThousandsSeparator() {
    if (typeof Intl !== 'object') {
        return ',';
    }

    let formatedNumberText = new Intl.NumberFormat().format(1000);
    return formatedNumberText.substring(1, 2);
}

