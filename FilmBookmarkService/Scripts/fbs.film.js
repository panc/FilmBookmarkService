function initilizeFilmHandler(settings) {

    $(document).on('click', '#addFilm', function () {
        var film = {
            name: $('#newFilmName').val(),
            link: $('#newLink').val(),
            season: $('#newSeason').val(),
            episode: $('#newEpisode').val()
        };
        
        $.post(settings.addFilmUrl, film, function(data) {

            if (data.success)
                alert('success');
            else
                alert(data.message);
        })
        .fail(function () {
            alert("Failed to add the new film!");
        });
    });
}