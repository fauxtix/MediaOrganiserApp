(function () {
    'use strict';

    if (!window.MutationObserver) return;

    function forceRepaint(el) {
        if (!el) return;
        el.style.willChange = 'transform';
        el.style.transform = 'translateZ(0)';
        requestAnimationFrame(function () {
            el.style.transform = '';
            el.style.willChange = '';
            void el.offsetHeight;
            requestAnimationFrame(function () {
                el.style.transform = 'translateZ(0)';
                requestAnimationFrame(function () {
                    el.style.transform = '';
                    el.style.willChange = '';
                });
            });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        var observer = new MutationObserver(function (mutations) {
            for (var i = 0; i < mutations.length; i++) {
                var m = mutations[i];
                if (!m || m.type !== 'childList' || !m.target) continue;
                var targetId = m.target.id;
                if (!targetId || targetId.indexOf('_tooltip') === -1) continue;

                var removedSvg = false;
                for (var j = 0; j < m.removedNodes.length; j++) {
                    var n = m.removedNodes[j];
                    if (n && n.tagName === 'SVG') { removedSvg = true; break; }
                }
                if (!removedSvg) continue;

                var chart = m.target.closest('.e-accumulationchart');
                if (chart) forceRepaint(chart);
            }
        });
        observer.observe(document.body, { childList: true, subtree: true });
    });
})();