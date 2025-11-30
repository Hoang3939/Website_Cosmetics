// Variants page functionality

document.addEventListener('DOMContentLoaded', function() {
    // Variant image upload preview
    const imageUpload = document.getElementById('imageUpload');
    if (imageUpload) {
        imageUpload.addEventListener('change', function(e) {
            const preview = document.getElementById('imagePreview');
            if (!preview) return;
            
            preview.innerHTML = '';
            
            const files = e.target.files;
            for (let i = 0; i < files.length; i++) {
                const file = files[i];
                const reader = new FileReader();
                
                reader.onload = function(e) {
                    const col = document.createElement('div');
                    col.className = 'col-md-3 mb-3';
                    
                    col.innerHTML = `
                        <div class="card">
                            <img src="${e.target.result}" class="card-img-top" style="height: 200px; object-fit: cover;" />
                            <div class="card-body">
                                <div class="form-check">
                                    <input class="form-check-input primary-checkbox" 
                                           type="checkbox" 
                                           name="primaryImageIndices" 
                                           value="${i}" 
                                           id="primary_${i}"
                                           data-index="${i}">
                                    <label class="form-check-label" for="primary_${i}">
                                        Set as Primary Image
                                    </label>
                                </div>
                                <div class="form-check">
                                    <input class="form-check-input makeup-checkbox" 
                                           type="checkbox" 
                                           name="makeupReferenceIndices" 
                                           value="${i}" 
                                           id="makeup_${i}"
                                           data-index="${i}">
                                    <label class="form-check-label" for="makeup_${i}">
                                        Use for Virtual Try-On
                                    </label>
                                </div>
                            </div>
                        </div>
                    `;
                    
                    preview.appendChild(col);
                };
                
                reader.readAsDataURL(file);
            }

            setTimeout(() => {
                attachCheckboxListeners();
            }, 100);
        });
    }

    // Bulk delete functionality
    attachBulkDeleteHandlers();
    
    // Drag and drop for variant images
    attachDragAndDrop();
    
    // Checkbox listeners
    attachCheckboxListeners();
    attachExistingCheckboxListeners();
    attachDeleteImageHandlers();
});

function attachCheckboxListeners() {
    // Handle primary image checkbox - only one can be selected
    document.querySelectorAll('.primary-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                // Uncheck all other primary checkboxes
                document.querySelectorAll('.primary-checkbox').forEach(cb => {
                    if (cb !== this) {
                        cb.checked = false;
                    }
                });
            }
        });
    });

    // Handle makeup reference checkbox - only one can be selected
    document.querySelectorAll('.makeup-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                // Uncheck all other makeup reference checkboxes
                document.querySelectorAll('.makeup-checkbox').forEach(cb => {
                    if (cb !== this) {
                        cb.checked = false;
                    }
                });
            }
        });
    });
}

function attachExistingCheckboxListeners() {
    // Handle existing image checkboxes
    document.querySelectorAll('.existing-primary-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                document.querySelectorAll('.existing-primary-checkbox, .primary-checkbox').forEach(cb => {
                    if (cb !== this) cb.checked = false;
                });
            }
        });
    });

    document.querySelectorAll('.existing-makeup-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                document.querySelectorAll('.existing-makeup-checkbox, .makeup-checkbox').forEach(cb => {
                    if (cb !== this) cb.checked = false;
                });
            }
        });
    });
}

function attachBulkDeleteHandlers() {
    const selectAll = document.getElementById('selectAll');
    if (selectAll) {
        selectAll.addEventListener('change', function() {
            const checkboxes = document.querySelectorAll('.variant-checkbox');
            checkboxes.forEach(cb => cb.checked = this.checked);
            updateSelectedCount();
        });
    }

    document.querySelectorAll('.variant-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', updateSelectedCount);
    });
}

function updateSelectedCount() {
    const selected = document.querySelectorAll('.variant-checkbox:checked');
    const count = selected.length;
    const countElement = document.getElementById('selectedCount');
    const bulkDeleteBtn = document.getElementById('bulkDeleteBtn');
    
    if (countElement) {
        countElement.textContent = count;
    }
    
    if (bulkDeleteBtn) {
        bulkDeleteBtn.disabled = count === 0;
    }
}

