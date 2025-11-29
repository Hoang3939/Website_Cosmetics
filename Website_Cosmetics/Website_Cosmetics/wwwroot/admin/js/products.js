// Products page functionality

document.addEventListener('DOMContentLoaded', function() {
    // Handle new image upload preview for Edit page
    const newImageUpload = document.getElementById('newImageUpload');
    if (newImageUpload) {
        newImageUpload.addEventListener('change', function(e) {
            const preview = document.getElementById('newImagePreview');
            if (!preview) return;
            
            preview.innerHTML = '';
            
            const files = e.target.files;
            for (let i = 0; i < files.length; i++) {
                const file = files[i];
                const reader = new FileReader();
                
                reader.onload = function(e) {
                    const col = document.createElement('div');
                    col.className = 'image-card';
                    
                    const imageIndex = document.querySelectorAll('#newImagePreview .image-card').length;
                    
                    col.innerHTML = `
                        <img src="${e.target.result}" class="image-card__img" />
                        <div class="image-card__body">
                            <div class="form-check">
                                <input class="form-check-input new-primary-checkbox" 
                                       type="checkbox" 
                                       name="primaryImageIndices" 
                                       value="${imageIndex}" 
                                       id="new_primary_${imageIndex}">
                                <label class="form-check-label" for="new_primary_${imageIndex}">
                                    Set as Primary Image
                                </label>
                            </div>
                            <div class="form-check">
                                <input class="form-check-input new-makeup-checkbox" 
                                       type="checkbox" 
                                       name="makeupReferenceIndices" 
                                       value="${imageIndex}" 
                                       id="new_makeup_${imageIndex}">
                                <label class="form-check-label" for="new_makeup_${imageIndex}">
                                    Use for Virtual Try-On
                                </label>
                            </div>
                        </div>
                    `;
                    
                    preview.appendChild(col);
                };
                
                reader.readAsDataURL(file);
            }

            setTimeout(() => {
                attachNewImageCheckboxListeners();
            }, 100);
        });
    }

    attachNewImageCheckboxListeners();
    attachExistingImageCheckboxListeners();
    attachDeleteImageHandlers();
});

function attachNewImageCheckboxListeners() {
    // Handle primary image checkbox for new images - only one can be selected
    document.querySelectorAll('.new-primary-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                // Uncheck all other primary checkboxes (both existing and new)
                document.querySelectorAll('.primary-checkbox, .new-primary-checkbox').forEach(cb => {
                    if (cb !== this) {
                        cb.checked = false;
                    }
                });
            }
        });
    });

    // Handle makeup reference checkbox for new images - only one can be selected
    document.querySelectorAll('.new-makeup-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                // Uncheck all other makeup reference checkboxes (both existing and new)
                document.querySelectorAll('.makeup-checkbox, .new-makeup-checkbox').forEach(cb => {
                    if (cb !== this) {
                        cb.checked = false;
                    }
                });
            }
        });
    });
}

function attachExistingImageCheckboxListeners() {
    // Handle primary image checkbox for existing images - only one can be selected
    document.querySelectorAll('.primary-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                // Uncheck all other primary checkboxes (both existing and new)
                document.querySelectorAll('.primary-checkbox, .new-primary-checkbox').forEach(cb => {
                    if (cb !== this) {
                        cb.checked = false;
                    }
                });
            }
        });
    });

    // Handle makeup reference checkbox for existing images - only one can be selected
    document.querySelectorAll('.makeup-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            if (this.checked) {
                // Uncheck all other makeup reference checkboxes (both existing and new)
                document.querySelectorAll('.makeup-checkbox, .new-makeup-checkbox').forEach(cb => {
                    if (cb !== this) {
                        cb.checked = false;
                    }
                });
            }
        });
    });
}

function attachDeleteImageHandlers() {
    // Handle delete image button
    document.querySelectorAll('.btn-delete-image').forEach(button => {
        button.addEventListener('click', function() {
            const imageId = this.getAttribute('data-image-id');
            if (confirm('Are you sure you want to delete this image?')) {
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                if (!token) {
                    alert('Security token not found');
                    return;
                }

                const deleteUrl = button.getAttribute('data-delete-url') || '/Admin/Products/DeleteImage';
                
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
                        this.closest('.image-card, .col-md-3').remove();
                    } else {
                        alert('Error deleting image: ' + (data.message || 'Unknown error'));
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('Error deleting image');
                });
            }
        });
    });
}

