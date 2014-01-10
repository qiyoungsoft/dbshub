
/**********  顺序执行n个函数 ****************/
var seq_async = function (funcs, callback) {
    this.funcs = funcs;
    this.callback = callback;
}

seq_async.prototype = {
    constructor: seq_async,
    execOne: function (params, index) {
        var obj = this;
        this.funcs[index](params, function (data) {
            if (arguments.length <= 1) data = data;
            else data = arguments;

            if (index == obj.funcs.length - 1)
                obj.callback(data);
            else obj.execOne(data, index + 1);
        })
    },
    exec: function () {
        this.execOne(null, 0);
    }
}

/**********  只执行一个函数，执行次数由数组决定     ***********/
var seq_asyncArray = function (func, paramsArray, callback) {
    this.func = func;
    this.paramsArray = paramsArray;
    this.callback = callback;
}
seq_asyncArray.prototype = {
    constructor: seq_asyncArray,
    execOne: function (params, index) {
        var obj = this;
        this.func(this.paramsArray[index], params, function (data) {
            if (arguments.length <= 1) data = data;
            else data = arguments;

            if (index == obj.paramsArray.length - 1) {
                if (obj.callback ) obj.callback(data);
            }
            else obj.execOne(data, index + 1);
        })
    },
    exec: function () {
        if (this.paramsArray.length > 0) this.execOne(null, 0);
        else this.callback(null);
    }
}

$.idc = {
    dialog: function (option) {
        $("div.idcDialog").remove();
        return $("<div>").addClass('idcDialog').appendTo('body').dialog(
                $.extend($.idc.dialogSize(), { modal: true, title: option.title, buttons: {
                        Ok: function () {
                            if (option.confirm()) $(this).dialog("close");
                        }
                    }
                })).load(option.url);
    },
    dialogSize: function () {
        return { width: $(window).width() * 0.80, height: $(window).height() * 0.80 };
    },
    //精度截取
    precNumber: function (value, precision) {
        if (value == "") return "";
        if(typeof value != "number")
            try{value = parseFloat(value);} catch(e){ return value;}
        if(typeof precision != "number") precision = parseInt(precision);
        if (precision != 0) {
            var p = Math.pow(10, precision);
            value = Math.round(value * p ) /p;
        } else value = Math.round(value);

        return value;
    },
    //加货币符，不够精度补0
    formatNumber: function (value, precision, money) {
        value = $.idc.precNumber(value, precision);
        if (precision > 0){
            var vv = value.toString().split(".");
            if (vv.length == 1) value = value + ".";
            for(var i = 0; i < (precision - (vv.length > 1? parseInt(vv[1].length): 0)); i++) value = value + "0";
        }
        return (money?"￥":"") + value;
    },
    onUpload :function (file, from) {
        $(':input#' + from).val(file).trigger('change.upload').change();
    },
    skipReplace: function(skip, str, src, dest){
        if (skip) {
            str = str.replace(src, dest);
            if (str.indexOf(src) >= 0) return str.replace(src, dest).replace( dest, src);
            else return str;
        }
        else return str.replace(src, dest);
    }
}

