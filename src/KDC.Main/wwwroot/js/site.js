// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto highlight active navigation
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.sidenav__link').forEach(link => {
        if (link.getAttribute('href').toLowerCase() === location.pathname.toLowerCase()) {
            link.parentNode.classList.add('sidenav__item--active');
        } else {
            link.parentNode.classList.remove('sidenav__item--active');
        }
    });
})