window.getTheme = () => localStorage.getItem('theme') || 'light';
window.setTheme = (theme) => { localStorage.setItem('theme', theme); document.documentElement.setAttribute('data-theme', theme); };

window.scrollToBottom = function (el) {
    try {
        if (!el) return;
        const target = el instanceof HTMLElement ? el : el;
        if (!target) return;
        target.scrollTop = target.scrollHeight;
    } catch (e) { }
};

window.scrollMessagesToBottom = function (el) {
    try {
        if (!el) return;
        el.scrollTop = el.scrollHeight;
    } catch { }
};
window.isNearBottom = function (el, threshold) {
    try {
        if (!el) return true;
        const target = el instanceof HTMLElement ? el : el;
        if (!target) return true;

        threshold = Number(threshold) || 80;
        const distance = target.scrollHeight - (target.scrollTop + target.clientHeight);
        return distance <= threshold;
    } catch (e) { return true; }
};

window.hasScrollbar = function (el) {
    try {
        if (!el) return false;
        const target = el instanceof HTMLElement ? el : el;
        if (!target) return false;

        return target.scrollHeight > target.clientHeight;
    } catch (e) { return false; }
};

window.updateAppToolbarHeight = function () {
    try {
        const candidates = Array.from(document.querySelectorAll('body *'));
        let maxBottom = 0;

        candidates.forEach(el => {
            try {
                const style = window.getComputedStyle(el);
                if (!style) return;

                if (style.position === 'fixed' || style.position === 'sticky') {
                    const rect = el.getBoundingClientRect();
                    if (rect.top <= 4 && rect.bottom > maxBottom && rect.height > 0)
                        maxBottom = rect.bottom;
                }
            } catch (e) { }
        });

        if (maxBottom === 0) {
            const toolbar = document.querySelector('.top-row') || document.querySelector('.navbar');
            if (toolbar) maxBottom = toolbar.getBoundingClientRect().bottom || 0;
        }

        document.documentElement.style.setProperty('--app-toolbar-height', (maxBottom || 0) + 'px');
    } catch (e) { }
};

window.getToolbarHeight = function () {
    const toolbar = document.querySelector('.top-row');
    return toolbar ? toolbar.offsetHeight : 0;
};

window.addEventListener('load', () => window.updateAppToolbarHeight());
window.addEventListener('resize', () => window.updateAppToolbarHeight());
setTimeout(() => window.updateAppToolbarHeight(), 200);
setTimeout(() => window.updateAppToolbarHeight(), 800);

(function () {
    let adjustTimeout = null;

    function measureToolbarBottom() {
        try {
            const candidates = Array.from(document.querySelectorAll('body *'));
            let maxBottom = 0;

            candidates.forEach(el => {
                try {
                    const style = window.getComputedStyle(el);
                    if (!style) return;

                    if (style.position === 'fixed' || style.position === 'sticky') {
                        const rect = el.getBoundingClientRect();
                        if (rect.top <= 4 && rect.bottom > maxBottom && rect.height > 0)
                            maxBottom = rect.bottom;
                    }
                } catch (e) { }
            });

            if (maxBottom === 0) {
                const toolbar = document.querySelector('.top-row') || document.querySelector('.navbar');
                if (toolbar) maxBottom = toolbar.getBoundingClientRect().bottom || 0;
            }

            return maxBottom || 0;
        } catch (e) {
            return 0;
        }
    }

    function adjustChatHeaderPosition() {
        try {
            const header = document.querySelector('.chat-header');
            const messages = document.querySelector('.messages');
            if (!header) return;

            const toolbarBottom = measureToolbarBottom();

            header.style.top = toolbarBottom + 'px';
            header.style.left = '0';
            header.style.right = '0';

            try {
                const hdrRect = header.getBoundingClientRect();
                const pad = (toolbarBottom + hdrRect.height + 8) || (hdrRect.height + 8);
                if (messages) messages.style.paddingTop = pad + 'px';
            } catch (e) { }

        } catch (e) { }
    }

    function scheduleAdjust() {
        if (adjustTimeout) clearTimeout(adjustTimeout);
        adjustTimeout = setTimeout(() => {
            window.updateAppToolbarHeight();
            adjustChatHeaderPosition();
        }, 80);
    }

    window.addEventListener('load', scheduleAdjust);
    window.addEventListener('resize', scheduleAdjust);

    try {
        const mo = new MutationObserver(() => scheduleAdjust());
        mo.observe(document.body, { childList: true, subtree: true });
    } catch (e) { }

    scheduleAdjust();
    setTimeout(scheduleAdjust, 250);
    setTimeout(scheduleAdjust, 1000);
})();

document.documentElement.setAttribute('data-theme', window.getTheme());

