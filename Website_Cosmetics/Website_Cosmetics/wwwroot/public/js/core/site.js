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
        
        // Initialize user menu dropdown
        initUserMenu();
    });
    
    // User Menu Dropdown Handler
    function initUserMenu() {
        const trigger = document.querySelector('.user-menu__trigger');
        const dropdown = document.querySelector('.user-menu__dropdown');
        
        if (!trigger || !dropdown) return;
        
        // Toggle dropdown on trigger click
        trigger.addEventListener('click', (e) => {
            e.stopPropagation();
            const isExpanded = trigger.getAttribute('aria-expanded') === 'true';
            trigger.setAttribute('aria-expanded', !isExpanded);
            
            // Reinitialize Lucide icons when dropdown opens
            if (!isExpanded && typeof lucide !== 'undefined') {
                setTimeout(() => {
                    lucide.createIcons();
                }, 10);
            }
        });
        
        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!trigger.contains(e.target) && !dropdown.contains(e.target)) {
                trigger.setAttribute('aria-expanded', 'false');
            }
        });
        
        // Close dropdown on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && trigger.getAttribute('aria-expanded') === 'true') {
                trigger.setAttribute('aria-expanded', 'false');
            }
        });
    }
})();
