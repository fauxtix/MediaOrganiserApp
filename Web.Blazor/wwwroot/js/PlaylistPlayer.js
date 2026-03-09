(function () {
  const ns = (window.playlistPlayerInterop = window.playlistPlayerInterop || {});

  function isMediaElement(el) {
    return el && typeof el.pause === "function" && typeof el.play === "function";
  }

  function nextPlayToken(audioElement) {
    const t = (audioElement.__pp_playToken || 0) + 1;
    audioElement.__pp_playToken = t;
    return t;
  }

  function isCurrentToken(audioElement, token) {
    return (audioElement.__pp_playToken || 0) === token;
  }

  function waitForCanPlay(mediaElement, timeoutMs) {
    if (!isMediaElement(mediaElement)) return Promise.resolve(false);
    if ((mediaElement.readyState || 0) >= 3) return Promise.resolve(true); 

    const ms = timeoutMs ?? 4000;

    return new Promise((resolve) => {
      let done = false;

      const cleanup = () => {
        mediaElement.removeEventListener("canplay", onCanPlay);
        mediaElement.removeEventListener("error", onError);
      };

      const finish = (ok) => {
        if (done) return;
        done = true;
        cleanup();
        resolve(ok);
      };

      const onCanPlay = () => finish(true);
      const onError = () => finish(false);

      mediaElement.addEventListener("canplay", onCanPlay, { once: true });
      mediaElement.addEventListener("error", onError, { once: true });

      setTimeout(() => finish((mediaElement.readyState || 0) >= 3), ms);
    });
  }

  // Prevent internal "pause()" (track swap/stop) from flipping Blazor UI to paused.
  function suppressPauseOnce(mediaElement, action) {
    if (!isMediaElement(mediaElement)) return;

    mediaElement.__pp_suppressPause = true;
    try {
      action();
    } finally {
      setTimeout(() => { mediaElement.__pp_suppressPause = false; }, 0);
    }
  }

  // ---------------------------
  // AUDIO (HTMLAudioElement)
  // ---------------------------
  ns.initAudio = function (audioElement, dotNetRef) {
    if (!isMediaElement(audioElement) || !dotNetRef) return;

    // Prevent double wiring
    if (audioElement.__pp_handlers) return;

    audioElement.__pp_dotNetRef = dotNetRef;

    const onEnded = async () => {
      try { await dotNetRef.invokeMethodAsync("OnAudioEnded"); } catch { }
    };

    const onTimeUpdate = async () => {
      const now = performance.now();
      const last = audioElement.__pp_lastTimeUpdate || 0;
      if (now - last < 250) return;
      audioElement.__pp_lastTimeUpdate = now;

      try {
        await dotNetRef.invokeMethodAsync(
          "OnAudioTimeUpdate",
          audioElement.currentTime || 0,
          audioElement.duration || 0
        );
      } catch { }
    };

    const onLoadedMeta = async () => {
      try { await dotNetRef.invokeMethodAsync("OnAudioMetadata", audioElement.duration || 0); } catch { }
    };

    const onError = async () => {
      try { await dotNetRef.invokeMethodAsync("OnAudioError"); } catch { }
    };

    const onPlay = async () => {
      // Snapshot the token that is currently playing.
      // Any pause event that arrives after the token changes is from a previous track swap
      // and must not flip the UI back to "not playing".
      audioElement.__pp_playingToken = audioElement.__pp_playToken || 0;

      try { await dotNetRef.invokeMethodAsync("OnMediaStateChanged", true); } catch { }
    };

    const onPause = async () => {
      // Ignore pauses caused by our own "reset before play/stop"
      if (audioElement.__pp_suppressPause) return;

      // Ignore stale pause events from a previous track
      const playingToken = audioElement.__pp_playingToken || 0;
      const currentToken = audioElement.__pp_playToken || 0;
      if (playingToken !== currentToken) return;

      try { await dotNetRef.invokeMethodAsync("OnMediaStateChanged", false); } catch { }
    };

    audioElement.__pp_handlers = { onEnded, onTimeUpdate, onLoadedMeta, onError, onPlay, onPause };

    audioElement.addEventListener("ended", onEnded);
    audioElement.addEventListener("timeupdate", onTimeUpdate);
    audioElement.addEventListener("loadedmetadata", onLoadedMeta);
    audioElement.addEventListener("error", onError);
    audioElement.addEventListener("play", onPlay);
    audioElement.addEventListener("pause", onPause);
  };

  // Single-call helper: set source -> wait -> play (token cancels stale attempts)
  ns.setAudioSourceAndMaybePlay = async function (audioElement, src, volume01, autoplay) {
    if (!isMediaElement(audioElement)) return;

    const token = nextPlayToken(audioElement);
    const s = src || "";

    suppressPauseOnce(audioElement, () => {
      audioElement.pause();
      audioElement.currentTime = 0;
    });

    audioElement.src = s;

    const source = audioElement.querySelector("source");
    if (source) source.src = s;

    audioElement.load();

    ns.setAudioVolume(audioElement, volume01);

    if (!autoplay) return;

    await waitForCanPlay(audioElement, 4000);
    if (!isCurrentToken(audioElement, token)) return;

    try {
      await audioElement.play();
    } catch (e) {
      if (!isCurrentToken(audioElement, token)) return;
      if (e && e.name === "AbortError") return;

      console.warn("playlistPlayerInterop.setAudioSourceAndMaybePlay play() failed:", e);

      try {
        const ref = audioElement.__pp_dotNetRef;
        if (ref) {
          await ref.invokeMethodAsync(
            "OnAudioPlayFailed",
            (e && e.name) ? e.name : "play_failed",
            (e && e.message) ? e.message : ""
          );
        }
      } catch { }
    }
  };

  ns.playAudio = async function (audioElement) {
    if (!isMediaElement(audioElement)) return;

    const token = nextPlayToken(audioElement);

    try {
      await audioElement.play();
    } catch (e) {
      if (!isCurrentToken(audioElement, token)) return;
      if (e && e.name === "AbortError") return;

      console.warn("playlistPlayerInterop.playAudio failed:", e);

      try {
        const ref = audioElement.__pp_dotNetRef;
        if (ref) {
          await ref.invokeMethodAsync(
            "OnAudioPlayFailed",
            (e && e.name) ? e.name : "play_failed",
            (e && e.message) ? e.message : ""
          );
        }
      } catch { }
    }
  };

  ns.pauseAudio = function (audioElement) {
    if (!isMediaElement(audioElement)) return;
    audioElement.pause();
  };

  ns.stopAudio = function (audioElement) {
    if (!isMediaElement(audioElement)) return;

    // Cancel any pending play attempt from the previous source
    nextPlayToken(audioElement);

    // Prevent this internal pause from firing OnMediaStateChanged(false) late
    suppressPauseOnce(audioElement, () => {
      audioElement.pause();
      audioElement.currentTime = 0;
    });
  };

  ns.seekAudio = function (audioElement, seconds) {
    if (!isMediaElement(audioElement)) return;
    audioElement.currentTime = seconds || 0;
  };

  ns.setAudioVolume = function (audioElement, volume01) {
    if (!isMediaElement(audioElement)) return;
    const v = Math.max(0, Math.min(1, volume01 ?? 0.5));
    audioElement.volume = v;
  };

  ns.disposeAudio = function (audioElement) {
    if (!isMediaElement(audioElement) || !audioElement.__pp_handlers) return;

    const h = audioElement.__pp_handlers;
    audioElement.removeEventListener("ended", h.onEnded);
    audioElement.removeEventListener("timeupdate", h.onTimeUpdate);
    audioElement.removeEventListener("loadedmetadata", h.onLoadedMeta);
    audioElement.removeEventListener("error", h.onError);
    audioElement.removeEventListener("play", h.onPlay);
    audioElement.removeEventListener("pause", h.onPause);

    delete audioElement.__pp_handlers;
    delete audioElement.__pp_dotNetRef;
    delete audioElement.__pp_lastTimeUpdate;
    delete audioElement.__pp_playToken;
    delete audioElement.__pp_playingToken;
    delete audioElement.__pp_suppressPause;
  };
})();