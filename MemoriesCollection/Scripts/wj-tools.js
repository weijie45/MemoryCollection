var g_iTopScrollTop = 0;
var g_useParentScrollTop = false;
var _Percent = 0;


$(document).on('click', '[dissmiss-modal]', function () {
    $(this).parents('.wj-modal:first').hide();
});


//�x�s��������(�V�W�u��)��
function SaveScrollTop(iTop) {
    if (typeof iTop != "undefined") {
        g_iTopScrollTop = iTop;
    } else {
        g_iTopScrollTop = g_useParentScrollTop ? $(parent.window).scrollTop() : Math.max($(window).scrollTop(), $("body").scrollTop());
    }
    sessionStorage.setItem('KeepH', g_iTopScrollTop);
}

//���]��������(�V�W�u��)��
function RestoreScrollTop(iTop) {
    var topN = 0;
    if (typeof iTop != "undefined") {
        topN = iTop;
    } else if (typeof g_iTopScrollTop != "undefined") {
        topN = g_iTopScrollTop
    }
    if (g_useParentScrollTop) {
        $(parent.window).scrollTop(topN);
    } else {
        $(document).scrollTop(topN);
    }
    sessionStorage.removeItem('KeepH');
}


// When the user scrolls down 20px from the top of the document, show the button
window.onscroll = function () { scrollFunction(); FixTopFunction(); };

function scrollFunction() {
    if (document.body.scrollTop > 600 || document.documentElement.scrollTop > 600) {
        document.getElementById("backToTopBtn").style.display = "block";
    } else {
        document.getElementById("backToTopBtn").style.display = "none";
    }
}

