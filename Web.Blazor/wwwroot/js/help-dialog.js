// Behaviour of the help manual dialog (docs/Help.md rendered by
// Components/Help/HelpDialog.razor).
//
// 1) Anchor navigation inside the scrollable .help-content.
//    Markdig keeps the heading ids as raw UTF-8 text
//    ("0-quick-start--começar-em-5-minutos"), but HtmlSanitizer percent-encodes the
//    TOC fragments ("#0-quick-start--come%C3%A7ar-em-5-minutos"). Looking the raw
//    fragment up with getElementById never matches, so the fragment is decoded first.
//    The listener is registered in the capture phase so it runs before Blazor's
//    router (which would otherwise swallow the fragment-only link) and the click is
//    handled on the .help-content container, not on the page itself.
//
// 2) The "back to top" button (.help-scroll-top) rendered next to that container.
//    It is shown once the manual is scrolled down and scrolls the container back
//    to the start. The button is toggled with a class, so no Blazor state/interop
//    round trip is needed.
//
// This lives in a file loaded by App.razor because <script> tags rendered from a
// Razor component are not executed by Blazor.
(function () {
    if (window.helpDialogRegistered) {
        return;
    }
    window.helpDialogRegistered = true;

    var scrollTopThreshold = 240;

    function findTarget(container, fragment) {
        var id = fragment;
        try {
            id = decodeURIComponent(fragment);
        } catch (e) {
            // Malformed escape sequence: fall back to the raw fragment.
        }

        var target = document.getElementById(id);
        if (target && container.contains(target)) {
            return target;
        }

        // Defensive fallback: compare against the ids actually rendered.
        var candidates = container.querySelectorAll('[id]');
        for (var i = 0; i < candidates.length; i++) {
            var candidateId = candidates[i].id;
            if (candidateId === id || candidateId === fragment) {
                return candidates[i];
            }
        }

        return null;
    }

    function syncScrollTopButton(container) {
        var wrapper = container.parentElement;
        if (!wrapper) {
            return;
        }

        var button = wrapper.querySelector('.help-scroll-top');
        if (!button) {
            return;
        }

        button.classList.toggle('is-visible', container.scrollTop > scrollTopThreshold);
    }

    document.addEventListener('click', function (e) {
        if (!e.target || !e.target.closest) {
            return;
        }

        // "Back to top": scrolls the dialog body, not the page behind it.
        var topButton = e.target.closest('.help-scroll-top');
        if (topButton) {
            e.preventDefault();
            e.stopPropagation();

            var wrapper = topButton.parentElement;
            var scroller = wrapper ? wrapper.querySelector('.help-content') : null;
            if (scroller) {
                scroller.scrollTo({ top: 0, behavior: 'smooth' });
            }
            return;
        }

        var anchor = e.target.closest('a[href^="#"]');
        if (!anchor) {
            return;
        }

        var container = anchor.closest('.help-content');
        if (!container) {
            return;
        }

        var href = anchor.getAttribute('href') || '';
        if (href.length < 2) {
            return;
        }

        var target = findTarget(container, href.slice(1));
        if (!target) {
            return;
        }

        e.preventDefault();
        e.stopPropagation();

        // Scroll the container itself (the dialog body), leaving the page untouched.
        var offset = target.getBoundingClientRect().top - container.getBoundingClientRect().top;
        container.scrollTo({ top: container.scrollTop + offset - 12, behavior: 'smooth' });
    }, true);

    // 'scroll' does not bubble, but it is observable in the capture phase. Listening
    // on the document keeps this working when Blazor/SfDialog recreate the DOM.
    document.addEventListener('scroll', function (e) {
        var container = e.target;
        if (!container || !container.classList || !container.classList.contains('help-content')) {
            return;
        }

        syncScrollTopButton(container);
    }, true);
})();
