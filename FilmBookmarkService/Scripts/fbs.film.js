function initilizeFilmHandler(settings) {

    var updateStream = function (filmElement) {
        var id = filmElement.attr('id');

        $.getJSON(settings.getStreamUrl + "/" + id, function (data) {
            if (!data.success) {
                // todo
                return;
            }

            var text = "Watch season " + data.season + " episode " + data.episode;

            filmElement.find('.film-view .stream').html('<a href="' + data.streamUrl + '" target="_blank">' + text + '</a>');
        });
    };

    $('.film').each(function () {
        var filmElement = $(this);
        updateStream(filmElement);
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

            updateStream(filmElement);

            var view = filmElement.children('.film-view');
            view.children('.film-name').html(film.name);
            view.find('.url a').html(film.url).attr('href', film.url);

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

        $.post(settings.nextEpisodeUrl + "/" + id, function (data) {

            var editor = filmElement.children('.film-editor');
            editor.children('.edit-season').val(data.season);
            editor.children('.edit-episode').val(data.episode);

            updateStream(filmElement);
        });
    });

    $(document).on('click', '.prev-episode-button', function () {
        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');

        $.post(settings.prevEpisodeUrl + "/" + id, function (data) {

            var editor = filmElement.children('.film-editor');
            editor.children('.edit-season').val(data.season);
            editor.children('.edit-episode').val(data.episode);

            updateStream(filmElement);
        });
    });
}