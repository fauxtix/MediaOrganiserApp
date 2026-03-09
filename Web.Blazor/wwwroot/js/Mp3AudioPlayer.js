var isPlaying = false;
window.PlayAudioFile = (audioSrc) => {
    var audio = document.getElementById('audioPlayer');
    var playBtn = document.getElementById('playerPlayBtn');
    var pauseBtn = document.getElementById('playerPauseBtn');
    var stopBtn = document.getElementById('playerStopBtn');
    var waveAnimation = document.getElementById('bars-animation');

    var coverImage = document.getElementById('myImage');

    if (audio != null) {

        audio.onended = function () {
            StopPlayAudioFile();
        };

        var audioSource = document.getElementById('audioSource');
        if (audioSource != null) {
            if (!isPlaying) {
                audioSource.src = audioSrc;
                audio.volume = 0.5;

                // only loads the audio if it's not paused (currenttime > 0)
                if (audio.currentTime == 0) {
                    coverImage.style.display = "block";
                    waveAnimation.style.display = "none";
                    audio.load();
                }
                isPlaying = true;
                coverImage.style.display = "none";
                waveAnimation.style.display = "block";

                (playBtn).disabled = true;
                (pauseBtn).disabled = false;
                (stopBtn).disabled = false;

                audio.play();
            }
        }
    }
    else {
        window.alert('File not found!');
    }
}

window.PauseAudioFile = () => {
    var playBtn = document.getElementById('playerPlayBtn');
    var pauseBtn = document.getElementById('playerPauseBtn');
    var stopBtn = document.getElementById('playerStopBtn');
    var audio = document.getElementById('audioPlayer');
    var waveAnimation = document.getElementById('bars-animation');
    var coverImage = document.getElementById('myImage');
    if (audio != null) {
        isPlaying = false;
        audio.pause();
        (playBtn).disabled = false;
        (pauseBtn).disabled = true;
        (stopBtn).disabled = false;
        coverImage.style.display = "block";
        waveAnimation.style.display = "none";

    }
    else {
        window.alert('File not found!');
    }
}

// reset time, go to start
window.StopPlayAudioFile = () => {
    var playBtn = document.getElementById('playerPlayBtn');
    var pauseBtn = document.getElementById('playerPauseBtn');
    var stopBtn = document.getElementById('playerStopBtn');
    var audio = document.getElementById('audioPlayer');
    var waveAnimation = document.getElementById('bars-animation');
    var coverImage = document.getElementById('myImage');

    if (audio != null) {
        isPlaying = false;
        audio.pause();
        audio.currentTime = 0;
        (playBtn).disabled = false;
        (pauseBtn).disabled = true;
        (stopBtn).disabled = true;
        coverImage.style.display = "block";
        waveAnimation.style.display = "none";
    }
    else {
        window.alert('File not found!');
    }
}

window.DisposeAudioFile = () => {
    var audio = document.getElementById('audioPlayer');
    var audioSource = document.getElementById('audioSource');

    StopPlayAudioFile();

    if (audioSource != null) {
        audioSource.src = "";
        audio.DisposeAudioFile;
        //audio.dispose;
    }

}

function InitAudio(element, reference) {
    isPlaying = false;
    var audio = document.getElementById('audioPlayer');
    if (audio != null) {
        audio.addEventListener("ended", async e => {
            await reference.invokeMethodAsync("OnEnd");
        });
    }

    //if (element != null) {
    //    element.addEventListener("ended", async e => {
    //        await reference.invokeMethodAsync("OnEnd");
    //    });
//    }
}


window.PlayVideo = (src) => {
    var video = document.getElementById('videoPlayer');
    if (video != null) {
        var videoSource = document.getElementById('videoSource');
        if (video != null) {
            video.play();
        }
        else {
            window.alert("video = null!");
        }
    }
}

window.PauseVideo = (src) => {
    var video = document.getElementById('videoPlayer');
    if (video != null) {
        var videoSource = document.getElementById('videoSource');
        if (video != null) {
            video.pause();
        }
        else {
            window.alert("video = null!");
        }
    }
}

window.StopVideo = (src) => {
    var iFrame = document.getElementsByTagName("iframe")
    //$('.embed-responsive-item').each(function () {
    //    this.contentWindow.postMessage('{"event":"command","func":"stopVideo","args":""}', '*')
    //});
    var video = document.getElementById('videoPlayer');
    //            var videoSource = document.getElementById('videoSource');
    if (video != null) {
        if (video != null) {
            video.src = "";
            //                    videoSource.src = "";
            video.dispose;
        }
        else {
            window.alert("video = null!");
        }
    }
    //var player = videojs('videoPlayer');

    //player.on('ended', function () {
    //    this.dispose();
    //});
}


function playPause() {
    if (video != null) {
        if (video.paused)
            video.play();
        else
            video.pause();
    }
    else {
        window.alert("video = null!");
    }
}

function makeBig() {
    video.width = 560;
}

function makeSmall() {
    video.width = 320;
}

function makeNormal() {
    video.width = 420;
}
window.PlayVideo = (src) => {
    var video = document.getElementById('videoPlayer');
    if (video != null) {
        var videoSource = document.getElementById('videoSource');
        if (video != null) {
            video.play();
        }
        else {
            window.alert("video = null!");
        }
    }
}

window.PauseVideo = (src) => {
    var video = document.getElementById('videoPlayer');
    if (video != null) {
        var videoSource = document.getElementById('videoSource');
        if (video != null) {
            video.pause();
        }
        else {
            window.alert("video = null!");
        }
    }
}

window.StopVideo = (src) => {
    var iFrame = document.getElementsByTagName("iframe")
    //$('.embed-responsive-item').each(function () {
    //    this.contentWindow.postMessage('{"event":"command","func":"stopVideo","args":""}', '*')
    //});
    var video = document.getElementById('videoPlayer');
    //            var videoSource = document.getElementById('videoSource');
    if (video != null) {
        if (video != null) {
            video.src = "";
            //                    videoSource.src = "";
            video.dispose;
        }
        else {
            window.alert("video = null!");
        }
    }
    //var player = videojs('videoPlayer');

    //player.on('ended', function () {
    //    this.dispose();
    //});
}


function playPause() {
    if (video != null) {
        if (video.paused)
            video.play();
        else
            video.pause();
    }
    else {
        window.alert("video = null!");
    }
}

function makeBig() {
    video.width = 560;
}

function makeSmall() {
    video.width = 320;
}

function makeNormal() {
    video.width = 420;
}

