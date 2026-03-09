
window.checkPermissions = async function () {
    try {
        const permissions = navigator.permissions;
        const queryBound = permissions.query.bind(permissions);
        const result = await queryBound({ name: 'geolocation' });
        console.log(result.state);
    } catch (error) {
        console.error('Failed to execute query on Permissions:', error);
    }
};

window.initializeVideoPlayer = function () {
    // Initialize video player without checking permissions
    console.log('Video player initialized');
};

function togglePlayPause() {
    var audio = document.getElementById('audioPlayer');
    var playpause = document.getElementById("playerText");
    var stateText = document.getElementById("stateText");

    var audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    //create, tune, start and connect each oscillator sinea, sineb and sinec
    var sinea = audioCtx.createOscillator();
    sinea.frequency.value = 440;
    sinea.type = "sine";
    sinea.start();
    sinea.connect(audioCtx.destination);
    var sineb = audioCtx.createOscillator();
    sineb.frequency.value = 523.25;
    sineb.type = "sine";
    sineb.start();
    sineb.connect(audioCtx.destination);
    var sinec = audioCtx.createOscillator();
    sinec.frequency.value = 698.46;
    sinec.type = "sine";
    sinec.start();
    sinec.connect(audioCtx.destination);

    audio.onended = onEnded;
    audio.onpause = onPause;
    audio.onplay= onPlay;
    if (audio.paused || audio.ended) {
        if (audio.ended)
            playpause.textContent = "Play";
        else
        playpause.textContent = "Pause";
        audio.play();
    } else {
        playpause.textContent = "Play";
        audio.pause();
    }

    function onPlay() {
        stateText.textContent = "Now Playing...";
    }

    function onPause() {
        stateText.textContent = "Audio Paused";
    }

    function onEnded() {
        stateText.textContent = "Audio Ended";
        playpause.textContent = "Play";
    }
}
