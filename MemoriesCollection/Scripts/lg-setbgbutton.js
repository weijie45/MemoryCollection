(function ($, window, document, undefined) {

    'use strict';

    var defaults = {
        setBg: false
    };

    var SetBg = function (element) {

        // You can access all lightgallery variables and functions like this.
        this.core = $(element).data('lightGallery');

        this.$el = $(element);
        this.core.s = $.extend({}, defaults, this.core.s)

        this.init();

        return this;
    }

    SetBg.prototype.init = function () {
        if (this.core.s.setBg) {
            var deleteIcon = '<span id="lg-setBg" class="lg-icon setAlbumBg"><span class="fa fa-thumb-tack"></span></span>';
            this.core.$outer.find('.lg-toolbar').append(deleteIcon);

            this.setAlbumBg();
        }

    };


    SetBg.prototype.setAlbumBg = function () {
        var that = this;
        this.core.$outer.find('.setAlbumBg').on('click', function () {

            var index = that.core.index;
            var $sel = $('#' + that.core.$el.children()[index].getAttribute('id')).find('img');
            SetAlbumBg($sel);

        });
    }

    /**
     * Destroy function must be defined.
     * lightgallery will automatically call your module destroy function
     * before destroying the gallery
     */
    SetBg.prototype.destroy = function () {

    }

    // Attach your module with lightgallery
    $.fn.lightGallery.modules.setBg = SetBg;


})(jQuery, window, document);