$.fn.tmplRow = function (option){
    return this.each(function(){
        if (option && option.newRow){
            var row = $(this);
            var idx = row.parent().children().size() - 1;
            var r = row.clone();

            if (option && option.tmplVisible) r.insertBefore(row);
            else { r.appendTo(row.parent());}
            if (r.attr('idx')) r.attr('idx', idx);
            
            r.removeClass('tmpl').children().each(function () {
                var inputs = $(this).find(':input');
                var fn = $(this).attr('fn');
                var value = option&& option.data? option.data[fn]:null;
                if (inputs.size() > 0) {
                    inputs.each(function(){
                        var input = $(this);
                        if (input.attr('_name')) //这里要检查下
                            input.attr('name', input.attr('_name').replace('[-1]', '[' + idx + ']')).removeAttr('_name');
                        if (input.attr('id')) input.attr('id', input.attr('id').replace('_-1_', '_' + idx + '_'));
                    });
                    if (option && option.data) inputs.val(value);
                    
                } else if ($(this).attr('fn')) $(this).text(value)
                else {
                    $(this).find('.editors').trigger("clone", [idx]);
                }
            });
            if (option && option.callback) option.callback(r);
        }
        else if (option && option.delRow){
            if (confirm("确认要删除吗?")) {
                var row = $(this);
                var idx = parseInt(row.attr('idx') ? row.attr('idx') : row[0].sectionRowIndex);
                row.parent().children().slice( idx + 1).each(function () {
                    var row1 = $(this);
                    var idx1 = parseInt(row1.attr('idx') ? row1.attr('idx') : row1[0].sectionRowIndex);
                    var old = (idx1 + (option.tmplVisible?0:-1)); var New = old -1;
                    //alert(old);
                    row1.find('[name]').each(function () {
                        $(this).attr('name', $.idc.skipReplace(option.skipReplace, $(this).attr('name'), "[" + old + "]", "[" + New + "]"));
                        if ($(this).attr('id')) $(this).attr('id', $.idc.skipReplace(option.skipReplace, $(this).attr('id'), "_" + old + "_", "_" + New + "_"));
                    });
                    if (row1.attr('idx')) row1.attr('idx', New);
                });
            };
        }
        else{
            var row = $(this);
            row.find(':input').each(function(){
                $(this).attr('_name', $(this).attr('name')).removeAttr('name');
            }).on('change.tmpl', function () {
                row.tmplRow({newRow: true, tmplVisible: option && option.tmplVisible});
                $(this).val('').trigger('change.reset');
            });
        }
    });
}
 

$.fn.AjaxLoad = function(option){
    return this.each(function(){
        if (option.form){
            $(this).click(function(){
                var form = $(option.form);
                $.get(form.attr("action"), form.serializeArray(), function(data){
                   $(option.container).html(data); 
                });
                return false;
            });        
        }
        else if ($(this).attr('href')){
            $(this).click(function(){
                $(option.container).load($(this).attr('href')); 
                return false;
            });
        }
    });
}

$.fn.waitting = function(){
    return this.each(function(){
    });
}

$.fn.AjaxPost = function () {
    return this.each(function () {
        $(this).click(function () {
            var _confirm = $(this).attr('confirm');
            var href = $(this).attr('href');
            if (!_confirm || confirm(_confirm)) {
                var form = $(".pageBody form:not(.imgForm)");
                if (form.validateFormat()) {
                    var url = $(this).attr('action');
                    if (!url) url = form.attr('action');
                    tinyMCE.triggerSave();
                    var data = form.serializeArray();
                    $.post(url, data, function (da) {
                        if (da.IsValid) {
                            if (da.Message) alert(da.Message); else alert("操作成功");
                            window.location = da.Href ? da.Href : (href ? href : window.location.toString());
                        }
                        else form.HandleError(da.Errors);
                        return false;
                    });
                } else alert("格式错误");
            }
            return false;
        });

    });
}

$.fn.HandleError = function (errors) {
    return this.each(function () {
        
        var c = $(this).find('[name]').removeClass('modelError').end();
        alert(_.map(errors, function (i) { return i.ErrorMessage; }).join("\n"));
        _.each(errors, function (i) {
            c.find("[name='" + i.MemberNames[0] + "']").addClass('modelError').attr('title', i.ErrorMessage);
        });

    });
}
 
$.fn.Table = function (option) {
    return this.each(function () {
        var grid = $(this);
        var keys = grid.attr('keyFields');
        if (option && option.newRow) {
            var duplicate = false;
            if (keys) {
                var keyValue = option.data[keys];
                if (grid.find("tbody td[key] :input").filter(function () { return $(this).val() == keyValue; }).size() > 0)
                    duplicate = true;
            }
            if (duplicate) alert(keyValue + "重复了.");
            else {
                grid.find('.t-grid-content tbody tr.tmpl').tmplRow({ newRow: true, data: option.data });
            }
        }
        else {
            var sel = grid.attr('sel');
            if (sel) {
                grid.on('click', "tbody tr:not(.tmpl)", function () {
                    var trs = grid.find("tbody tr");
                    if (sel == 'single') trs.removeClass('sel');
                    $(this).toggleClass('sel');
                });
            }
            grid.find("a.delRow").button().end().on("click", "a.delRow", function () {
                $(this).closest('tr').tmplRow({delRow: true, tmplVisible: false});
            });
            grid.find('tr.tmpl').tmplRow({tmplVisible: true});
            grid.find("a.newRow").button().click(function () {
                grid.find('table').children('tbody').children('tr.tmpl')
                        .tmplRow({
                            newRow: true, tmplVisible: false, callback: function (r) {
                                r.find('.Editor').Editor();
                            }
                        });
                return false;
            });
            grid.children("table").children("tbody").children("tr:even").addClass("t-alt");
            grid.find('.pageNumber').keydown(function(e){
                if (e.which == 13) {
                    var p = parseInt($(this).val());
                    if(p) window.location = $(this).attr('url').replace($(this).attr('rep'), 'page=' + p);
                }
            });
        }
    });
}
  
