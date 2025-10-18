// Simple reusable modal with overlay, ESC close, focus trap, and nav slots
; (() => {
    const Modal = {
        _overlay: null,
        _dialog: null,
        _prevActive: null,
        _escHandler: null,
        _focusables: [],
        _first: null,
        _last: null,

        init() {
            if (this._overlay) return;

            const overlay = document.createElement('div');
            overlay.className = 'modal-overlay';
            overlay.setAttribute('aria-hidden', 'true');

            // nav buttons (outside dialog)
            const navPrev = document.createElement('button');
            navPrev.className = 'modal-nav modal-nav--prev';
            navPrev.type = 'button';
            navPrev.innerHTML = '‹';
            navPrev.setAttribute('data-modal-nav', 'prev');

            const navNext = document.createElement('button');
            navNext.className = 'modal-nav modal-nav--next';
            navNext.type = 'button';
            navNext.innerHTML = '›';
            navNext.setAttribute('data-modal-nav', 'next');

            const dialog = document.createElement('div');
            dialog.className = 'modal';
            dialog.setAttribute('role', 'dialog');
            dialog.setAttribute('aria-modal', 'true');
            dialog.setAttribute('aria-label', 'Quick view');

            const closeBtn = document.createElement('button');
            closeBtn.className = 'modal__close';
            closeBtn.type = 'button';
            closeBtn.setAttribute('aria-label', 'Close');
            closeBtn.innerHTML = '×';
            closeBtn.addEventListener('click', () => this.close());

            dialog.appendChild(closeBtn);
            overlay.appendChild(navPrev);
            overlay.appendChild(dialog);
            overlay.appendChild(navNext);

            overlay.addEventListener('click', (e) => {
                if (e.target === overlay) this.close(); // click outside to close
            });

            document.body.appendChild(overlay);

            this._overlay = overlay;
            this._dialog = dialog;

            // ESC
            this._escHandler = (e) => {
                if (e.key === 'Escape') this.close();
                if (e.key === 'Tab' && this._focusables.length) {
                    // focus trap
                    if (e.shiftKey && document.activeElement === this._first) {
                        e.preventDefault(); this._last.focus();
                    } else if (!e.shiftKey && document.activeElement === this._last) {
                        e.preventDefault(); this._first.focus();
                    }
                }
            };
        },

        open(contentNode) {
            this.init();

            this._prevActive = document.activeElement;

            // reset content
            this._dialog.querySelectorAll('.modal__body').forEach(n => n.remove());
            const body = document.createElement('div');
            body.className = 'modal__body';
            body.appendChild(contentNode);
            this._dialog.appendChild(body);

            // show
            this._overlay.classList.add('is-open');
            this._overlay.setAttribute('aria-hidden', 'false');
            document.documentElement.classList.add('no-scroll');

            // focusables
            this._focusables = Array.from(this._dialog.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'))
                .filter(el => !el.hasAttribute('disabled'));
            this._first = this._focusables[0] || this._dialog;
            this._last = this._focusables[this._focusables.length - 1] || this._dialog;
            setTimeout(() => (this._first && this._first.focus()), 0);

            document.addEventListener('keydown', this._escHandler);
        },

        close() {
            if (!this._overlay) return;
            this._overlay.classList.remove('is-open');
            this._overlay.setAttribute('aria-hidden', 'true');
            document.documentElement.classList.remove('no-scroll');
            document.removeEventListener('keydown', this._escHandler);
            if (this._prevActive) this._prevActive.focus();
        },

        onNav(handler) {
            // delegate prev/next clicks
            this._overlay?.addEventListener('click', (e) => {
                const btn = e.target.closest('[data-modal-nav]');
                if (!btn) return;
                handler(btn.getAttribute('data-modal-nav'));
            });
        }
    };

    window.Modal = Modal;
})();
