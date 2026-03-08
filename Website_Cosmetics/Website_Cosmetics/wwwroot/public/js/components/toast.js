// Minimal toast: success / error, top-right stack
; (() => {
    const Toast = {
        _wrap: null,
        init() {
            if (this._wrap) return;
            const wrap = document.createElement('div');
            wrap.className = 'toast-wrap';
            document.body.appendChild(wrap);
            this._wrap = wrap;
        },
        show(message, type = 'success', ms = 2500) {
            this.init();
            const item = document.createElement('div');
            item.className = `toast toast--${type}`;
            item.role = 'status';
            item.innerHTML = `<span class="toast__icon">${type === 'success' ? '✔' : '✖'}</span><span>${message}</span>`;
            this._wrap.appendChild(item);
            // animate in
            requestAnimationFrame(() => item.classList.add('is-in'));
            const remove = () => {
                item.classList.remove('is-in');
                item.addEventListener('transitionend', () => item.remove(), { once: true });
            };
            const t = setTimeout(remove, ms);
            item.addEventListener('click', () => { clearTimeout(t); remove(); });
        },
        success(msg, ms) { this.show(msg, 'success', ms); },
        error(msg, ms) { this.show(msg, 'error', ms); }
    };
    window.Toast = Toast;
})();
