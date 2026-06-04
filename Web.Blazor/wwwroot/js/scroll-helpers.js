// Scroll a DOM element into view (used by Blazor via JS interop)
window.scrollElementIntoView = function (el) {
    try {
        if (!el) return;
        if (el instanceof HTMLElement) {
            el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    } catch (e) { }
};
