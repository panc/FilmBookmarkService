function initilizeFilmHandler(settings) {

    $(document).on('click', '.edit-button', function() {
        alert("Hello World: " + settings.headLine);
    });
}