window.chatInput = (function () {
    let isResetting = false;

    function insertNewlineAtCaret(el) {
        if (!el) return;
        const start = el.selectionStart;
        const end = el.selectionEnd;
        const value = el.value;
        el.value = value.slice(0, start) + '\n' + value.slice(end);
        el.selectionStart = el.selectionEnd = start + 1;
        autoResize(el);
    }

    function autoResize(el) {
        if (!el) return;

        if (isResetting || !el.value || el.value.trim() === '') {
            el.style.height = '';
            el.style.removeProperty('height');
            return;
        }

        el.style.height = 'auto';
        el.style.height = (el.scrollHeight + 2) + 'px';
    }

    function resetHeight(el) {
        const target = el instanceof HTMLElement ? el : document.querySelector('.chat-input');
        if (!target) return;

        isResetting = true;

        //target.value = '';
        target.style.height = '';
        target.style.removeProperty('height');

        let attempts = 0;
        const forceClear = () => {
            if (target) {
                //target.value = '';
                target.style.height = '';
                target.style.removeProperty('height');
            }
            attempts++;
            if (attempts < 3) {
                requestAnimationFrame(forceClear);
            } else {
                isResetting = false;
            }
        };

        requestAnimationFrame(forceClear);
    }

    function attachHandlers(el, dotNetRef) {
        const target = el instanceof HTMLElement ? el : document.querySelector('.chat-input');
        if (!target) return;

        if (target.__chatInputHandler) {
            target.__chatInputDotNetRef = dotNetRef;
            return;
        }

        const handler = function (e) {
            if (e.key === 'Enter') {

                // SHIFT+ENTER → nova linha
                if (e.shiftKey) {
                    e.preventDefault();
                    insertNewlineAtCaret(target);
                    return;
                }

                // ENTER normal → enviar
                e.preventDefault();

                target.dispatchEvent(new Event('change', { bubbles: true }));
                target.dispatchEvent(new Event('input', { bubbles: true }));

                const ref = target.__chatInputDotNetRef || dotNetRef;
                if (ref) ref.invokeMethodAsync('OnEnterPressedFromJs');
            }
        };
        const resizeHandler = function () { autoResize(target); };

        target.__chatInputHandler = handler;
        target.__chatInputResizeHandler = resizeHandler;
        target.__chatInputDotNetRef = dotNetRef;

        target.addEventListener('keydown', handler);
        target.addEventListener('input', resizeHandler);

        autoResize(target);
    }

    function detachHandlers(el) {
        const target = el instanceof HTMLElement ? el : document.querySelector('.chat-input');
        if (!target) return;

        if (target.__chatInputHandler) {
            target.removeEventListener('keydown', target.__chatInputHandler);
            delete target.__chatInputHandler;
        }
        if (target.__chatInputResizeHandler) {
            target.removeEventListener('input', target.__chatInputResizeHandler);
            delete target.__chatInputResizeHandler;
        }
    }

    return { attachHandlers, detachHandlers, insertNewlineAtCaret, resetHeight };
})();

window.copilotInput = (function () {
    function attachHandlers(el) {
        if (!el) return;
        if (el.__copilotAttached) return;

        const wrapper = el.closest && el.closest('.copilot-input-wrapper');
        const overlay = wrapper ? wrapper.querySelector('.copilot-suggestion') : null;

        function syncOverlay() {
            try {
                if (!overlay || !el) return;
                const cs = getComputedStyle(el);

                overlay.style.fontFamily = cs.fontFamily;
                overlay.style.fontSize = cs.fontSize;
                overlay.style.lineHeight = cs.lineHeight;
                overlay.style.padding = cs.padding;

                overlay.style.width = el.clientWidth + 'px';
                overlay.scrollTop = el.scrollTop;
            } catch (e) { }
        }

        el.addEventListener('input', syncOverlay);
        el.addEventListener('scroll', syncOverlay);

        el.__copilotAttached = true;
    }

    function detachHandlers(el) {
        if (!el) return;
        delete el.__copilotAttached;
    }

    return { attachHandlers, detachHandlers };
})();

window.focusElement = function (el) {
    try { if (el) el.focus(); } catch (e) { }
};

window.showToast = function (text) {
    try {
        const el = document.createElement('div');
        el.textContent = text;
        el.style.position = 'fixed';
        el.style.bottom = '110px';
        el.style.right = '20px';
        el.style.background = 'rgba(0,0,0,0.75)';
        el.style.color = '#fff';
        el.style.padding = '8px 12px';
        el.style.borderRadius = '6px';
        el.style.zIndex = 9999;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 2600);
    } catch (e) { }
};


window.chatScroll = {

    getScrollState: function (el) {
        try {
            if (!el) return null;

            return {
                scrollTop: el.scrollTop,
                scrollHeight: el.scrollHeight,
                clientHeight: el.clientHeight
            };
        } catch {
            return null;
        }
    },

    isAtBottom: function (el, threshold = 80) {
        try {
            if (!el) return true;

            const distance = el.scrollHeight - el.scrollTop - el.clientHeight;
            return distance <= threshold;
        } catch {
            return true;
        }
    },

    scrollToBottom: function (el) {
        try {
            if (!el) return;

            requestAnimationFrame(() => {
                try {
                    requestAnimationFrame(() => {
                        el.scrollTop = el.scrollHeight;
                    });
                } catch { }
            });

        } catch { }
    },

    scrollDuringStream: function (el) {
        try {
            if (!el) return;

            const distance = el.scrollHeight - (el.scrollTop + el.clientHeight);
            const shouldStick = distance < 120;

            if (!shouldStick) return;

            el.scrollTop = el.scrollHeight;

        } catch { }
    }
};