$.fn.Pictures = function(option){
    return this.each(function(){
        var ul = $(this).find('ul').children('li.tmpl').tmplRow({tmplVisible: true}).end();
        var form = $('body').find('.imgForm');
        var imgGroup = $(this).attr("fn");
        //.appendTo('body');
        $(this).find(".uploadImg").change(function () {
            form.find(':file').remove();
            $(this).clone().appendTo(form);
            form[0].submit();
            $(this).val('');
            return false;
        }).end().on('click', 'a.img', function () {
            
            $.idc.dialog({
                url: $(this).find('img').attr('src').replace("downloadimg", "ViewImg"),
                buttons: {
                    Cancel: function () { $(this).dialog("close"); }
                }
            });
            return false;
        }).on('click', 'a.delRow', function(){
            $(this).closest('li').tmplRow({delRow: true, tmplVisible: true});
            $.post('/util/deleteimg/' + $(this).attr('imgId') + 'type=prodImage')
            return false;
        });
 
        ul.on('change.upload', "li input", function () {
            $(this).parent().find('img').attr('src', '/util/downloadimg/' + $(this).val() + "?type=prodImage");
        });
    });
}
 
$.fn.Editor = function () {
    return this.each(function () {
        if ($(this).hasClass('t-numerictextbox')) {
            var input = $(this).find('input').focus(function(){
                $(this).addClass("focus").parent().children('.t-formatted-value').hide();
            }).blur(function(){
                $(this).removeClass("focus").parent().children('.t-formatted-value').show();
            }).change(function(){
                $(this).trigger('change.reset');
            }).bind('change.reset', function () {
                var p = $(this).parent();
                var v = $.idc.precNumber($(this).val(), p.attr("precision"));
                $(this).val(v);
                p.children('.t-formatted-value').text($.idc.formatNumber(v, p.attr("precision"), p.attr("money")));
            });
            $(this).on('click', '.t-arrow-up', function(){
                var v = parseFloat(input.val());
                input.val(!v &&(v!=0) ? 1: v + 1).change(); 
                return false;
            });
            $(this).find('.t-arrow-down').click(function(){
                var v = parseFloat(input.val());
                input.val(!v &&(v!=0) ? 1: v - 1).change(); 
                return false;
            });
        }
        else if ($(this).hasClass('Date')){
            $(this).datepicker({ "dateFormat": "yy-mm-dd"});
        } else if ($(this).hasClass('DateTime')){
            $(this).datepicker({ "dateFormat": "yy-mm-dd"});
        }
        else if ($(this).attr('richType')) {
            $(this).tinyMCE();
        }
        if ($(this).is(':checkbox')) {
            if ($(this).attr('readonly'))
                $(this).attr('disabled', 'disabled');
        }
    });
}

$.fn.validateFormat = function ( ) {
    this.each(function () {
        var binded = $(this).attr('vfbind');
        $(this).find('[fn]').each(function () {
            if (!binded)
                $(this).blur(function () {
                    var f = $(this).attr('regex');
                    $(this).removeClass("formatError").removeAttr('title');
                    if ((f) && ($(this).val())) {
                        var reg = eval("/" + f + "/");
                        if (!reg.exec($(this).val())) {
                            $(this).addClass("formatError");
                            $(this).attr("title", $(this).attr('formatErrorMsg'));
                        }
                    }
                })
            $(this).trigger('blur');
        });
    });
    return this.find('.formatError').size() == 0;
}
   