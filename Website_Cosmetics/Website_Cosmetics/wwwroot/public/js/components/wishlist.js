// Wishlist component - Handles adding/removing products from wishlist
(function() {
    'use strict';

    const Wishlist = {
        init: function() {
            this.bindEvents();
            this.checkInitialState();
        },

        bindEvents: function() {
            // Handle wishlist button clicks
            document.addEventListener('click', (e) => {
                const wishlistBtn = e.target.closest('.product__wishlist-btn, .product-details__wishlist-btn, .btn--wishlist');
                if (wishlistBtn) {
                    e.preventDefault();
                    e.stopPropagation();
                    const productId = wishlistBtn.getAttribute('data-product-id');
                    if (productId) {
                        this.toggleWishlist(parseInt(productId), wishlistBtn);
                    }
                }
            });
        },

        checkInitialState: function() {
            // Check wishlist state for all products on page load
            const wishlistButtons = document.querySelectorAll('[data-product-id]');
            if (!wishlistButtons.length) return;

            const productIds = Array.from(wishlistButtons).map(btn => 
                parseInt(btn.getAttribute('data-product-id'))
            ).filter(id => !isNaN(id));

            if (!productIds.length) return;

            // Check all products at once
            this.checkWishlistStatus(productIds);
        },

        checkWishlistStatus: async function(productIds) {
            try {
                for (const productId of productIds) {
                    const response = await fetch(`/Wishlist/Check?productId=${productId}`);
                    const data = await response.json();
                    
                    const buttons = document.querySelectorAll(`[data-product-id="${productId}"]`);
                    buttons.forEach(btn => {
                        if (data.isLiked) {
                            btn.classList.add('product__wishlist-btn--active', 'wishlist-active');
                            const icon = btn.querySelector('.wishlist-icon');
                            if (icon) {
                                icon.setAttribute('data-lucide', 'heart');
                                icon.setAttribute('fill', 'currentColor');
                            }
                        } else {
                            btn.classList.remove('product__wishlist-btn--active', 'wishlist-active');
                            const icon = btn.querySelector('.wishlist-icon');
                            if (icon) {
                                icon.setAttribute('data-lucide', 'heart');
                                icon.removeAttribute('fill');
                            }
                        }
                    });
                }

                // Reinitialize Lucide icons
                if (typeof lucide !== 'undefined') {
                    lucide.createIcons();
                }
            } catch (error) {
                console.error('Error checking wishlist status:', error);
            }
        },

        toggleWishlist: async function(productId, button) {
            // Check if user is authenticated
            const isAuthenticated = button.getAttribute('data-authenticated') === 'true' || 
                                   document.querySelector('.user-menu__trigger') !== null;

            if (!isAuthenticated) {
                window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                return;
            }

            const isCurrentlyLiked = button.classList.contains('product__wishlist-btn--active') || 
                                     button.classList.contains('wishlist-active');

            // Show confirmation if removing from wishlist
            if (isCurrentlyLiked) {
                const confirmed = confirm('Are you sure you want to remove this item from your wishlist?');
                if (!confirmed) {
                    return;
                }
            }

            // Disable button during request
            button.disabled = true;
            const originalContent = button.innerHTML;

            try {
                // Get anti-forgery token from form or meta tag
                let token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                if (!token) {
                    token = document.querySelector('meta[name="__RequestVerificationToken"]')?.content;
                }
                
                const formData = new FormData();
                formData.append('productId', productId);
                if (token) {
                    formData.append('__RequestVerificationToken', token);
                }

                const response = await fetch('/Wishlist/Toggle', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (data.success) {
                    // Update button state
                    if (data.isLiked) {
                        button.classList.add('product__wishlist-btn--active', 'wishlist-active');
                        const icon = button.querySelector('.wishlist-icon');
                        if (icon) {
                            icon.setAttribute('data-lucide', 'heart');
                            icon.setAttribute('fill', 'currentColor');
                        }
                        
                        // Show success message
                        this.showToast('Added to wishlist', 'success');
                    } else {
                        button.classList.remove('product__wishlist-btn--active', 'wishlist-active');
                        const icon = button.querySelector('.wishlist-icon');
                        if (icon) {
                            icon.setAttribute('data-lucide', 'heart');
                            icon.removeAttribute('fill');
                        }
                        
                        // Show success message
                        this.showToast('Removed from wishlist', 'success');

                        // Handle page-specific behavior after removal
                        const currentPath = window.location.pathname.toLowerCase();
                        const isDetailsPage = currentPath.includes('/products/details');
                        const isWishlistPage = currentPath.includes('/wishlist');
                        
                        // If on wishlist page, remove the product card with animation
                        if (isWishlistPage) {
                            const productCard = button.closest('.product');
                            if (productCard) {
                                productCard.style.transition = 'opacity 0.3s ease';
                                productCard.style.opacity = '0';
                                setTimeout(() => {
                                    productCard.remove();
                                    
                                    // Check if wishlist is empty
                                    const remainingProducts = document.querySelectorAll('.product');
                                    if (remainingProducts.length === 0) {
                                        location.reload();
                                    }
                                }, 300);
                            }
                        }
                        // If on product listing pages (not Details page), reload to update all wishlist buttons
                        else if (!isDetailsPage) {
                            // Reload page after a short delay to show the toast message
                            setTimeout(() => {
                                location.reload();
                            }, 500);
                        }
                        // On Details page, just update the current button (no reload needed)
                    }

                    // Reinitialize Lucide icons
                    if (typeof lucide !== 'undefined') {
                        lucide.createIcons();
                    }
                } else {
                    if (data.requiresLogin) {
                        window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    } else {
                        this.showToast(data.message || 'An error occurred', 'error');
                    }
                }
            } catch (error) {
                console.error('Error toggling wishlist:', error);
                this.showToast('An error occurred. Please try again.', 'error');
            } finally {
                button.disabled = false;
            }
        },

        showToast: function(message, type) {
            // Use existing toast system if available
            if (typeof Toast !== 'undefined' && typeof Toast.show === 'function') {
                Toast.show(message, type);
            } else {
                // Fallback: simple alert
                alert(message);
            }
        }
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => Wishlist.init());
    } else {
        Wishlist.init();
    }

    // Export for global access
    window.Wishlist = Wishlist;
})();
