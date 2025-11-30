// Search autocomplete component
(function() {
    'use strict';

    const Search = {
        input: null,
        dropdown: null,
        debounceTimer: null,
        isSearching: false,

        init: function() {
            this.input = document.querySelector('.header__search input');
            if (!this.input) return;

            // Create dropdown container
            this.createDropdown();

            // Bind events
            this.input.addEventListener('input', (e) => this.handleInput(e));
            this.input.addEventListener('focus', () => this.handleFocus());
            this.input.addEventListener('blur', () => this.handleBlur());
            this.input.addEventListener('keydown', (e) => this.handleKeydown(e));

            // Close dropdown when clicking outside
            document.addEventListener('click', (e) => {
                if (!e.target.closest('.header__search')) {
                    this.hideDropdown();
                }
            });
        },

        createDropdown: function() {
            const searchContainer = this.input.closest('.header__search');
            if (!searchContainer) return;

            this.dropdown = document.createElement('div');
            this.dropdown.className = 'search-dropdown';
            this.dropdown.innerHTML = '<div class="search-dropdown__content"></div>';
            searchContainer.appendChild(this.dropdown);
        },

        handleInput: function(e) {
            const keyword = e.target.value.trim();

            // Clear previous timer
            if (this.debounceTimer) {
                clearTimeout(this.debounceTimer);
            }

            // If empty, hide dropdown
            if (!keyword) {
                this.hideDropdown();
                return;
            }

            // Debounce: wait 2 seconds before searching
            this.debounceTimer = setTimeout(() => {
                this.performSearch(keyword);
            }, 2000);
        },

        handleFocus: function() {
            // If there's a keyword, show results immediately
            const keyword = this.input.value.trim();
            if (keyword && this.dropdown.querySelector('.search-result')) {
                this.showDropdown();
            }
        },

        handleBlur: function() {
            // Delay to allow click events on dropdown items
            setTimeout(() => {
                if (!document.activeElement.closest('.search-dropdown')) {
                    this.hideDropdown();
                }
            }, 200);
        },

        handleKeydown: function(e) {
            if (e.key === 'Enter') {
                const keyword = this.input.value.trim();
                if (keyword) {
                    // Navigate to search results page
                    window.location.href = `/Products?search=${encodeURIComponent(keyword)}`;
                }
            } else if (e.key === 'Escape') {
                this.hideDropdown();
            }
        },

        async performSearch(keyword) {
            if (this.isSearching) return;

            this.isSearching = true;
            this.showLoading();

            try {
                const response = await fetch(`/Search/Autocomplete?keyword=${encodeURIComponent(keyword)}`);
                const data = await response.json();

                if (data.success && data.products) {
                    this.displayResults(data.products, keyword);
                } else {
                    this.showNoResults();
                }
            } catch (error) {
                console.error('Search error:', error);
                this.showError();
            } finally {
                this.isSearching = false;
            }
        },

        showLoading: function() {
            const content = this.dropdown.querySelector('.search-dropdown__content');
            if (content) {
                content.innerHTML = '<div class="search-dropdown__loading">Searching...</div>';
                this.showDropdown();
            }
        },

        displayResults: function(products, keyword) {
            const content = this.dropdown.querySelector('.search-dropdown__content');
            if (!content) return;

            if (products.length === 0) {
                this.showNoResults();
                return;
            }

            let html = '';
            products.forEach(product => {
                html += `
                    <a href="${product.url}" class="search-result">
                        <div class="search-result__image">
                            <img src="${product.imageUrl}" alt="${product.name}" />
                        </div>
                        <div class="search-result__info">
                            <div class="search-result__brand">${this.escapeHtml(product.brand)}</div>
                            <div class="search-result__name">${this.highlightKeyword(this.escapeHtml(product.name), keyword)}</div>
                            <div class="search-result__price">$${product.price.toFixed(2)}</div>
                        </div>
                    </a>
                `;
            });

            content.innerHTML = html;
            this.showDropdown();
        },

        showNoResults: function() {
            const content = this.dropdown.querySelector('.search-dropdown__content');
            if (content) {
                content.innerHTML = '<div class="search-dropdown__empty">No products found</div>';
                this.showDropdown();
            }
        },

        showError: function() {
            const content = this.dropdown.querySelector('.search-dropdown__content');
            if (content) {
                content.innerHTML = '<div class="search-dropdown__error">An error occurred. Please try again.</div>';
                this.showDropdown();
            }
        },

        showDropdown: function() {
            if (this.dropdown) {
                this.dropdown.classList.add('is-open');
            }
        },

        hideDropdown: function() {
            if (this.dropdown) {
                this.dropdown.classList.remove('is-open');
            }
        },

        escapeHtml: function(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        },

        highlightKeyword: function(text, keyword) {
            if (!keyword) return text;
            const regex = new RegExp(`(${this.escapeRegex(keyword)})`, 'gi');
            return text.replace(regex, '<mark>$1</mark>');
        },

        escapeRegex: function(text) {
            return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        }
    };

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => Search.init());
    } else {
        Search.init();
    }

    // Export for global access
    window.Search = Search;
})();

