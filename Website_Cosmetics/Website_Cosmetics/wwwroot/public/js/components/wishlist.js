// Wishlist component - Handles adding/removing products from wishlist
(function() {
    'use strict';

    const Wishlist = {
        pendingRemove: null, // Store productId and button for pending removal

        init: function() {
            this.bindEvents();
            this.checkInitialState();
            this.initRemoveModal();
        },

        initRemoveModal: function() {
            const confirmBtn = document.getElementById('confirmRemoveBtn');
            if (confirmBtn) {
                confirmBtn.addEventListener('click', () => {
                    if (this.pendingRemove) {
                        const { productId, button } = this.pendingRemove;
                        this.pendingRemove = null;
                        this.closeRemoveModal();
                        this.performRemove(productId, button);
                    }
                });
            }

            // Close modal when clicking outside
            const modal = document.getElementById('wishlistRemoveModal');
            if (modal) {
                modal.addEventListener('click', (e) => {
                    if (e.target === modal) {
                        this.closeRemoveModal();
                    }
                });
            }
        },

        showRemoveModal: function(productId, button) {
            const modal = document.getElementById('wishlistRemoveModal');
            if (modal) {
                this.pendingRemove = { productId, button };
                modal.classList.add('is-open');
                document.body.classList.add('no-scroll');
            }
        },

        closeRemoveModal: function() {
            const modal = document.getElementById('wishlistRemoveModal');
            if (modal) {
                this.pendingRemove = null;
                modal.classList.remove('is-open');
                document.body.classList.remove('no-scroll');
            }
        },

        performRemove: async function(productId, button) {
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
                    // Update ALL buttons with the same productId on the page
                    const allButtons = document.querySelectorAll(`[data-product-id="${productId}"]`);
                    
                    // Remove from wishlist - update all buttons
                    allButtons.forEach(btn => {
                        btn.classList.remove('product__wishlist-btn--active', 'wishlist-active');
                        const icon = btn.querySelector('.wishlist-icon, i[data-lucide="heart"]');
                        if (icon) {
                            icon.setAttribute('data-lucide', 'heart');
                            icon.removeAttribute('fill');
                        }
                    });
                    
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
                    // On product listing pages (homepage, catalog, etc.), no need to reload
                    // All buttons with the same productId have already been updated above

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
                console.error('Error removing from wishlist:', error);
                this.showToast('An error occurred. Please try again.', 'error');
            } finally {
                button.disabled = false;
            }
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

            // Show confirmation modal if removing from wishlist
            if (isCurrentlyLiked) {
                this.showRemoveModal(productId, button);
                return;
            }

            // If adding to wishlist, proceed directly
            this.performAdd(productId, button);
        },

        performAdd: async function(productId, button) {
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
                    // Update ALL buttons with the same productId on the page
                    const allButtons = document.querySelectorAll(`[data-product-id="${productId}"]`);
                    
                    if (data.isLiked) {
                        // Add to wishlist - update all buttons
                        allButtons.forEach(btn => {
                            btn.classList.add('product__wishlist-btn--active', 'wishlist-active');
                            // Try multiple selectors to find the icon
                            const icon = btn.querySelector('.wishlist-icon') || 
                                       btn.querySelector('i[data-lucide="heart"]') ||
                                       btn.querySelector('i');
                            if (icon) {
                                icon.setAttribute('data-lucide', 'heart');
                                icon.setAttribute('fill', 'currentColor');
                                icon.classList.add('wishlist-icon');
                            }
                        });
                        
                        // Reinitialize Lucide icons immediately
                        if (typeof lucide !== 'undefined') {
                            lucide.createIcons();
                        }
                        
                        // Show success message
                        this.showToast('Added to wishlist', 'success');
                    } else {
                        // Remove from wishlist - update all buttons (shouldn't happen in performAdd, but handle it)
                        allButtons.forEach(btn => {
                            btn.classList.remove('product__wishlist-btn--active', 'wishlist-active');
                            const icon = btn.querySelector('.wishlist-icon, i[data-lucide="heart"]');
                            if (icon) {
                                icon.setAttribute('data-lucide', 'heart');
                                icon.removeAttribute('fill');
                            }
                        });
                        
                        // Show success message
                        this.showToast('Removed from wishlist', 'success');
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
                console.error('Error adding to wishlist:', error);
                this.showToast('An error occurred. Please try again.', 'error');
            } finally {
                button.disabled = false;
            }
        },

        showToast: function(message, type) {
            // Use existing toast system if available
            if (typeof Toast !== 'undefined' && typeof Toast.show === 'function') {
                Toast.show(message, type);
            } else if (typeof Toast !== 'undefined' && typeof Toast.success === 'function' && type === 'success') {
                Toast.success(message);
            } else if (typeof Toast !== 'undefined' && typeof Toast.error === 'function' && type === 'error') {
                Toast.error(message);
            } else {
                // Fallback: simple alert
                console.log(`Toast: ${message} (${type})`);
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
