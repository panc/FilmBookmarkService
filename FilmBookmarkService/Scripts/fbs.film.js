function initializeFilmHandler(settings) {

    $('.films').sortable({
        handle: ".sort",
        placeholder: "ui-state-highlight",
        delay: 150,
        opacity: 0.7,
        revert: true,
        scroll: true,
        update: function (event, ui) {

            var sortedIds = $('.films').sortable("toArray");

            $.post(settings.updateSortOrderUrl, { positions: sortedIds }, function (data) {
                if (!data.success)
                    alert('Failed to update sortorder!');
            });
        }
    });

    var updateStreamUrl = function (filmElement, result) {
        var html = 'Failure';

        if (result.success) {
            var text = "Watch season " + result.season + " episode " + result.episode;
            html = '<a href="' + result.streamUrl + '" target="_blank">' + text + '</a>';
        }
        
        filmElement.find('.film-view .stream').html(html);
    };

    var updateEpisodeAvailableIndicator = function(filmElement, result) {

        filmElement.find('.indicator').toggle(result.success);
    };

    $('.film').each(function () {
        var element = $(this);
        var id = element.attr('id');

        $.post(settings.getStreamUrl + "/" + id, function (data) { updateStreamUrl(element, data); });
        $.post(settings.isAnotherEpisodeAvailableUrl + "/" + id, function (data) { updateEpisodeAvailableIndicator(element, data); });
    });

    $(document).on('click', '#add-button', function () {
        var film = {
            name: $('#new-film-name').val(),
            url: $('#new-url').val(),
            season: $('#new-season').val(),
            episode: $('#new-episode').val()
        };

        $.post(settings.addFilmUrl, film, function (data) {

            if (data.success)
                location.reload(); // the easiest way to refresh the list without databinding ;-)
            else
                alert(data.message);
        })
        .fail(function () {
            alert("Failed to add the new film!");
        });
    });

    $(document).on('click', '.edit-button', function () {
        var element = $(this).closest('.film');
        element.children('.film-view').hide();
        element.children('.film-editor').show();
    });

    $(document).on('click', '.cancel-button', function () {
        var element = $(this).closest('.film');
        element.children('.film-view').show();
        element.children('.film-editor').hide();
    });

    $(document).on('click', '.remove-button', function () {
        var id = $(this).closest('.film').attr('id');

        $.post(settings.removeFilmUrl, { id: id }, function (data) {

            if (data.success)
                location.reload(); // the easiest way to refresh the list without databinding ;-)
            else
                alert(data.message);
        })
        .fail(function () {
            alert("Failed to remove the film with id " + id + "!");
        });
    });

    $(document).on('click', '.save-button', function() {
        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');
        var editor = filmElement.children('.film-editor');

        var film = {
            name: editor.children('.edit-film-name').val(),
            url: editor.children('.edit-url').val(),
            season: editor.children('.edit-season').val(),
            episode: editor.children('.edit-episode').val()
        };

        $.post(settings.editFilmUrl + "/" + id, film, function(data) {
            if (!data.success) {
                alert(data.message);
                return;
            }

            showLoadingIndicator(filmElement);

            $.post(settings.getStreamUrl + "/" + id, function (streamData) { updateStreamUrl(filmElement, streamData); });
            $.post(settings.isAnotherEpisodeAvailableUrl + "/" + id, function (result) { updateEpisodeAvailableIndicator(filmElement, result); });

            var view = filmElement.children('.film-view');
            view.find('.url a').html(film.name).attr('href', film.url);

            filmElement.children('.film-view').show();
            filmElement.children('.film-editor').hide();
        })
        .fail(function() {
            alert("Failed to remove the film with id " + id + "!");
        });
    });

    $(document).on('click', '.next-episode-button', function () {
        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');

        showLoadingIndicator(filmElement);

        $.post(settings.nextEpisodeUrl + "/" + id, function (data) { onEpisodeChanged(filmElement, id, data); });
    });

    $(document).on('click', '.prev-episode-button', function () {
        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');

        showLoadingIndicator(filmElement);

        $.post(settings.prevEpisodeUrl + "/" + id, function(data) { onEpisodeChanged(filmElement, id, data); });
    });

    var onEpisodeChanged = function (filmElement, id, data) {
        if (!data.success)
            alert(data.message);

        var editor = filmElement.children('.film-editor');
        editor.children('.edit-season').val(data.season);
        editor.children('.edit-episode').val(data.episode);

        updateStreamUrl(filmElement, data);
        $.post(settings.isAnotherEpisodeAvailableUrl + "/" + id, function (result) { updateEpisodeAvailableIndicator(filmElement, result); });
    };

    var showLoadingIndicator = function (filmElement) {
        
        filmElement.find('.film-view .stream').html('<img src="./Content/images/ajax-loader.gif"/>');
    };
}