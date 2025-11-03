// Entry: initialize modules used across pages
; (() => {
    document.addEventListener('DOMContentLoaded', () => {
        // Initialize core components
        Modal.init();
        Toast.init();
        QuickView.init();
        
        // Initialize Lucide icons if library is loaded
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    });
})();
