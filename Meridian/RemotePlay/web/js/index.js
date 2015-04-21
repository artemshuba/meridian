$(document).ready(function () {
    var positionChanging = false;

    var play = document.getElementById("play");
    var pause = document.getElementById("pause");
    var prev = document.getElementById("prev");
    var next = document.getElementById("next");
    var playerControls = document.getElementById("playerControls");

    var currentAudioTitle = document.getElementById("currentAudioTitle");
    var currentAudioArtist = document.getElementById("currentAudioArtist");

    var currentTime = document.getElementById("currentTime");
    var duration = document.getElementById("duration");

    play.onclick = onPlay;
    pause.onclick = onPause;
    next.onclick = onNext;
    prev.onclick = onPrev;

    var progressBar = $("#progressBar");
    progressBar.slider({
        range: "min",
        value: 0,
        slide: onSliderChanged
    });


    var volumeBar = $("#volumeBar");
    volumeBar.slider({
        range: "min",
        max: 100,
        slide: onVolumeSliderChanged
    });

    api("/isPlaying", null, null, function (isPlaying) {
        setIsPlaying(isPlaying);
    });

    api("/volume", null, null, function (volume) {
        volumeBar.slider({ value: volume });
    });

    setInterval(function () {
        api("/isPlaying", null, null, function (isPlaying) {
            setIsPlaying(isPlaying);
        });

        api("/currentTrack", null, null, function (trackInfo) {
            currentAudioTitle.innerText = trackInfo.track.title;
            currentAudioArtist.innerText = trackInfo.track.artist;

            currentTime.innerText = trackInfo.currentTime.toString().toShortTimeString();
            duration.innerText = trackInfo.duration.toString().toShortTimeString();

            if (!positionChanging)
                progressBar.slider({ value: trackInfo.currentTime, max: trackInfo.duration });
        });
    }, 1000);


    function onPlay() {
        api("", "play", null, function (ok) {
            if (ok)
                setIsPlaying(true);
        });
    }


    function onPause() {
        api("", "pause", null, function (ok) {
            if (ok)
                setIsPlaying(false);
        });
    }

    function onNext() {
        api("", "next", null, function (ok) {

        });

    }

    function onPrev() {
        api("", "prev", null);
    }

    function onSliderChanged(event, ui) {
        positionChanging = true;

        api("", "seek", ui.value, function (ok) {
            currentTime.innerText = ui.value.toString().toShortTimeString();
        });

        positionChanging = false;
    }


    function onVolumeSliderChanged(event, ui) {
        api("", "volume", ui.value, function (ok) {

        });
    }

    function setIsPlaying(isPlaying) {
        if (isPlaying) {
            playerControls.className = 'state-play';
        } else {
            playerControls.className = 'state-pause';
        }
    }

    function api(method, command, commandParam, cb) {
        var postData = {};
        if (command)
            postData = { v: 1, command: command, commandParam: commandParam };

        //Safari on iOS and IE on Windows Phone doesn't most of time doesn't send data in the post
        //so using x-data header to pass commands instead of request body
        $.ajax({
            url: "/api" + method,
            cache: false,
            headers: { "cache-control": "max-age=0", "x-data": JSON.stringify(postData)},
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            //data: JSON.stringify(postData),
            success: function (data) {
                cb(data.response);
            }
        });
    }
});