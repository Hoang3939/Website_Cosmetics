/**
 * Virtual Try-On Component
 * Xử lý upload ảnh và gọi PSGAN API
 */

class VirtualTryOn {
    constructor(productId) {
        this.productId = productId;
        this.selectedFile = null;
        this.init();
    }

    init() {
        this.cacheDOM();
        this.bindEvents();
    }

    cacheDOM() {
        // Button trigger
        this.triggerBtn = document.querySelector('[data-try-on]');
        
        // Modal elements
        this.modal = document.getElementById('virtualTryOnModal');
        this.closeBtn = document.getElementById('closeModal');
        this.cancelBtn = document.getElementById('cancelBtn');
        
        // Upload area
        this.uploadArea = document.getElementById('uploadArea');
        this.fileInput = document.getElementById('customerPhotoInput');
        this.uploadContent = document.getElementById('uploadContent');
        this.previewArea = document.getElementById('previewArea');
        this.previewImage = document.getElementById('previewImage');
        this.changePhotoBtn = document.getElementById('changePhotoBtn');
        
        // Loading
        this.loadingArea = document.getElementById('loadingArea');
        
        // Result
        this.resultArea = document.getElementById('resultArea');
        this.originalImage = document.getElementById('originalImage');
        this.sampleImage = document.getElementById('sampleImage');
        this.resultImage = document.getElementById('resultImage');
        this.downloadBtn = document.getElementById('downloadResultBtn');
        this.tryAgainBtn = document.getElementById('tryAgainBtn');
        
        // Apply button
        this.applyBtn = document.getElementById('applyMakeupBtn');
    }

