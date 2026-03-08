// Admin Common JavaScript - Shared functionality across admin pages

document.addEventListener('DOMContentLoaded', function() {
    // Initialize Lucide icons function
    function initIcons() {
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }
    
    // Initialize icons on page load
    initIcons();
    
    // Reinitialize icons after a short delay to ensure DOM is fully ready
    setTimeout(initIcons, 100);

    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.remove();
            }, 300);
        }, 5000);
    });

    // Handle alert close buttons
    const closeButtons = document.querySelectorAll('.alert .close');
    closeButtons.forEach(button => {
        button.addEventListener('click', function() {
            const alert = this.closest('.alert');
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.remove();
            }, 300);
        });
    });

    // Form validation enhancement
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const requiredFields = form.querySelectorAll('[required]');
            let isValid = true;

            requiredFields.forEach(field => {
                if (!field.value.trim()) {
                    isValid = false;
                    field.style.borderColor = 'var(--danger)';
                } else {
                    field.style.borderColor = 'var(--stroke)';
                }
            });

            if (!isValid) {
                e.preventDefault();
                alert('Please fill in all required fields.');
            }
        });
    });

    // Phone number formatting
    const phoneInputs = document.querySelectorAll('input[type="text"][name*="Phone"], input[type="tel"]');
    phoneInputs.forEach(input => {
        input.addEventListener('input', function(e) {
            // Remove non-numeric characters
            this.value = this.value.replace(/\D/g, '');
        });
    });

    // Username validation
    const usernameInputs = document.querySelectorAll('input[name*="Username"]');
    usernameInputs.forEach(input => {
        input.addEventListener('input', function(e) {
            // Only allow alphanumeric and underscore
            this.value = this.value.replace(/[^a-zA-Z0-9_]/g, '');
        });
    });

    // Email validation
    const emailInputs = document.querySelectorAll('input[type="email"]');
    emailInputs.forEach(input => {
        input.addEventListener('blur', function(e) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (this.value && !emailRegex.test(this.value)) {
                this.style.borderColor = 'var(--danger)';
            } else {
                this.style.borderColor = 'var(--stroke)';
            }
        });
    });

    // Confirm delete actions
    const deleteButtons = document.querySelectorAll('form[action*="Delete"] button[type="submit"], .btn-danger[onclick*="delete"], .btn-danger[onclick*="Delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
                e.preventDefault();
            }
        });
    });

    // Table row hover effect
    const tableRows = document.querySelectorAll('table tbody tr, .table tbody tr');
    tableRows.forEach(row => {
        row.addEventListener('mouseenter', function() {
            this.style.backgroundColor = '#0F1114';
        });
        row.addEventListener('mouseleave', function() {
            this.style.backgroundColor = 'transparent';
        });
    });

    // Tooltip positioning with fixed position to avoid clipping
    function setupTooltips() {
        const iconButtons = document.querySelectorAll('.btn-icon');
        
        iconButtons.forEach(button => {
            const tooltip = button.querySelector('.tooltip');
            if (!tooltip) return;
            
            let showTimeout;
            let hideTimeout;
            
            function showTooltip() {
                clearTimeout(hideTimeout);
                
                showTimeout = setTimeout(() => {
                    const buttonRect = button.getBoundingClientRect();
                    const tooltipText = tooltip.textContent.trim();
                    
                    // Temporarily show tooltip to measure it
                    tooltip.style.visibility = 'visible';
                    tooltip.style.opacity = '0';
                    tooltip.style.left = '0';
                    tooltip.style.top = '0';
                    
                    const tooltipWidth = tooltip.offsetWidth;
                    const tooltipHeight = tooltip.offsetHeight;
                    
                    // Calculate center position of button
                    const buttonCenterX = buttonRect.left + (buttonRect.width / 2);
                    const buttonTop = buttonRect.top;
                    
                    // Position tooltip above button
                    let tooltipLeft = buttonCenterX;
                    let tooltipTop = buttonTop - tooltipHeight - 8;
                    
                    // Check if tooltip goes off left edge
                    if (tooltipLeft - (tooltipWidth / 2) < 10) {
                        tooltipLeft = tooltipWidth / 2 + 10;
                    }
                    
                    // Check if tooltip goes off right edge
                    if (tooltipLeft + (tooltipWidth / 2) > window.innerWidth - 10) {
                        tooltipLeft = window.innerWidth - (tooltipWidth / 2) - 10;
                    }
                    
                    // Check if tooltip goes off top edge - show below instead
                    if (tooltipTop < 10) {
                        tooltipTop = buttonRect.bottom + 8;
                        tooltip.classList.add('tooltip-below');
                    } else {
                        tooltip.classList.remove('tooltip-below');
                    }
                    
                    // Set final position
                    tooltip.style.left = tooltipLeft + 'px';
                    tooltip.style.top = tooltipTop + 'px';
                    tooltip.style.transform = 'translateX(-50%)';
                    tooltip.classList.add('show');
                }, 100);
            }
            
            function hideTooltip() {
                clearTimeout(showTimeout);
                hideTimeout = setTimeout(() => {
                    tooltip.classList.remove('show');
                    tooltip.classList.remove('tooltip-below');
                }, 50);
            }
            
            button.addEventListener('mouseenter', showTooltip);
            button.addEventListener('mouseleave', hideTooltip);
            button.addEventListener('focus', showTooltip);
            button.addEventListener('blur', hideTooltip);
        });
    }
    
    // Setup tooltips after icons are initialized
    setTimeout(setupTooltips, 200);
});

