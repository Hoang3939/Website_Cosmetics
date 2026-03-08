// Staff Management JavaScript - Page-specific functionality

document.addEventListener('DOMContentLoaded', function() {
    // Check if filters are applied and show/hide reset button
    function checkFilterState() {
        const searchInput = document.getElementById('search');
        const statusSelect = document.getElementById('status');
        const resetBtn = document.getElementById('resetBtn');
        
        if (searchInput && statusSelect && resetBtn) {
            const hasSearch = searchInput.value.trim() !== '';
            const hasStatus = statusSelect.value !== '';
            const hasFilters = hasSearch || hasStatus;
            
            if (hasFilters) {
                resetBtn.style.display = 'inline-flex';
            } else {
                resetBtn.style.display = 'none';
            }
            
            // Reinitialize icons after showing/hiding button
            if (typeof lucide !== 'undefined') {
                lucide.createIcons();
            }
        }
    }

    // Check filter state on page load
    checkFilterState();

    // Monitor filter inputs for changes
    const searchInput = document.getElementById('search');
    const statusSelect = document.getElementById('status');
    
    if (searchInput) {
        searchInput.addEventListener('input', checkFilterState);
        searchInput.addEventListener('change', checkFilterState);
    }
    
    if (statusSelect) {
        statusSelect.addEventListener('change', checkFilterState);
    }

    // Search input debounce (if needed for future AJAX search)
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener('input', function(e) {
            clearTimeout(searchTimeout);
            // Could implement AJAX search here in the future
        });
    }
});

