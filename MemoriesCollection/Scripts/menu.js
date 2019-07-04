jQuery(document).ready(function () {
    var accordionsMenu = $('#menu .cd-accordion-menu');

    if (accordionsMenu.length > 0) {

        accordionsMenu.each(function () {
            var accordion = $(this);
            //detect change in the input[type="checkbox"] value
            accordion.on('click', '[menu]', function (e) {
                var checkbox = $(this);
                var href = checkbox.attr('href');
                if (checkbox.attr('close')) {
                    checkbox.siblings('ul').attr('style', 'display:block;').slideUp(300);
                    checkbox.removeAttr('close');
                } else {
                    checkbox.siblings('ul').attr('style', 'display:none;').slideDown(300);
                    checkbox.attr('close', 'Y');
                }
                if ((href == "#" || href == "")) {
                    e.preventDefault();
                    $('#menu').animate({
                        scrollTop: window.innerHeight
                    }, 500).hide();
                } else {
                    checkbox.attr('close', 'Y');
                    location.href = href;
                }
            });
        });
    }

    $("#menu-trigger").on('click', function () {
        $('#menu').is(':visible') ? $('#menu').hide('slide', { direction: 'left' }) : $('#menu').show('slide', { direction: 'left' });
    });

    $(document).on('click', "#navbarContent a", function () {
        $('#navbarContentBtn').trigger('click');
    });
});