// When the user clicks on the button, scroll to the top of the document
function topFunction() {
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

// Get the header
var header = document.getElementById("navbar-sum");

// Get the offset position of the navbar
var sticky = header.offsetTop;

// Add the sticky class to the header when you reach its scroll position. Remove "sticky" when you leave the scroll position
function FixTopFunction() {
    if (window.pageYOffset >= 10) {
        header.classList.add("sticky");
    } else {
        header.classList.remove("sticky");
    }
}

// ���o�ثe��controller name
function Controller() {
    var url = window.location.pathname.split("/");
    return url[1].toLowerCase();
}

// Over write Jquery Function
(function (b) {
    b.fn.tags = function () { //����/�����W���U������
        var args = $.extend.apply(true, arguments);
        return $.extend(NameVals(this), args);
    };
})(jQuery)

// �Nform�নjson
function ToJson($form) {
    var formData = $form.serializeArray();
    var json = {};

    //���h��name������
    $.map(formData, function (n, i) {
        var key = n['name'].trim();
        var val = n['value'].trim();
        var isMulti = $('[name=' + key + ']').attr('multi') == "";
        if (key in json) {
            if (typeof json[key] == "object") {
                json[key].push(val);
            } else {
                var tmp = json[n['name']];
                json[key] = [];
                json[key].push(tmp, val);
            }
        } else {
            if (isMulti) {
                var arr = [];
                arr.push(val);
                json[key] = arr;
            } else {
                json[key] = val;
            }
        }
    });
    return json;
}

var Sys = (function (Sys) {
    var res = "";
    // JSON �榡
    Sys.AjaxExec = function (url, tags, model, fn) {
        var params = {};
        var isAsync = true;
        switch (arguments.length) {
            case 1:
                tags = {};
                break;
            case 2:
                if (typeof tags == "function") {
                    fn = tags;
                    tags = {};
                }
                break;
            case 3:
                fn = model;
                break;
            case 4:
                params._models = JSON.stringify(model);
                break;
        }
        if (typeof tags.Async != "undefined") {
            isAsync = tags.Async == "true";
        }
        if (typeof fn != "function") {
            fn = function () { };
        }
        if (typeof tags == "string") {
            tags = {
                "SN": tags
            }
        }
        params._tags = JSON.stringify(tags);
        $.ajax({
            type: 'Post',
            url: url,
            data: JSON.stringify(params),
            async: isAsync, //�ҥΦP�B�ШD
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',//dataType,
            beforeSend: function () {
                //$.wait();
            },
            success: function (data) {
                if (typeof data.Errors != "undefined") {
                    $.close();
                    layer.alert(data.Errors[0], {
                        icon: 7
                    });
                } else {
                    if (Array.isArray(data)) {
                        if (data[0] != "") {
                            layer.alert(data[0]);
                            $.close();
                        } else if (typeof tags.TargetID != "undefined" && data[1] != "") {
                            $("#{0}".format(tags.TargetID)).html(data[1]).show();
                            fn(data);
                        } else {
                            fn(data);
                        }
                    } else {
                        fn(data);
                    }
                }
            },
            error: function (response) {
                layer.alert(url + ' ajax���~ !', {
                    icon: 7
                });
                $.close();
            },
            complete: function (e) {
                $('#navbar').removeClass('in');
            }
        });
    };

    // �ɮפW��
    Sys.AjaxUpload = function (url, fd, fn) {
        $.ajax({ // JQuery Ajax
            type: 'POST',
            url: url, // URL to the PHP file which will insert new value in the database
            data: fd, // We send the data string
            processData: false,
            contentType: false,
            beforeSend: function () {
                _Percent = 0;
            },
            success: function (data) {
                if (typeof data.Errors != "undefined") {
                    $.close();
                    layer.alert(data.Errors[0], { icon: 7 });
                } else {
                    if (Array.isArray(data)) {
                        if (data[0] != "") {
                            layer.alert(data[0], { icon: 7 });
                            $.close();
                        } else {
                            fn(data);
                        }
                    } else {
                        fn(data);
                    }
                }
            }, error: function (response) {
                if (response.status == 200) {
                    location.reload();
                } else {
                    layer.alert(decodeURIComponent(response.statusText), { icon: 7 }, function () {
                        location.reload();
                    });
                }
                $.close();
            },
            xhr: function () {
                var xhr = $.ajaxSettings.xhr();
                xhr.upload.onprogress = function (e) {
                    var pourc = (e.loaded / e.total * 100) - (Math.floor(Math.random() * (9 - 1 + 0.62)) + 1);
                    console.log(pourc);
                    if (_Percent < pourc) {
                        _Percent = pourc;
                    }
                    UpdateProgress();
                };
                return xhr;
            },
            complete: function (e) {
                UpdateProgress(100);
                $('#navbar').removeClass('in');
            }
        });
    };

    return Sys;
})(Sys || {});


function UpdateProgress() {
    console.log(_Percent);
    $('#ProgressBarBlock #bar').html("{0}{1}".format($.number(_Percent, 2), "%"));
    if (parseInt(_Percent) == 100) {
        setTimeout(function () { $.closeProgress(); }, 500);
    }
}



function NameVals(sSelector) {
    var info = {};
    var _models = null;
    var $root = sSelector.jquery ? sSelector : $(sSelector);
    if ($root.length == 0) {
        return info;
    }
    $root.each(function () {
        var $part = $(this);
        var _isModel = false;
        var modelName = $part.attr("model");
        if (typeof modelName == "string") {
            modelName = modelName.trim();

            if (modelName != "") {
                _isModel = true;
                if (_models == null) {
                    _models = {};
                }
            }
        }
        var aNames = $.unique($part.find(":text[name], textarea[name], :radio[name], :checkbox[name], :password[name], select[name], input[name]").attrs("name"));
        var sNames = aNames + "";
        if (sNames != "") {
            var aValues = Vals(sNames, $part);
            for (var i = 0; i < aNames.length; i++) {
                if (aNames[i] != "") {
                    if (_isModel) {
                        if (typeof _models[modelName] == "undefined") {
                            _models[modelName] = {};
                        }
                        _models[modelName][aNames[i]] = aValues[i];
                        //_models[aNames[i]] = aValues[i];
                    } else {
                        info[aNames[i]] = aValues[i];
                    }
                }
            }
            if (!_isModel) {
                var $select = $part.find("select[name]");
                if ($select.length > 0) {
                    $select.each(function () {
                        var $this = $(this);
                        var n = $this.attr("name") + "_Text";
                        var t = $.trim($this.find("option:selected").texts());
                        info[n] = t;
                    });
                }
            }
        }
    });
    if (_models != null) {
        info.m = _models;
    } else {
        //info.json = $.toJSON(info);
    }
    return info;
}
//���o�ҿﶵ�حȬ��@�Ӱ}�C
function Vals(sNameList, sSelector) {
    var aList = new Array;
    if (sNameList == "") {
        return aList;
    }
    var $root;
    if (typeof sSelector == "undefined") {
        $root = null;
    } else {
        $root = sSelector.jquery ? sSelector : $(sSelector);
        if ($root.length == 0) {
            $root = null;
        }
    }
    var aName = sNameList.split(",");
    for (var i = 0; i < aName.length; i++) {
        if (aName[i] != "") {
            var $tag = $root != null ? $root.find("input[name='" + aName[i] + "']") : $("input[name='" + aName[i] + "']");
            if ($tag.length == 0) {
                $tag = $root != null ? $root.find("select[name='" + aName[i] + "']") : $("select[name='" + aName[i] + "']");
            }
            if ($tag.length == 0) {
                $tag = $root != null ? $root.find("textarea[name='" + aName[i] + "']") : $("textarea[name='" + aName[i] + "']");
            }
            if ($tag.length == 0) {
                $tag = $root != null ? $root.find("div[id='" + aName[i] + "']") : $("div[id='" + aName[i] + "']");
            }
            if ($tag.length == 0) {
                $tag = $root != null ? $root.find("span[id='" + aName[i] + "']") : $("span[id='" + aName[i] + "']");
            }
            if ($tag.length > 0) {
                switch ($tag.get(0).tagName) {
                    case "LABEL":
                    case "DIV":
                    case "SPAN":
                    case "STRONG":
                    case "H1":
                    case "H2":
                    case "H3":
                    case "UL":
                    case "LI":
                    case "EM":
                    case "TD":
                    case "TR":
                    case "P":
                    case "B":
                    case "A":
                        aList.push($tag.html());
                        break;
                    default:
                        switch ($tag.get(0).type) {
                            case "checkbox":
                            case "radio":
                                aList.push($tag.filter(":checked").vals() + "");
                                break;
                            case "select-one":
                            case "select-multiple":
                                var size = $tag.attr("size");
                                if (size == undefined || size == "1") {
                                    aList.push($.trim($tag.val()));
                                } else {
                                    aList.push($tag.find("option").vals() + "");
                                }
                                break;
                            default:
                                aList.push($.trim($tag.val()));
                                break;
                        }
                        break;
                }
            } else {
                aList.push("");
            }
        } else {
            aList.push("");
        }
    }
    return aList;
}


//  �̽�
$.wait = function (msg) {
    $("body").addClass("loading");
    if (!msg) { msg = "��Ƭd�ߤ�..."; }
    $('#LoadingMsg').text(msg);
};

$.close = function () {
    $("body").removeClass("loading");
    //$('#ListPanel-loading').hide();
};

// �i�ױ��̽�
$.waitProgress = function () {
    $("body").addClass("progressing");
};

$.closeProgress = function () {
    $("body").removeClass("progressing");
};

Array.prototype.max = function () {
    return Math.max.apply(null, this);
};

Array.prototype.min = function () {
    return Math.min.apply(null, this);
};

// �}�C���O�_�]�t���w����
Array.prototype.contains = function (aValue) {
    if (aValue.constructor === Array) {
        for (var i = 0; i < aValue.length; i++) {
            if ($.inArray(aValue[i], this) == -1) {
                return false;
            }
        }
    } else {
        if ($.inArray(aValue, this) == -1) {
            return false;
        }
    }
    return true;
};

// �}�C���O�_�]�t���w����
Array.prototype.contains = function (aValue) {
    if (aValue.constructor === Array) {
        for (var i = 0; i < aValue.length; i++) {
            if ($.inArray(aValue[i], this) == -1) {
                return false;
            }
        }
    } else {
        if ($.inArray(aValue, this) == -1) {
            return false;
        }
    }
    return true;
};

//��Ʈ榡�Ƴ]�w
String.prototype.format = function () {
    var txt = this.toString();
    var args = arguments;
    if (typeof args == "undefined") {
        return txt;
    }
    if (args.length == 1 && typeof (args[0]) != "undefined" && args[0].constructor === Array) {
        args = args[0];
    }
    for (var i = 0; i < args.length; i++) {
        var exp = getStringFormatPlaceHolderRegEx(i);
        txt = txt.replace(exp, (typeof (args[i]) == "undefined" || args[i] == null) ? "" : args[i]);
    }
    return txt;
};
//�榡�Ƴ]�w�F���h��ܦ�
function getStringFormatPlaceHolderRegEx(placeHolderIndex) {
    return new RegExp("({)?\\{" + placeHolderIndex + "\\}(?!})", "gm");
}


// �V���ɭ�
String.prototype.padLeft = function (l, c) {
    return Array(l - this.length + 1).join(c || " ") + this;
}

// �V�k�ɴ�
String.prototype.padRight = function (l, c) {
    return this + Array(l - this.length + 1).join(c || " ");
}


// �����r��ť�
// �^��: string
String.prototype.trim = function (v) {
    if (this == undefined) {
        return "";
    }
    if (v != undefined && v != "") {
        var re = new RegExp("^(" + v + ")+|(" + v + ")+$", "ig");
    } else {
        var re = /^\s+|\s+$/g;
    }
    return this.replace(re, "");
};

// �����r�ꥪ��ť�
// �^��: string
String.prototype.ltrim = function (v) {
    if (this == undefined) {
        return "";
    }
    if (v != undefined && v != "") {
        var re = new RegExp("^(" + v + ")+", "ig");
    } else {
        var re = /^\s+/;
    }
    return this.replace(re, "");
};

// �����r��k��ť�
// �^��: string
String.prototype.rtrim = function (v) {
    if (this == undefined) {
        return "";
    }
    if (v != undefined && v != "") {
        var re = new RegExp("(" + v + ")+$", "ig");
    } else {
        var re = /\s+$/;
    }
    return this.replace(re, "");
}


// �P�_�r��}�Y�O�_�����w���r
// �^��: bool
String.prototype.startWith = function (v) {
    return (this.length > 0 && this.length >= v.length) && this.substr(0, v.length).toLowerCase() === v.toLowerCase();
};

// �P�_�r�굲���O�_�����w���r
// �^��: bool
String.prototype.endWith = function (v) {
    return (this.length > 0 && this.length >= v.length) && this.substr(this.length - v.length).toLowerCase() === v.toLowerCase();
};

// �P�_�r��O�_���]�t���w���r
// �^��: bool
String.prototype.contain = function (v) {
    return this.indexOf(v) >= 0;
};

// �q�r�ꥪ��}�l���ȡA�æ^��
// �^��:string
String.prototype.left = function (i) {
    return this.substr(0, i);
};

// �q�r��k��}�l���ȡA�æ^��
// �^��:string
String.prototype.right = function (i) {
    return this.substr(this.length - i, i);
};

// �q�r���s�Ӷ}�l���ȡA�@��i�Ӧr��
// �^��:string
String.prototype.mid = function (s, i) {
    return this.substr(s - 1, i);
}

String.prototype.replaceAt = function (index, replacement) {
    return this.substr(0, index) + replacement + this.substr(index + replacement.length);
}


// �}�C�নJSON�榡
Array.prototype.parseJSON = function () {
    var engine = typeof JSON == "undefined" ? 2 : 1;
    for (var i = 0; i < this.length; i++) {
        if (typeof this[i] == "string" && this[i].length > 1) {
            if (this[i].length == 2 && this[i] == "{}") {
                this[i] = {};
            } else {
                if (this[i].left(2) == "{\"" && this[i].right(1) == "}") {
                    try {
                        this[i] = engine == 1 ? JSON.parse(this[i]) : $.parseJSON(this[i]);
                    } catch (e) { };
                }
            }
        }
    }
};