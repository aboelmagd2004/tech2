    document.addEventListener('DOMContentLoaded', function () {
    // Theme Toggle
    const themeToggle = document.getElementById('adminThemeToggle');
    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            const html = document.documentElement;
            const isDark = html.getAttribute('data-theme') === 'dark';
            const newTheme = isDark ? 'light' : 'dark';

            // Update theme
            html.setAttribute('data-theme', newTheme);

            // Update toggle button icon and label
            this.innerHTML = `<i class="fas fa-${newTheme === 'dark' ? 'moon' : 'sun'}"></i> ${newTheme === 'dark' ? 'Dark Mode' : 'Light Mode'}`;

            // Save theme to cookie and localStorage
            document.cookie = `theme=${newTheme}; path=/; max-age=31536000; SameSite=Lax`;
            localStorage.setItem('adminTheme', newTheme);
        });
    }

    // Mobile Sidebar Toggle
    const sidebarToggleMobile = document.getElementById('sidebarToggleMobile');
    if (sidebarToggleMobile) {
        sidebarToggleMobile.addEventListener('click', function () {
            const sidebar = document.querySelector('.admin-sidebar');
            if (sidebar) {
                sidebar.classList.toggle('show');
            }
        });
    }

    // Initialize Bootstrap tooltips
    const tooltipTriggerList = Array.from(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.forEach(function (el) {
        new bootstrap.Tooltip(el);
    });
});
