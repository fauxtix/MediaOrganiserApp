(function () {
    'use strict';

    var STORAGE_KEY = 'theme';
    var LIGHT_THEME = 'bootstrap5.css';
    var DARK_THEME = 'bootstrap5-dark.css';
    var LIGHT_LINK_ID = 'sfThemeLight';
    var DARK_LINK_ID = 'sfThemeDark';

    function currentTheme() {
        var stored = null;
        try {
            stored = localStorage.getItem(STORAGE_KEY);
        } catch (e) { }
        return stored === 'dark' ? 'dark' : 'light';
    }

    function setLinkDisabled(id, disabled) {
        var link = document.getElementById(id);
        if (link) {
            link.disabled = disabled;
        }
    }

    window.applyTheme = function (theme) {
        var isDark = theme === 'dark';
        try {
            localStorage.setItem(STORAGE_KEY, isDark ? 'dark' : 'light');
        } catch (e) { }
        setLinkDisabled(LIGHT_LINK_ID, isDark);
        setLinkDisabled(DARK_LINK_ID, !isDark);
        document.documentElement.setAttribute('data-theme', isDark ? 'dark' : 'light');
        document.body.classList.toggle('dark-theme', isDark);
    };

    window.getCurrentTheme = function () {
        return currentTheme();
    };

    window.toggleTheme = function () {
        var next = currentTheme() === 'dark' ? 'light' : 'dark';
        window.applyTheme(next);
        return next;
    };

    var initial = currentTheme();
    setLinkDisabled(LIGHT_LINK_ID, initial === 'dark');
    setLinkDisabled(DARK_LINK_ID, initial !== 'dark');
    document.documentElement.setAttribute('data-theme', initial);
})();