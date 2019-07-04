(function ($, window, document, undefined) {

    'use strict';

    var defaults = { editPic: true };

    var Edit = function (element) {

        // You can access all lightgallery variables and functions like this.
        this.core = $(element).data('lightGallery');

        this.$el = $(element);
        this.core.s = $.extend({}, defaults, this.core.s)

        this.init();

        return this;
    }

    Edit.prototype.init = function () {
        if (this.core.s.editPic) {
            var deleteIcon = '<span id="lg-edit" class="lg-icon editPicture"><span class="fa fa-file-text-o"></span></span>';
            this.core.$outer.find('.lg-toolbar').append(deleteIcon);

            this.edit();
        }

    };


    Edit.prototype.edit = function () {
        var that = this;
        this.core.$outer.find('.editPicture').on('click', function () {

            var index = that.core.index;
            var $sel = $('#' + that.core.$el.children()[index].getAttribute('id')).find('img');
            PhotoDetail($sel.attr('data-params'));

        });
    }

    /**
     * Destroy function must be defined.
     * lightgallery will automatically call your module destroy function
     * before destroying the gallery
     */
    Edit.prototype.destroy = function () {

    }

    // Attach your module with lightgallery
    $.fn.lightGallery.modules.edit = Edit;


})(jQuery, window, document);