    bindEvents() {
        // Open modal
        if (this.triggerBtn) {
            this.triggerBtn.addEventListener('click', () => this.openModal());
        }

        // Close modal
        if (this.closeBtn) {
            this.closeBtn.addEventListener('click', () => this.closeModal());
        }
        if (this.cancelBtn) {
            this.cancelBtn.addEventListener('click', () => this.closeModal());
        }

        // Upload area click
        if (this.uploadArea) {
            this.uploadArea.addEventListener('click', () => {
                if (!this.previewArea.style.display || this.previewArea.style.display === 'none') {
                    this.fileInput.click();
                }
            });
        }

        // Drag and drop
        if (this.uploadArea) {
            this.uploadArea.addEventListener('dragover', (e) => {
                e.preventDefault();
                this.uploadArea.classList.add('dragover');
            });

            this.uploadArea.addEventListener('dragleave', () => {
                this.uploadArea.classList.remove('dragover');
            });

            this.uploadArea.addEventListener('drop', (e) => {
                e.preventDefault();
                this.uploadArea.classList.remove('dragover');
                const files = e.dataTransfer.files;
                if (files.length > 0) {
                    this.handleFileSelect(files[0]);
                }
            });
        }

        // File input change
        if (this.fileInput) {
            this.fileInput.addEventListener('change', (e) => {
                if (e.target.files.length > 0) {
                    this.handleFileSelect(e.target.files[0]);
                }
            });
        }

        // Change photo
        if (this.changePhotoBtn) {
            this.changePhotoBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                this.fileInput.click();
            });
        }

        // Apply makeup
        if (this.applyBtn) {
            this.applyBtn.addEventListener('click', () => this.applyMakeup());
        }

        // Try again
        if (this.tryAgainBtn) {
            this.tryAgainBtn.addEventListener('click', () => this.reset());
        }

        // Download result
        if (this.downloadBtn) {
            this.downloadBtn.addEventListener('click', () => this.downloadResult());
        }

        // Close on overlay click
        if (this.modal) {
            this.modal.querySelector('.modal__overlay')?.addEventListener('click', () => this.closeModal());
        }
    }

    openModal() {
        if (this.modal) {
            // Force center positioning with inline styles to override any CSS
            this.modal.style.cssText = `
                position: fixed !important;
                top: 0 !important;
                left: 0 !important;
                right: 0 !important;
                bottom: 0 !important;
                width: 100vw !important;
                height: 100vh !important;
                display: flex !important;
                flex-direction: row !important;
                align-items: center !important;
                justify-content: center !important;
                background: rgba(0, 0, 0, 0.5) !important;
                z-index: 9999 !important;
                overflow-y: auto !important;
                overflow-x: hidden !important;
                padding: 2rem !important;
                margin: 0 !important;
                transform: none !important;
            `;
            
            this.modal.classList.add('is-open');
            document.body.style.overflow = 'hidden';
            document.body.classList.add('no-scroll');
            
            // Ensure modal content is centered
            const modalContent = this.modal.querySelector('.modal__content');
            if (modalContent) {
                modalContent.style.cssText += `
                    margin-left: auto !important;
                    margin-right: auto !important;
                    margin-top: auto !important;
                    margin-bottom: auto !important;
                `;
            }
            
            // Load sample image for current variant when modal opens
            const selectedVariantInput = document.getElementById('selectedVariantId');
            if (selectedVariantInput && selectedVariantInput.value) {
                const variantId = parseInt(selectedVariantInput.value);
                if (variantId > 0) {
                    this.loadSampleImageFromVariant(variantId);
                }
            }
        }
    }

    closeModal() {
        if (this.modal) {
            this.modal.classList.remove('is-open');
            document.body.classList.remove('no-scroll');
            setTimeout(() => {
                this.modal.style.display = 'none';
            }, 200); // Wait for animation
            document.body.style.overflow = '';
            this.reset();
        }
    }

    handleFileSelect(file) {
        // Validate file type
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png'];
        if (!allowedTypes.includes(file.type)) {
            alert('Chỉ hỗ trợ file JPG, JPEG, PNG');
            return;
        }

        // Validate file size (10MB)
        if (file.size > 10 * 1024 * 1024) {
            alert('Kích thước file tối đa 10MB');
            return;
        }

        this.selectedFile = file;

        // Show preview
        const reader = new FileReader();
        reader.onload = (e) => {
            this.previewImage.src = e.target.result;
            this.uploadContent.style.display = 'none';
            this.previewArea.style.display = 'block';
            this.applyBtn.disabled = false;
        };
        reader.readAsDataURL(file);
    }

    async applyMakeup() {
        if (!this.selectedFile) {
            alert('Vui lòng chọn ảnh');
            return;
        }

        // Get selected variant ID
        const selectedVariantInput = document.getElementById('selectedVariantId');
        const variantId = selectedVariantInput ? selectedVariantInput.value : null;
        
        // Log for debugging
        console.log('=== VirtualTryOn applyMakeup ===');
        console.log('selectedVariantInput:', selectedVariantInput);
        console.log('variantId from input:', variantId);
        console.log('variantId type:', typeof variantId);
        
        if (!variantId || variantId === '0' || variantId === 0) {
            console.error('ERROR: No variant selected');
            alert('Vui lòng chọn màu sản phẩm trước khi thử makeup');
            return;
        }

        // Convert to number to ensure it's valid
        const numericVariantId = parseInt(variantId);
        if (isNaN(numericVariantId) || numericVariantId <= 0) {
            console.error('ERROR: Invalid variantId:', variantId);
            alert('Màu sản phẩm không hợp lệ. Vui lòng chọn lại màu và thử lại.');
            return;
        }

        console.log('Sending variantId:', numericVariantId);

        // Show loading
        this.uploadArea.style.display = 'none';
        this.loadingArea.style.display = 'block';
        this.applyBtn.disabled = true;

        const formData = new FormData();
        formData.append('variantId', numericVariantId.toString());
        formData.append('customerPhoto', this.selectedFile);

        try {
            console.log('Calling /Products/VirtualTryOn with variantId:', numericVariantId);
            const response = await fetch('/Products/VirtualTryOn', {
                method: 'POST',
                body: formData
            });
            
            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers);

            if (response.ok && response.headers.get('content-type')?.includes('image')) {
                // Success - received image
                const blob = await response.blob();
                const imageUrl = URL.createObjectURL(blob);
                
                // Get makeup reference image URL from response header
                const makeupReferenceUrl = response.headers.get('X-Makeup-Reference-URL');
                
                // Show result
                this.originalImage.src = this.previewImage.src;
                
                // Set sample image (makeup reference)
                if (makeupReferenceUrl) {
                    this.sampleImage.src = makeupReferenceUrl;
                    this.sampleImage.onerror = () => {
                        // Fallback: try to get from current variant
                        this.loadSampleImageFromVariant(numericVariantId);
                    };
                } else {
                    // Fallback: get from current variant
                    this.loadSampleImageFromVariant(numericVariantId);
                }
                
                this.resultImage.src = imageUrl;
                this.resultImage.dataset.url = imageUrl; // Save for download

                this.loadingArea.style.display = 'none';
                this.resultArea.style.display = 'block';
            } else {
                // Error - received JSON
                const error = await response.json();
                throw new Error(error.message || 'Đã xảy ra lỗi');
            }
        } catch (error) {
            console.error('Error:', error);
            alert('Lỗi: ' + error.message);
            this.reset();
        }
    }

    downloadResult() {
        const imageUrl = this.resultImage.dataset.url;
        if (imageUrl) {
            const link = document.createElement('a');
            link.href = imageUrl;
            link.download = `virtual-makeup-${Date.now()}.png`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }

    async loadSampleImageFromVariant(variantId, images = null) {
        try {
            let imageData = images;
            
            // If images not provided, fetch from API
            if (!imageData) {
                const response = await fetch(`/Products/GetVariant?variantId=${variantId}`);
                const data = await response.json();
                imageData = data?.images;
            }
            
            if (imageData && imageData.length > 0) {
                // Find image with isMakeupReference = true, or fallback to highest DisplayOrder
                const makeupRefImage = imageData.find(img => img.isMakeupReference === true) 
                                    || imageData.sort((a, b) => (b.displayOrder || 0) - (a.displayOrder || 0))[0];
                
                if (makeupRefImage && makeupRefImage.url && this.sampleImage) {
                    console.log('Updating sample image:', makeupRefImage.url);
                    this.sampleImage.src = makeupRefImage.url;
                    // Ensure sample image is visible if result area is showing
                    if (this.resultArea && this.resultArea.style.display !== 'none') {
                        this.resultArea.style.display = 'block';
                    }
                }
            }
        } catch (error) {
            console.error('Error loading sample image:', error);
            // Set a placeholder if error
            if (this.sampleImage) {
                this.sampleImage.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2YzZjRmNiIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iMTQiIGZpbGw9IiM5Y2EzYWYiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj7guYDguIjguLXguJjguLjguKHguJg8L3RleHQ+PC9zdmc+';
            }
        }
    }

    reset() {
        this.selectedFile = null;
        this.uploadContent.style.display = 'block';
        this.previewArea.style.display = 'none';
        this.loadingArea.style.display = 'none';
        this.resultArea.style.display = 'none';
        this.uploadArea.style.display = 'block';
        this.applyBtn.disabled = true;
        this.fileInput.value = '';
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Get product ID from URL or data attribute
    const pathParts = window.location.pathname.split('/');
    const productId = pathParts[pathParts.length - 1];
    
    if (productId) {
        // Store instance globally so it can be accessed from other scripts
        window.virtualTryOnInstance = new VirtualTryOn(productId);
        
        // Listen for variant changes to update sample image
        const variantOptions = document.querySelectorAll('.variant-option');
        variantOptions.forEach(option => {
            option.addEventListener('click', async function() {
                if (this.disabled) return;
                
                const variantId = this.dataset.variantId;
                if (variantId && window.virtualTryOnInstance) {
                    // Update sample image in modal when variant changes
                    await window.virtualTryOnInstance.loadSampleImageFromVariant(parseInt(variantId));
                }
            });
        });
    }
});

