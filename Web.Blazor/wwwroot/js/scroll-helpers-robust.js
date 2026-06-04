// Scroll accordion item into view and highlight it
window.scrollElementIntoViewWhenVisible = function (elementId) {

    if (!elementId)
        return;

    const el = document.getElementById(elementId);

    if (!el)
        return;

    const scroll = () => {

        requestAnimationFrame(() => {

            // Highlight
            el.classList.add('highlight-scroll');

            // Remove highlight later
            setTimeout(() => {
                el.classList.remove('highlight-scroll');
            }, 1500);

            // Smooth scroll
            el.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        });
    };

    // Already rendered
    if (el.offsetParent !== null) {
        scroll();
        return;
    }

    // Wait for Syncfusion accordion animation/render
    const observer = new MutationObserver(() => {

        if (el.offsetParent !== null) {

            observer.disconnect();

            // small delay after accordion expansion
            setTimeout(scroll, 150);
        }
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true
    });

    // Fallback
    setTimeout(() => {
        observer.disconnect();
        scroll();
    }, 2000);
};