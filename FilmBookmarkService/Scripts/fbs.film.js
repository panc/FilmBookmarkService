function initializeFilmHandler(settings) {

    // define needed methods
    var showLoadingIndicator = function (filmElement) {

        filmElement.find('.film-view .stream').html('<div class="loading-indicator"/>');
    };

    var updateEpisodeAvailableIndicator = function (filmElement, isAnotherEpisodeAvailable) {

        filmElement.find('.indicator').toggle(isAnotherEpisodeAvailable);
    };

    var updateStreamUrl = function (filmElement, result) {
        var html = 'Failure';

        if (result.success)
            html = '<a href="' + result.streamUrl + '" class="control-button" target="_blank">PLAY &nbsp;&nbsp;<span class="glyphicon glyphicon-play"></a>';

        filmElement.find('.film-view .stream').html(html);
    };

    var onMirrorChanged = function (filmElement, selection) {
        var id = filmElement.attr('id');

        showLoadingIndicator(filmElement);

        var val = selection.val();
        $.post(settings.getStreamUrl + "/" + id, { url: val }, function (data) { updateStreamUrl(filmElement, data); });
    }

    var updateMirrors = function (filmElement, result) {
        if (!result.success) {
            filmElement.find('.film-view .stream').html('Failure');
            filmElement.find('.film-view .info').html('Failure');
            return;
        }

        var selection = filmElement.find('.film-view .mirrors select');
        selection.empty();

        $.each(result.mirrors, function (key, mirror) {
            selection
                .append($('<option>', { value: mirror.StreamUrl })
                .text(mirror.Name));
        });

        var text = "Season " + result.season + " episode " + result.episode + " / " + result.numberOfEpisodes;
        filmElement.find('.film-view .info').html(text);

        onMirrorChanged(filmElement, filmElement.find('.film-view .mirrors select'));
    };

    var onEpisodeChanged = function (filmElement, data) {
        if (!data.success) {
            alert(data.message);
            return;
        }

        var editor = filmElement.children('.film-editor');
        editor.children('.edit-season').val(data.season);
        editor.children('.edit-episode').val(data.episode);

        updateMirrors(filmElement, data);
        updateEpisodeAvailableIndicator(filmElement, data.isAnotherEpisodeAvailable);
    };

    // start initial loading
    $('.film').each(function () {
        var element = $(this);
        var id = element.attr('id');

        $.post(settings.getMirrorsUrl + "/" + id, function (data) { onEpisodeChanged(element, data); });
    });

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

    // register document hooks
    $(document).on('click', '#add-button', function () {
        var film = {
            name: $('#new-film-name').val(),
            url: $('#new-url').val(),
            coverUrl: $('#new-cover-url').val(),
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

    $(document).on('click', '.cancel-button', function (e) {
        e.preventDefault();

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

    $(document).on('click', '.save-button', function (e) {
        e.preventDefault();

        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');
        var editor = filmElement.find('.film-editor .row').first();

        var film = {
            name: editor.children('.edit-film-name').val(),
            url: editor.children('.edit-url').val(),
            coverUrl: editor.children('.edit-cover-url').val(),
            season: editor.children('.edit-season').val(),
            episode: editor.children('.edit-episode').val()
        };

        $.post(settings.editFilmUrl + "/" + id, film, function (data) {
            if (!data.success) {
                alert(data.message);
                return;
            }

            showLoadingIndicator(filmElement);

            $.post(settings.getStreamUrl + "/" + id, function (streamData) { updateStreamUrl(filmElement, streamData); });
            //$.post(settings.isAnotherEpisodeAvailableUrl + "/" + id, function (result) { updateEpisodeAvailableIndicator(filmElement, result); });

            var view = filmElement.children('.film-view');
            view.find('.mirrorLink a').html(film.name).attr('href', film.url);

            filmElement.children('.film-view').show();
            filmElement.children('.film-editor').hide();
        })
        .fail(function () {
            alert("Failed to update the film with id " + id + "!");
        });
    });

    $(document).on('click', '.next-episode-button', function () {
        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');

        showLoadingIndicator(filmElement);

        $.post(settings.nextEpisodeUrl + "/" + id, function (data) { onEpisodeChanged(filmElement, data); });
    });

    $(document).on('click', '.prev-episode-button', function () {
        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');

        showLoadingIndicator(filmElement);

        $.post(settings.prevEpisodeUrl + "/" + id, function (data) { onEpisodeChanged(filmElement, data); });
    });

    $(document).on('click', '.favorite', function () {
        var filmElement = $(this).closest('.film');
        var id = filmElement.attr('id');

        $.post(settings.setIsFavoriteUrl + "/" + id, function(data) {

            var isFavoriteElement = filmElement.find('.favorite');
            var icon = data.isFavorite ? '/Content/images/star.png' : '/Content/images/star-disabled.png';
            isFavoriteElement.find('img').attr('src', icon);
        });
    });

    $(document).on('click', '.expand-button', function () {
        var filmElement = $(this).closest('.film');
        
        var isExpanded = filmElement.data('is-expanded');
        filmElement.data('is-expanded', !isExpanded);

        var expandButton = filmElement.find('.expand-button span');
        expandButton.toggleClass("glyphicon-chevron-right").toggleClass("glyphicon-chevron-down");

        var filmDetails = filmElement.find('.film-details');
        filmDetails.toggle();
    });

    $(document).on('change', '.film-view .mirrors select', function() {
         onMirrorChanged($(this).closest('.film'), $(this));
    });
}