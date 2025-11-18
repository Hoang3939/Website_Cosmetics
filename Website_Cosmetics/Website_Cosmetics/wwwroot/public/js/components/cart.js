// Cart component - Handles shopping cart operations
(function() {
    'use strict';

    const Cart = {
        init: function() {
            this.bindEvents();
            this.updateCartCount();
        },

        bindEvents: function() {
            // Handle quantity decrease
            document.addEventListener('click', (e) => {
                if (e.target.closest('.quantity-control__btn--decrease')) {
                    e.preventDefault();
                    const btn = e.target.closest('.quantity-control__btn--decrease');
                    const cartItemId = parseInt(btn.getAttribute('data-cart-item-id'));
                    this.decreaseQuantity(cartItemId);
                }
            });

            // Handle quantity increase
            document.addEventListener('click', (e) => {
                if (e.target.closest('.quantity-control__btn--increase')) {
                    e.preventDefault();
                    const btn = e.target.closest('.quantity-control__btn--increase');
                    const cartItemId = parseInt(btn.getAttribute('data-cart-item-id'));
                    const stock = parseInt(btn.getAttribute('data-stock'));
                    this.increaseQuantity(cartItemId, stock);
                }
            });

            // Handle quantity input change
            document.addEventListener('change', (e) => {
                if (e.target.classList.contains('quantity-control__input')) {
                    const input = e.target;
                    const cartItemId = parseInt(input.getAttribute('data-cart-item-id'));
                    const stock = parseInt(input.getAttribute('data-stock'));
                    const quantity = parseInt(input.value) || 1;
                    this.updateQuantity(cartItemId, quantity, stock);
                }
            });

            // Handle remove item
            document.addEventListener('click', (e) => {
                if (e.target.closest('.cart-item__remove')) {
                    e.preventDefault();
                    e.stopPropagation();
                    const btn = e.target.closest('.cart-item__remove');
                    if (btn.disabled) return; // Prevent double click
                    btn.disabled = true;
                    const cartItemId = parseInt(btn.getAttribute('data-cart-item-id'));
                    this.removeItem(cartItemId).finally(() => {
                        btn.disabled = false;
                    });
                }
            }, { once: false, passive: false });
        },

        decreaseQuantity: function(cartItemId) {
            const input = document.querySelector(`.quantity-control__input[data-cart-item-id="${cartItemId}"]`);
            if (!input) return;

            const currentQuantity = parseInt(input.value) || 1;
            if (currentQuantity > 1) {
                input.value = currentQuantity - 1;
                this.updateQuantity(cartItemId, currentQuantity - 1);
            }
        },

        increaseQuantity: function(cartItemId, stock) {
            const input = document.querySelector(`.quantity-control__input[data-cart-item-id="${cartItemId}"]`);
            if (!input) return;

            const currentQuantity = parseInt(input.value) || 1;
            if (currentQuantity < stock) {
                input.value = currentQuantity + 1;
                this.updateQuantity(cartItemId, currentQuantity + 1);
            } else {
                this.showToast(`Only ${stock} items available in stock`, 'error');
            }
        },

        updateQuantity: async function(cartItemId, quantity, stock) {
            if (quantity <= 0) {
                this.showToast('Quantity must be greater than 0', 'error');
                return;
            }

            if (stock && quantity > stock) {
                quantity = stock;
                const input = document.querySelector(`.quantity-control__input[data-cart-item-id="${cartItemId}"]`);
                if (input) input.value = stock;
                this.showToast(`Only ${stock} items available in stock`, 'error');
            }

            try {
                let token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                if (!token) {
                    token = document.querySelector('meta[name="__RequestVerificationToken"]')?.content;
                }

                const formData = new FormData();
                formData.append('cartItemId', cartItemId);
                formData.append('quantity', quantity);
                if (token) {
                    formData.append('__RequestVerificationToken', token);
                }

                const response = await fetch('/Cart/Update', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (data.success) {
                    // Update subtotal
                    const cartItem = document.querySelector(`.cart-item[data-cart-item-id="${cartItemId}"]`);
                    if (cartItem) {
                        const subtotalElement = cartItem.querySelector('.cart-item__subtotal-value');
                        if (subtotalElement) {
                            subtotalElement.textContent = `$${data.subtotal.toFixed(2)}`;
                        }
                    }

                    // Update cart summary
                    this.updateCartSummary(data.cartTotal);
                    this.updateCartCount(data.cartCount);
                } else {
                    this.showToast(data.message || 'Failed to update cart', 'error');
                }
            } catch (error) {
                console.error('Error updating cart:', error);
                this.showToast('An error occurred. Please try again.', 'error');
            }
        },

        removeItem: async function(cartItemId) {
            // Prevent multiple calls
            if (this._removingItems && this._removingItems.has(cartItemId)) {
                return;
            }
            
            if (!this._removingItems) {
                this._removingItems = new Set();
            }
            this._removingItems.add(cartItemId);

            if (!confirm('Are you sure you want to remove this item from your cart?')) {
                this._removingItems.delete(cartItemId);
                return;
            }

            try {
                let token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                if (!token) {
                    token = document.querySelector('meta[name="__RequestVerificationToken"]')?.content;
                }

                const formData = new FormData();
                formData.append('cartItemId', cartItemId);
                if (token) {
                    formData.append('__RequestVerificationToken', token);
                }

                const response = await fetch('/Cart/Remove', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (data.success) {
                    // Remove item from DOM
                    const cartItem = document.querySelector(`.cart-item[data-cart-item-id="${cartItemId}"]`);
                    if (cartItem) {
                        cartItem.style.transition = 'opacity 0.3s ease';
                        cartItem.style.opacity = '0';
                        setTimeout(() => {
                            cartItem.remove();
                            this.updateCartSummary(data.cartTotal);
                            this.updateCartCount(data.cartCount);

                            // Check if cart is empty
                            const remainingItems = document.querySelectorAll('.cart-item');
                            if (remainingItems.length === 0) {
                                location.reload();
                            }
                        }, 300);
                    }

                    // Show toast only once with server message
                    this.showToast(data.message || 'Item removed from cart', 'success');
                } else {
                    this.showToast(data.message || 'Failed to remove item', 'error');
                }
            } catch (error) {
                console.error('Error removing item:', error);
                this.showToast('An error occurred. Please try again.', 'error');
            } finally {
                this._removingItems.delete(cartItemId);
            }
        },

        addToCart: async function(variantId, quantity = 1) {
            return new Promise(async (resolve, reject) => {
                try {
                    let token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                    if (!token) {
                        token = document.querySelector('meta[name="__RequestVerificationToken"]')?.content;
                    }

                    const formData = new FormData();
                    formData.append('variantId', variantId);
                    formData.append('quantity', quantity);
                    if (token) {
                        formData.append('__RequestVerificationToken', token);
                    }

                    const response = await fetch('/Cart/Add', {
                        method: 'POST',
                        body: formData
                    });

                    const data = await response.json();

                    if (data.success) {
                        this.updateCartCount(data.cartCount);
                        this.showToast(data.message || 'Item added to cart', 'success');
                        resolve(data);
                    } else {
                        if (data.requiresLogin) {
                            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                            reject(new Error('Login required'));
                        } else {
                            this.showToast(data.message || 'Failed to add item to cart', 'error');
                            reject(new Error(data.message || 'Failed to add item to cart'));
                        }
                    }
                } catch (error) {
                    console.error('Error adding to cart:', error);
                    this.showToast('An error occurred. Please try again.', 'error');
                    reject(error);
                }
            });
        },

        updateCartSummary: function(total) {
            const subtotalElement = document.querySelector('.cart-summary__subtotal');
            const totalElement = document.querySelector('.cart-summary__total');
            
            if (subtotalElement) {
                subtotalElement.textContent = `$${total.toFixed(2)}`;
            }
            if (totalElement) {
                totalElement.textContent = `$${total.toFixed(2)}`;
            }
        },

        updateCartCount: async function(count) {
            if (count !== undefined) {
                this.setCartCount(count);
            } else {
                try {
                    const response = await fetch('/Cart/GetCount');
                    const data = await response.json();
                    this.setCartCount(data.count);
                } catch (error) {
                    console.error('Error fetching cart count:', error);
                }
            }
        },

        setCartCount: function(count) {
            const cartBadge = document.querySelector('.cart-badge');
            const cartCount = document.querySelector('.cart-count');
            
            if (cartBadge) {
                if (count > 0) {
                    cartBadge.textContent = count;
                    cartBadge.style.display = 'flex';
                } else {
                    cartBadge.style.display = 'none';
                }
            }

            if (cartCount) {
                cartCount.textContent = count;
            }
        },

        showToast: function(message, type) {
            if (typeof Toast !== 'undefined' && typeof Toast.show === 'function') {
                Toast.show(message, type);
            } else {
                alert(message);
            }
        }
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => Cart.init());
    } else {
        Cart.init();
    }

    // Export for global access
    window.Cart = Cart;
})();

