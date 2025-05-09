$(function () {
    // Initialize price range slider
    var minVal = parseInt($("#min-price").val()) || 0;
    var maxVal = parseInt($("#max-price").val()) || 1000;

    if ($("#price-range").length) {
        $("#price-range").slider({
            range: true,
            min: 0,
            max: 1000,
            values: [minVal, maxVal],
            slide: function (event, ui) {
                $("#min-price").val(ui.values[0]);
                $("#max-price").val(ui.values[1]);
            }
        });
    }

    // Quick View functionality
    $(document).on('click', '.quick-view-btn', function (e) {
        e.preventDefault();
        var productId = $(this).data('id');
        if (productId) {
            $('#quickViewModal').modal('show');
            $('#quickViewContent').load('/Products/QuickView/' + productId, function (response, status, xhr) {
                if (status == "error") {
                    $('#quickViewContent').html('<div class="alert alert-danger">Error loading product details</div>');
                }
            });
        }
    });

    // Search autocomplete
    if ($('#search-input').length) {
        $('#search-input').autocomplete({
            source: function (request, response) {
                $.getJSON('/Products/SearchSuggestions', { term: request.term }, function (data) {
                    response(data);
                });
            },
            minLength: 2,
            select: function (event, ui) {
                if (ui.item && ui.item.id) {
                    window.location.href = '/Products/Details/' + ui.item.id;
                }
            }
        }).data('ui-autocomplete')._renderItem = function (ul, item) {
            return $('<li>')
                .append('<div><img src="' + (item.image || '/images/default-product.png') + '" class="search-thumbnail me-2" width="40"/> ' + item.label + '</div>')
                .appendTo(ul);
        };
    }

    // Update price display when range changes
    $('input[name="minPrice"], input[name="maxPrice"]').on('change', function () {
        $('#filter-form').trigger('submit');
    });

    // Star rating interaction
    $(document).on('mouseenter', '.rating-input label', function () {
        $(this).siblings('label').css('color', '#ddd');
        $(this).css('color', '#ffc107').prevAll('label').css('color', '#ffc107');
    }).on('mouseleave', '.rating-input', function () {
        var checked = $(this).find('input:checked');
        $(this).find('label').css('color', '#ddd');
        if (checked.length) {
            checked.nextAll('label').addBack('label[for="' + checked.attr('id') + '"]').css('color', '#ffc107');
        }
    });
});