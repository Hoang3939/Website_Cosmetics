// Price Filter Component
document.addEventListener('DOMContentLoaded', function() {
    const priceButtons = document.querySelectorAll('.price-btn');
    const filterForm = document.getElementById('filterForm');
    
    if (!priceButtons || priceButtons.length === 0) {
        return;
    }
    
    // Initialize from URL parameters
    const urlParams = new URLSearchParams(window.location.search);
    const currentMinPrice = urlParams.get('minPrice');
    const currentMaxPrice = urlParams.get('maxPrice');
    
    // Create hidden inputs for price filters
    let minPriceInput = document.getElementById('hiddenMinPrice');
    let maxPriceInput = document.getElementById('hiddenMaxPrice');
    
    if (!minPriceInput) {
        minPriceInput = document.createElement('input');
        minPriceInput.type = 'hidden';
        minPriceInput.id = 'hiddenMinPrice';
        minPriceInput.name = 'minPrice';
        filterForm.appendChild(minPriceInput);
    }
    
    if (!maxPriceInput) {
        maxPriceInput = document.createElement('input');
        maxPriceInput.type = 'hidden';
        maxPriceInput.id = 'hiddenMaxPrice';
        maxPriceInput.name = 'maxPrice';
        filterForm.appendChild(maxPriceInput);
    }
    
    // Initialize active button based on URL parameters
    if (currentMinPrice || currentMaxPrice) {
        updateActivePriceButtonFromURL(currentMinPrice, currentMaxPrice);
    } else {
        // Set "All" button as active by default
        priceButtons.forEach(btn => {
            if (btn.getAttribute('data-min') === '' && btn.getAttribute('data-max') === '') {
                btn.classList.add('active');
            }
        });
    }
    
    // Price button click handlers
    priceButtons.forEach(button => {
        button.addEventListener('click', function() {
            const minPrice = this.getAttribute('data-min');
            const maxPrice = this.getAttribute('data-max');
            
            // Remove active class from all buttons
            priceButtons.forEach(btn => btn.classList.remove('active'));
            
            // Add active class to clicked button
            this.classList.add('active');
            
            // Update hidden inputs
            if (minPrice === '') {
                // "All" button - remove price filters
                minPriceInput.remove();
                maxPriceInput.remove();
                
                // Recreate if needed
                minPriceInput = document.createElement('input');
                minPriceInput.type = 'hidden';
                minPriceInput.id = 'hiddenMinPrice';
                filterForm.appendChild(minPriceInput);
                
                maxPriceInput = document.createElement('input');
                maxPriceInput.type = 'hidden';
                maxPriceInput.id = 'hiddenMaxPrice';
                filterForm.appendChild(maxPriceInput);
            } else {
                // Set price values
                minPriceInput.name = 'minPrice';
                minPriceInput.value = minPrice;
                
                if (maxPrice) {
                    maxPriceInput.name = 'maxPrice';
                    maxPriceInput.value = maxPrice;
                } else {
                    maxPriceInput.removeAttribute('name');
                    maxPriceInput.value = '';
                }
            }
        });
    });
    
    // Update active button based on URL parameters
    function updateActivePriceButtonFromURL(minPrice, maxPrice) {
        // Remove active class from all buttons
        priceButtons.forEach(btn => btn.classList.remove('active'));
        
        // Check which button matches current values
        priceButtons.forEach(button => {
            const btnMin = button.getAttribute('data-min');
            const btnMax = button.getAttribute('data-max');
            
            if (btnMin === '' && btnMax === '') {
                // "All" button - only active if no price filters
                if (!minPrice && !maxPrice) {
                    button.classList.add('active');
                }
            } else {
                const btnMinNum = btnMin ? parseInt(btnMin) : null;
                const btnMaxNum = btnMax ? parseInt(btnMax) : null;
                const urlMinNum = minPrice ? parseInt(minPrice) : null;
                const urlMaxNum = maxPrice ? parseInt(maxPrice) : null;
                
                if (urlMinNum === btnMinNum && urlMaxNum === btnMaxNum) {
                    button.classList.add('active');
                }
            }
        });
    }
});
