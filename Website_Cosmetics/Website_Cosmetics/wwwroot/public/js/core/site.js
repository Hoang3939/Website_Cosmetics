// Entry: initialize modules used across pages
; (() => {
    document.addEventListener('DOMContentLoaded', () => {
        Modal.init();
        Toast.init();
        QuickView.init();
    });
})();
