function initilizeFilmHandler(settings) {

    $('.film').each(function(index) {
        var id = $(this).attr('id');
        $.getJSON(settings.getStreamUrl + "/" + id, function(data) {
            alert(data.streamUrl);
        });
    });
    
    $(document).on('click', '#add-button', function () {
        var film = {
            name: $('#new-film-name').val(),
            url: $('#new-link').val(),
            season: $('#new-season').val(),
            episode: $('#new-episode').val()
        };
        
        $.post(settings.addFilmUrl, film, function(data) {

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
        var id = $(this).closest('.film').attr('id');

        $.post(settings.editFilmUrl, { id: id }, function (data) {

            if (data.success)
                location.reload(); // the easiest way to refresh the list without databinding ;-)
            else
                alert(data.message);
        })
        .fail(function () {
            alert("Failed to remove the film with id " + id + "!");
        });
    });

    $(document).on('click', '.remove-button', function () {
        var id = $(this).closest('.film').attr('id');
        
        $.post(settings.removeFilmUrl, { id: id}, function (data) {

            if (data.success)
                location.reload(); // the easiest way to refresh the list without databinding ;-)
            else
                alert(data.message);
        })
        .fail(function () {
            alert("Failed to remove the film with id " + id + "!");
        });
    });
}