function bulkDelete() {
    const selected = Array.from(document.querySelectorAll('.variant-checkbox:checked'))
        .map(cb => parseInt(cb.value));
    
    if (selected.length === 0) {
        alert('Please select at least 1 variant to delete');
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selected.length} selected variant(s)? This action cannot be undone.`)) {
        return;
    }

    const deleteUrl = document.getElementById('bulkDeleteBtn')?.getAttribute('data-delete-url') || '/Admin/Variants/BulkDelete';
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    fetch(deleteUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ variantIds: selected })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert(data.message);
            location.reload();
        } else {
            alert('Error: ' + data.message);
        }
    })
    .catch(error => {
        alert('Error deleting variants: ' + error);
    });
}

function attachDragAndDrop() {
    const container = document.getElementById('existingImages');
    if (!container) return;

    const items = container.querySelectorAll('.image-item');
    
    items.forEach(item => {
        item.addEventListener('dragstart', function(e) {
            this.style.opacity = '0.5';
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/html', this.innerHTML);
        });

        item.addEventListener('dragend', function(e) {
            this.style.opacity = '1';
            items.forEach(i => i.classList.remove('drag-over'));
        });

        item.addEventListener('dragover', function(e) {
            if (e.preventDefault) {
                e.preventDefault();
            }
            e.dataTransfer.dropEffect = 'move';
            this.classList.add('drag-over');
            return false;
        });

        item.addEventListener('dragleave', function(e) {
            this.classList.remove('drag-over');
        });

        item.addEventListener('drop', function(e) {
            if (e.stopPropagation) {
                e.stopPropagation();
            }
            this.classList.remove('drag-over');

            const draggedElement = container.querySelector('.image-item[style*="opacity: 0.5"]');
            if (draggedElement && draggedElement !== this) {
                const allItems = Array.from(container.querySelectorAll('.image-item'));
                const draggedIndex = allItems.indexOf(draggedElement);
                const targetIndex = allItems.indexOf(this);

                if (draggedIndex < targetIndex) {
                    container.insertBefore(draggedElement, this.nextSibling);
                } else {
                    container.insertBefore(draggedElement, this);
                }

                updateImageOrder();
            }

            return false;
        });
    });
}

function updateImageOrder() {
    const items = document.querySelectorAll('#existingImages .image-item');
    items.forEach((item, index) => {
        // Update order number display
        const orderNumber = item.querySelector('.order-number');
        if (orderNumber) {
            orderNumber.textContent = index + 1;
        }

        // Update hidden input for display order
        const displayOrderInput = item.querySelector('.display-order-input');
        if (displayOrderInput) {
            displayOrderInput.value = index;
        }

        // Update checkbox values to match new index
        const primaryCheckbox = item.querySelector('.existing-primary-checkbox');
        const makeupCheckbox = item.querySelector('.existing-makeup-checkbox');
        if (primaryCheckbox) {
            primaryCheckbox.value = index;
            primaryCheckbox.id = `existing_primary_${index}`;
        }
        if (makeupCheckbox) {
            makeupCheckbox.value = index;
            makeupCheckbox.id = `existing_makeup_${index}`;
        }
    });
}

function attachDeleteImageHandlers() {
    document.querySelectorAll('.btn-delete-image').forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const imageId = this.getAttribute('data-image-id');
            
            if (!imageId) return;
            
            if (!confirm('Are you sure you want to delete this image?')) {
                return;
            }

            const deleteUrl = this.getAttribute('data-delete-url') || '/Admin/Variants/DeleteImage';
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

            fetch(deleteUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ imageId: parseInt(imageId) })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const imageItem = this.closest('.image-item, .image-card, .col-md-3');
                    if (imageItem) {
                        imageItem.remove();
                        const container = document.getElementById('existingImages');
                        if (container) {
                            updateImageOrder();
                        }
                    }
                } else {
                    alert('Error deleting image: ' + data.message);
                }
            })
            .catch(error => {
                alert('Error deleting image: ' + error);
            });
        });
    });
}

// Global deleteImage function for onclick handlers
window.deleteImage = function(imageId, button) {
    if (!confirm('Are you sure you want to delete this image?')) {
        return;
    }

    const deleteUrl = button?.getAttribute('data-delete-url') || '/Admin/Variants/DeleteImage';
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

    fetch(deleteUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ imageId: parseInt(imageId) })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const imageItem = button?.closest('.image-item, .image-card, .col-md-3');
            if (imageItem) {
                imageItem.remove();
                const container = document.getElementById('existingImages');
                if (container) {
                    updateImageOrder();
                }
            }
        } else {
            alert('Error deleting image: ' + data.message);
        }
    })
    .catch(error => {
        alert('Error deleting image: ' + error);
    });
};

// Make bulkDelete available globally
window.bulkDelete = bulkDelete;
window.toggleSelectAll = function(checkbox) {
    const checkboxes = document.querySelectorAll('.variant-checkbox');
    checkboxes.forEach(cb => cb.checked = checkbox.checked);
    updateSelectedCount();
};
window.updateSelectedCount = updateSelectedCount;

