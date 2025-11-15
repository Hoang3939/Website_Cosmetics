// Checkout component - Handles checkout form submission
(function() {
    'use strict';

    const Checkout = {
        init: function() {
            this.bindEvents();
            this.updateShippingFee();
        },

        bindEvents: function() {
            const form = document.getElementById('checkoutForm');
            if (form) {
                form.addEventListener('submit', (e) => {
                    e.preventDefault();
                    this.submitOrder();
                });
            }

            // Update shipping fee when method changes
            const shippingMethods = document.querySelectorAll('input[name="shippingMethod"]');
            shippingMethods.forEach(radio => {
                radio.addEventListener('change', () => {
                    this.updateShippingFee();
                });
            });

            // Update address text when address selection changes
            const addressRadios = document.querySelectorAll('input[name="selectedAddressId"]');
            addressRadios.forEach(radio => {
                radio.addEventListener('change', () => {
                    this.updateSelectedAddress();
                    this.updateAddressSelection();
                });
            });
            // Initialize address text on load
            this.updateSelectedAddress();
            this.updateAddressSelection();
        },

        updateSelectedAddress: function() {
            const selectedRadio = document.querySelector('input[name="selectedAddressId"]:checked');
            const addressTextInput = document.getElementById('selectedAddressText');
            
            if (selectedRadio && addressTextInput) {
                const addressOption = selectedRadio.closest('.address-option');
                if (addressOption) {
                    const details = addressOption.querySelector('.address-option__details');
                    if (details) {
                        const addressLines = Array.from(details.querySelectorAll('div'))
                            .map(div => div.textContent.trim())
                            .filter(text => text && !text.startsWith('Phone:'));
                        addressTextInput.value = addressLines.join(', ');
                    }
                }
            }
        },

        updateAddressSelection: function() {
            // Remove selected class from all address options
            const allAddressOptions = document.querySelectorAll('.address-option');
            allAddressOptions.forEach(option => {
                option.classList.remove('address-option--selected');
            });

            // Add selected class to the checked address option
            const selectedRadio = document.querySelector('input[name="selectedAddressId"]:checked');
            if (selectedRadio) {
                const selectedOption = selectedRadio.closest('.address-option');
                if (selectedOption) {
                    selectedOption.classList.add('address-option--selected');
                }
            }
        },

        updateShippingFee: function() {
            const selectedMethod = document.querySelector('input[name="shippingMethod"]:checked');
            if (!selectedMethod) return;

            const shippingFeeElement = document.getElementById('shippingFee');
            const orderTotalElement = document.getElementById('orderTotal');
            
            if (!shippingFeeElement || !orderTotalElement) return;

            let shippingFee = 5.00;
            switch (selectedMethod.value) {
                case 'Express':
                    shippingFee = 15.00;
                    break;
                case 'Same-Day':
                    shippingFee = 25.00;
                    break;
                default:
                    shippingFee = 5.00;
            }

            // Get subtotal from summary
            const subtotalText = document.querySelector('.checkout-summary__subtotal')?.textContent || '$0.00';
            const subtotal = parseFloat(subtotalText.replace('$', '').replace(',', '')) || 0;

            shippingFeeElement.textContent = `$${shippingFee.toFixed(2)}`;
            orderTotalElement.textContent = `$${(subtotal + shippingFee).toFixed(2)}`;
        },

        submitOrder: async function() {
            const form = document.getElementById('checkoutForm');
            if (!form) return;

            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Processing...';
            }

            try {
                const formData = new FormData(form);
                
                // Get selected address ID
                const selectedAddressRadio = document.querySelector('input[name="selectedAddressId"]:checked');
                if (selectedAddressRadio) {
                    formData.append('selectedAddressId', selectedAddressRadio.value);
                }
                
                let token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                if (!token) {
                    token = document.querySelector('meta[name="__RequestVerificationToken"]')?.content;
                }
                if (token) {
                    formData.append('__RequestVerificationToken', token);
                }

                const response = await fetch('/Orders/Create', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();

                if (data.success) {
                    // Redirect to order details page
                    window.location.href = `/Orders/Details/${data.orderId}`;
                } else {
                    this.showToast(data.message || 'Failed to place order', 'error');
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.textContent = 'Place Order';
                    }
                }
            } catch (error) {
                console.error('Error placing order:', error);
                this.showToast('An error occurred. Please try again.', 'error');
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.textContent = 'Place Order';
                }
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
        document.addEventListener('DOMContentLoaded', () => Checkout.init());
    } else {
        Checkout.init();
    }

    // Export for global access
    window.Checkout = Checkout;
})();

