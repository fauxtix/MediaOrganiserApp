window.getTheme = () => localStorage.getItem('theme') || 'light';

window.setTheme = (theme) => {
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
};

// Initialize theme on page load
const savedTheme = window.getTheme();
document.documentElement.setAttribute('data-theme', savedTheme);