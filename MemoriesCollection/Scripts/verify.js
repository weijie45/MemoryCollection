
//檢查字串bytes數(中文及全形算 3 bytes)
function ChkBytes(str, warning, len) {
    var byte_cnt = 0;
    var errMsg = "";
    if ((str == null || str == "")) return "";
    str = str.toString();
    //檢查'及"
    var reg = /["']/;
    if (reg.test(str)) {
        errMsg = warning + "不可輸入引號( 包含 " + "'" + " 及 " + "\"" + " )"
        return errMsg;
    }

    for (var i = 0; i < str.length; ++i) {
        var ch = str.charAt(i);
        if ((ch >= "0" && ch <= "9") || (ch >= "A" && ch <= "Z") || (ch >= "a" && ch <= "z") ||
              ch == " " || ch == "!" || ch == "#" || ch == "$" || ch == "%" || ch == "&" || ch == "(" || ch == ")" ||
              ch == "*" || ch == "+" || ch == "," || ch == "-" || ch == "." || ch == "/" || ch == ":" || ch == ";" || ch == "<" ||
              ch == "=" || ch == ">" || ch == "?" || ch == "@@" || ch == "[" || ch == "_" || ch == "^" || ch == "]" || ch == "`" ||
              ch == "{" || ch == "|" || ch == "}" || ch == "~" || (ch > "[" && ch < "]")) { /* 規則:共容許31個特殊字元,不可含任何單引號'及雙引號" */
            byte_cnt++;
        } else {
            byte_cnt = byte_cnt + 3;
        }
    }
    if (byte_cnt > len) {
        errMsg = warning + "超過容許的字數!";
        return errMsg;
    }
    return "";
}

function IsEmpty(val) {
    if (!val || val == "") {
        return true;
    }
    return false;
}

function IsNum(val) {
    return parseFloat(val).toString() != "NaN";
}

function ChkNum(val, warning) {
    if (!IsNum(val)) {
        return warning + "應為數字型態 !";
    }
    return "";
}

function ChkNumLen(val, len, warning) {
    if (IsEmpty()) {
        return "";
    }

    var r = ChkNum(val, warning);
    if (r == "") {
        if (val.toString().length < len) {
            return warning + "長度應為" + len + "碼 !";
        }
    } else {
        return r;
    }
    return "";
}

function ChkNumVal(val, min, max, warning) {
    var r = ChkNum(val, warning);
    if (r == "") {
        if (min != "x" && max != "x") {
            return (parseInt(val) < parseInt(min) || parseInt(val) > parseInt(max)) ? "{0}超出允許範圍({1}~{2})".format(warning, min, max) : "";
        } else if (min != "x") {
            return parseInt(val) < parseInt(min) ? "{0}不可低於最小值{1}".format(warning, min) : "";
        } else if (max != "x") {
            return parseInt(val) > parseInt(max) ? "{0}不可超出最大值{1}".format(warning, max) : "";
        }

    } else {
        return r;
    }
    return "";
}

function ChkTbRequired1(targetID) {
    var isValid = "";
    if (targetID != undefined) {
        var $target = '';
        targetID = targetID.replace("#", "");
        $target = $('#' + targetID);
        $('[required]', $target).each(function () {
            var val = $(this).val();
            var txt = $(this).parents('tr').children('th').text();
            txt = IsEmpty(txt) ? $(this).siblings('label').text() : txt;
            if ((!val || val == "")) {
                isValid += txt + "不可為空 !<br>";
                $(this).css('border-color', "red");
                $(this).siblings('.select2').find('.select2-selection').css('border-color', "red");
            }
        });

    }
    return isValid.substring(0, isValid.lastIndexOf('<br'));
}

// 檢查發票號碼
function ChkInvoiceNo(invNo) {
    var rule = /[A-Za-z]{2}[-][0-9]{8}/;
    if (typeof invNo != 'undefined' && invNo != "" && !rule.test(invNo)) {
        return "發票格式錯誤 !";
    }
    return "";
}

// 前後日期
function ChkDateRange(fewDaysAgo, fewDayslater, fn) {
    //var queryDate = '2009-11-01',
    //dateParts = queryDate.match(/(\d+)/g)
    //realDate = new Date(dateParts[0], dateParts[1] - 1, dateParts[2]);
    fewDaysAgo = typeof fewDaysAgo == "undefined" ? "0" : fewDaysAgo;
    fewDayslater = typeof fewDayslater == "undefined" ? "0" : fewDayslater;
    fn = typeof fn == "undefined" ? function () { } : fn;

    var dateFormat = 'yy/mm/dd',
          from = $("#SearchFmDate")
            .datepicker({
                defaultDate: "+1w",
                changeMonth: true,
                numberOfMonths: 1,
                dateFormat: 'yy/mm/dd'
            })
            .datepicker("setDate", fewDaysAgo)
            .on("change", function () {
                to.datepicker("option", "minDate", getDate(this));
            }),
          to = $("#SearchToDate").datepicker({
              defaultDate: "+1w",
              changeMonth: true,
              numberOfMonths: 1,
              dateFormat: 'yy/mm/dd'
          })
            .datepicker("setDate", fewDayslater)
           .on("change", function () {
               from.datepicker("option", "maxDate", getDate(this));
           });
    function getDate(element) {
        var date;
        try {
            date = $.datepicker.parseDate(dateFormat, element.value);
        } catch (error) {
            date = null;
        }

        return date;
    }
    fn();
}