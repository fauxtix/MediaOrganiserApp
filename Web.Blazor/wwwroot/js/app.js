window.getTheme = () => localStorage.getItem('theme') || 'light';
window.setTheme = (theme) => { localStorage.setItem('theme', theme); document.documentElement.setAttribute('data-theme', theme); };

// Scroll helper
window.scrollToBottom = function (el) { try { if (!el) return; const target = el instanceof HTMLElement ? el : (el && el instanceof Object && el instanceof window.HTMLElement ? el : el); if (!target) return; target.scrollTop = target.scrollHeight; } catch (e) { } };

// Toolbar height helper
window.updateAppToolbarHeight = function () {
    try {
        const candidates = Array.from(document.querySelectorAll('body *'));
        let maxBottom = 0;
        candidates.forEach(el => { try { const style = window.getComputedStyle(el); if (!style) return; if (style.position === 'fixed' || style.position === 'sticky') { const rect = el.getBoundingClientRect(); if (rect.top <= 4 && rect.bottom > maxBottom && rect.height > 0) maxBottom = rect.bottom; } } catch (e) { } });
        if (maxBottom === 0) { const toolbar = document.querySelector('.top-row') || document.querySelector('.navbar'); if (toolbar) maxBottom = toolbar.getBoundingClientRect().bottom || 0; }
        document.documentElement.style.setProperty('--app-toolbar-height', (maxBottom || 0) + 'px');
    } catch (e) { }
};
window.getToolbarHeight = function () { const toolbar = document.querySelector('.top-row'); return toolbar ? toolbar.offsetHeight : 0; };
window.addEventListener('load', () => window.updateAppToolbarHeight());
window.addEventListener('resize', () => window.updateAppToolbarHeight());
setTimeout(() => window.updateAppToolbarHeight(), 200);
setTimeout(() => window.updateAppToolbarHeight(), 800);

// Chat header adjustments
(function () {
    let adjustTimeout = null;
    function measureToolbarBottom() { try { const candidates = Array.from(document.querySelectorAll('body *')); let maxBottom = 0; candidates.forEach(el => { try { const style = window.getComputedStyle(el); if (!style) return; if (style.position === 'fixed' || style.position === 'sticky') { const rect = el.getBoundingClientRect(); if (rect.top <= 4 && rect.bottom > maxBottom && rect.height > 0) maxBottom = rect.bottom; } } catch (e) { } }); if (maxBottom === 0) { const toolbar = document.querySelector('.top-row') || document.querySelector('.navbar'); if (toolbar) maxBottom = toolbar.getBoundingClientRect().bottom || 0; } return maxBottom || 0; } catch (e) { return 0; } }
    function adjustChatHeaderPosition() {
        try {
            const header = document.querySelector('.chat-header'); const messages = document.querySelector('.messages'); if (!header) return;
            const toolbarBottom = measureToolbarBottom(); header.style.top = toolbarBottom + 'px'; header.style.left = '0'; header.style.right = '0';
            try { const hdrRect = header.getBoundingClientRect(); const pad = (toolbarBottom + hdrRect.height + 8) || (hdrRect.height + 8); if (messages) messages.style.paddingTop = pad + 'px'; } catch (e) { }
        } catch (e) { }
    }
    function scheduleAdjust() { if (adjustTimeout) clearTimeout(adjustTimeout); adjustTimeout = setTimeout(() => { window.updateAppToolbarHeight(); adjustChatHeaderPosition(); }, 80); }
    window.addEventListener('load', scheduleAdjust);
    window.addEventListener('resize', scheduleAdjust);
    try { const mo = new MutationObserver(() => scheduleAdjust()); mo.observe(document.body, { childList: true, subtree: true }); } catch (e) { }
    scheduleAdjust(); setTimeout(scheduleAdjust, 250); setTimeout(scheduleAdjust, 1000);
})();

// Initialize theme
document.documentElement.setAttribute('data-theme', window.getTheme());

// Chat input helpers
window.chatInput = (function () {
    function insertNewlineAtCaret(el) { if (!el) return; const start = el.selectionStart; const end = el.selectionEnd; const value = el.value; el.value = value.slice(0, start) + '\n' + value.slice(end); el.selectionStart = el.selectionEnd = start + 1; el.dispatchEvent(new Event('input', { bubbles: true })); autoResize(el); }
    function autoResize(el) { if (!el) return; el.style.height = 'auto'; el.style.height = (el.scrollHeight + 2) + 'px'; }
    function attachHandlers(el, dotNetRef) { if (!el) return; const handler = function (e) { if (e.key === 'Enter') { if (!e.shiftKey) { e.preventDefault(); if (dotNetRef) dotNetRef.invokeMethodAsync('OnEnterPressedFromJs'); } else insertNewlineAtCaret(el); } }; const resizeHandler = function () { autoResize(el); }; el.__chatInputHandler = handler; el.__chatInputResizeHandler = resizeHandler; el.addEventListener('keydown', handler); el.addEventListener('input', resizeHandler); autoResize(el); }
    function detachHandlers(el) { if (!el) return; if (el.__chatInputHandler) { el.removeEventListener('keydown', el.__chatInputHandler); delete el.__chatInputHandler; } if (el.__chatInputResizeHandler) { el.removeEventListener('input', el.__chatInputResizeHandler); delete el.__chatInputResizeHandler; } }
    return { attachHandlers, detachHandlers };
})();

// Focus helper
window.focusElement = function (el) { try { if (!el) return; el.focus(); } catch (e) { } };

// Toast
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
        setTimeout(() => { el.remove(); }, 2600);
    } catch (e) { console.error(e); }
};

// Conversation persistence helpers (store as an array of { Id, Title, Json } )
// Conversation persistence used to be stored in localStorage.
// Local storage CRUD helpers removed - conversations are persisted in server-side SQL now.
// Keep other UI helpers in this file (focus, toast, chat input, scroll, etc.).
