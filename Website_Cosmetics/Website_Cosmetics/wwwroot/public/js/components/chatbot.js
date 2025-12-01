// Chatbot Widget
const Chatbot = {
    isOpen: false,
    sessionId: null,
    messages: [],
    quickReplies: [
        "Which lipsticks are under $35?",
        "Foundation for oily skin",
        "Waterproof mascara"
    ],

    init: function() {
        this.sessionId = this.generateSessionId();
        this.render();
        this.attachEvents();
    },

    generateSessionId: function() {
        return 'chatbot_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    },

    render: function() {
        const chatbotHTML = `
            <div class="chatbot-widget">
                <button class="chatbot-button" id="chatbotToggle" aria-label="Open chatbot">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                    </svg>
                </button>
                <div class="chatbot-window" id="chatbotWindow">
                    <div class="chatbot-header">
                        <h3>Chat Support</h3>
                        <button class="chatbot-close" id="chatbotClose" aria-label="Close chatbot">
                            <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <line x1="18" y1="6" x2="6" y2="18"></line>
                                <line x1="6" y1="6" x2="18" y2="18"></line>
                            </svg>
                        </button>
                    </div>
                    <div class="chatbot-body">
                        <div class="chatbot-messages" id="chatbotMessages">
                            <div class="chatbot-message bot">
                                <div class="chatbot-message-bubble">
                                    Hello! How can I help you today? You can choose one of the questions below or ask your own question.
                                </div>
                            </div>
                        </div>
                        <div class="chatbot-quick-replies" id="chatbotQuickReplies">
                            ${this.quickReplies.map(reply => `
                                <button class="chatbot-quick-reply" data-message="${reply}">
                                    ${reply}
                                </button>
                            `).join('')}
                        </div>
                    </div>
                    <div class="chatbot-input-area">
                        <div class="chatbot-input-wrapper">
                            <input 
                                type="text" 
                                class="chatbot-input" 
                                id="chatbotInput" 
                                placeholder="Enter your question..."
                                autocomplete="off"
                            />
                            <button class="chatbot-send" id="chatbotSend" aria-label="Send message">
                                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <line x1="22" y1="2" x2="11" y2="13"></line>
                                    <polygon points="22 2 15 22 11 13 2 9 22 2"></polygon>
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', chatbotHTML);
    },

    attachEvents: function() {
        const toggleBtn = document.getElementById('chatbotToggle');
        const closeBtn = document.getElementById('chatbotClose');
        const sendBtn = document.getElementById('chatbotSend');
        const input = document.getElementById('chatbotInput');
        const quickReplies = document.querySelectorAll('.chatbot-quick-reply');

        toggleBtn.addEventListener('click', () => this.toggle());
        closeBtn.addEventListener('click', () => this.close());
        sendBtn.addEventListener('click', () => this.sendMessage());
        
        input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                this.sendMessage();
            }
        });

        quickReplies.forEach(btn => {
            btn.addEventListener('click', (e) => {
                const message = e.target.getAttribute('data-message');
                input.value = message;
                this.sendMessage();
            });
        });
    },

    toggle: function() {
        this.isOpen = !this.isOpen;
        const window = document.getElementById('chatbotWindow');
        if (this.isOpen) {
            window.classList.add('active');
            document.getElementById('chatbotInput').focus();
        } else {
            window.classList.remove('active');
        }
    },

    close: function() {
        this.isOpen = false;
        document.getElementById('chatbotWindow').classList.remove('active');
    },

    addMessage: function(text, isUser = false) {
        const messagesContainer = document.getElementById('chatbotMessages');
        const messageDiv = document.createElement('div');
        messageDiv.className = `chatbot-message ${isUser ? 'user' : 'bot'}`;
        messageDiv.innerHTML = `
            <div class="chatbot-message-bubble">${this.escapeHtml(text)}</div>
        `;
        messagesContainer.appendChild(messageDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
        
        // Hide quick replies after first user message
        if (isUser && this.messages.length === 0) {
            document.getElementById('chatbotQuickReplies').style.display = 'none';
        }
    },

    addLoading: function() {
        const messagesContainer = document.getElementById('chatbotMessages');
        const loadingDiv = document.createElement('div');
        loadingDiv.className = 'chatbot-message bot';
        loadingDiv.id = 'chatbotLoading';
        loadingDiv.innerHTML = `
            <div class="chatbot-loading">
                <span></span>
                <span></span>
                <span></span>
            </div>
        `;
        messagesContainer.appendChild(loadingDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    },

    removeLoading: function() {
        const loading = document.getElementById('chatbotLoading');
        if (loading) {
            loading.remove();
        }
    },

    sendMessage: async function() {
        const input = document.getElementById('chatbotInput');
        const message = input.value.trim();
        
        if (!message) return;

        // Add user message
        this.addMessage(message, true);
        this.messages.push({ role: 'user', content: message });
        input.value = '';
        
        // Show loading
        this.addLoading();
        
        // Disable input
        input.disabled = true;
        document.getElementById('chatbotSend').disabled = true;

        try {
            const response = await fetch('/api/chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    message: message,
                    sessionId: this.sessionId
                })
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const data = await response.json();
            
            // Remove loading
            this.removeLoading();
            
            // Add bot response
            this.addMessage(data.response || 'Sorry, I cannot answer this question.', false);
            this.messages.push({ role: 'bot', content: data.response });

            // Show products as simple links (no images)
            if (data.products && data.products.length > 0) {
                const validProducts = data.products.filter(p => {
                    // Double check: price must be > 0 and <= $35 (875,000 VND)
                    const maxPriceVND = 35 * 25000;
                    return p.price && p.price > 0 && p.price <= maxPriceVND;
                });
                if (validProducts.length > 0) {
                    this.addProductLinks(validProducts);
                }
            }

        } catch (error) {
            console.error('Chatbot error:', error);
            this.removeLoading();
            this.addMessage('Sorry, an error occurred. Please try again later.', false);
        } finally {
            // Re-enable input
            input.disabled = false;
            document.getElementById('chatbotSend').disabled = false;
            input.focus();
        }
    },

    formatPrice: function(price) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    },

    formatPriceUSD: function(priceVND) {
        // Convert VND to USD (approximate rate: 1 USD = 25,000 VND)
        const priceUSD = priceVND / 25000;
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(priceUSD);
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    addProductLinks: function(products) {
        const messagesContainer = document.getElementById('chatbotMessages');
        const productsContainer = document.createElement('div');
        productsContainer.className = 'chatbot-products';
        
        products.forEach(product => {
            const productLink = document.createElement('a');
            productLink.href = product.slug ? `/Products/Details/${product.slug}` : `/Products/Details/${product.productId}`;
            productLink.className = 'chatbot-product-link';
            productLink.target = '_blank';
            
            const productName = this.escapeHtml(product.name);
            const productPrice = this.formatPriceUSD(product.price);
            
            productLink.innerHTML = `
                <span class="chatbot-product-link-name">${productName}</span>
                <span class="chatbot-product-link-price">${productPrice}</span>
            `;
            
            productsContainer.appendChild(productLink);
        });
        
        messagesContainer.appendChild(productsContainer);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }
};

// Initialize chatbot when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => Chatbot.init());
} else {
    Chatbot.init();
}

