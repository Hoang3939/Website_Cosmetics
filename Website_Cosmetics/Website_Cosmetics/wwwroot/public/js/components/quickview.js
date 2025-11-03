// QuickView (desc + optional details link)
; (() => {
    const $ = (s, ctx = document) => ctx.querySelector(s);
    const $$ = (s, ctx = document) => Array.from(ctx.querySelectorAll(s));
    const slugify = (t) => (t || '').toLowerCase()
        .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
        .replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '');

    function readProductFromCard(card) {
        const img = $('img', $('.product__img', card));
        const brand = $('.product__brand', card)?.textContent?.trim() || '';
        const title = $('.product__title', card)?.textContent?.trim() || '';
        const priceNow = $('.price--now', card)?.textContent?.trim() || '';
        const priceOld = $('.price--old', card)?.textContent?.trim() || '';
        const ratingScore = $('.rating__score', card)?.textContent?.trim() || '';
        const ratingCount = $('.rating__count', card)?.textContent?.trim() || '';
        const variantsAttr = card.getAttribute('data-variants');
        const variants = variantsAttr ? variantsAttr.split(';').map(s => s.trim()).filter(Boolean) : null;

        // NEW: short description + details url (optional)
        const desc = card.getAttribute('data-desc') || $('.product__desc', card)?.textContent?.trim() || '';
        // prefer data-details, fallback to any anchor with href inside actions
        const detailsUrl =
            card.getAttribute('data-details') ||
            $('.product__actions a[href]', card)?.getAttribute('href') ||
            '';

        const id = card.getAttribute('data-id') || `${slugify(title)}-${Math.random().toString(36).slice(2, 7)}`;

        return {
            id, title, brand, image: img?.getAttribute('src') || '', priceNow, priceOld,
            ratingScore, ratingCount, variants, desc, detailsUrl
        };
    }

    function skeletonNode() {
        const wrap = document.createElement('div');
        wrap.className = 'qv qv--skeleton';
        wrap.innerHTML = `
      <div class="qv__media skeleton-block"></div>
      <div class="qv__info">
        <div class="skeleton-line w-60"></div>
        <div class="skeleton-line w-40"></div>
        <div class="skeleton-line w-80"></div>
        <div class="skeleton-line w-50"></div>
        <div class="skeleton-line w-30"></div>
      </div>`;
        return wrap;
    }

    function contentNode(p) {
        const wrap = document.createElement('div');
        wrap.className = 'qv';

        const variantsHtml = p.variants?.length
            ? `<div class="qv__row">
           <div class="qv__label">Variant</div>
           <div class="qv__variants">
             ${p.variants.map((v, i) => `<button type="button" class="chip ${i === 0 ? 'is-active' : ''}" data-variant="${v}">${v}</button>`).join('')}
           </div>
         </div>` : '';

        const descHtml = p.desc ? `<div class="qv__desc">${p.desc}</div>` : '';
        const detailsHtml = p.detailsUrl ? `<a class="qv__details" href="${p.detailsUrl}">See product details</a>` : '';

        wrap.innerHTML = `
      <div class="qv__media"><img src="${p.image}" alt="${p.title}"></div>
      <div class="qv__info">
        <div class="qv__brand">${p.brand}</div>
        <h2 class="qv__title">${p.title}</h2>

        <div class="qv__rating">
          <img class="qv__star-icon" src="/public/images/icons/star.png" alt="" aria-hidden="true" />
          <span class="qv__score">${p.ratingScore || ''}</span>
          <span class="qv__count">${p.ratingCount || ''}</span>
        </div>

        <div class="qv__price">
          <span class="qv__price-now">${p.priceNow}</span>
          ${p.priceOld ? `<span class="qv__price-old">${p.priceOld}</span>` : ''}
        </div>

        ${descHtml}
        ${detailsHtml}

        <div class="qv__quantity">
          <label class="qv__quantity-label">Quantity</label>
          <div class="qv__qty">
            <button type="button" class="qv__qty-btn qv__qty-btn--minus" data-qty="-1" aria-label="Decrease quantity">-</button>
            <input type="number" class="qv__qty-input" min="1" value="1" inputmode="numeric" pattern="[0-9]*" />
            <button type="button" class="qv__qty-btn qv__qty-btn--plus" data-qty="1" aria-label="Increase quantity">+</button>
          </div>
        </div>

        ${variantsHtml}

        <div class="qv__actions">
          <button type="button" class="btn btn--primary qv__add">Add to cart</button>
          <button type="button" class="btn btn--secondary qv__wish" aria-label="Add to wishlist">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
            </svg>
          </button>
        </div>
      </div>`;

        // qty
        const qtyInput = $('.qv__qty-input', wrap);
        $$('.qv__qty-btn', wrap).forEach(btn => {
            btn.addEventListener('click', () => {
                const delta = parseInt(btn.getAttribute('data-qty'), 10);
                const val = Math.max(1, parseInt(qtyInput.value || '1', 10) + delta);
                qtyInput.value = String(val);
            });
        });

        // variants
        const chips = $$('.chip', wrap);
        if (chips.length) {
            chips.forEach(ch => ch.addEventListener('click', () => {
                chips.forEach(c => c.classList.remove('is-active'));
                ch.classList.add('is-active');
            }));
        }

        // actions
        $('.qv__add', wrap).addEventListener('click', async () => {
            const variant = $('.chip.is-active', wrap)?.getAttribute('data-variant') || null;
            const qty = Math.max(1, parseInt(qtyInput.value || '1', 10));
            try { await QuickView._addToCart(p, { qty, variant }); Toast.success('Added to cart'); }
            catch { Toast.error('Add to cart failed'); }
        });
        $('.qv__wish', wrap).addEventListener('click', () => Toast.success('Saved to your list'));

        return wrap;
    }

    const QuickView = {
        _cards: [], _indexById: new Map(), _currentIndex: -1,
        init() {
            this._cards = $$('.card.product'); this._indexById.clear();
            this._cards.forEach((card, idx) => {
                if (!card.getAttribute('data-id')) {
                    const title = $('.product__title', card)?.textContent || `p-${idx}`;
                    card.setAttribute('data-id', slugify(title) + '-' + idx);
                }
                this._indexById.set(card.getAttribute('data-id'), idx);
            });

            // Handle Quick view button clicks
            document.addEventListener('click', (e) => {
                const trigger = e.target.closest('[data-quickview]');
                if (!trigger) return;
                
                // Prevent default link behavior and stop propagation immediately
                e.preventDefault();
                e.stopPropagation();
                
                const card = e.target.closest('.card.product');
                if (!card) return;
                
                this.openByCard(card);
            }, true);

            // Handle card clicks (navigate to details page)
            document.addEventListener('click', (e) => {
                // Ignore if clicking on Quick view button or any button
                if (e.target.closest('[data-quickview]') || e.target.closest('button')) return;
                
                const card = e.target.closest('.card.product');
                if (!card) return;
                
                // Navigate to details page
                const detailsUrl = card.getAttribute('data-details');
                if (detailsUrl) {
                    window.location.href = detailsUrl;
                }
            });

            Modal.onNav((dir) => {
                if (this._currentIndex < 0) return;
                const next = dir === 'next' ? this._currentIndex + 1 : this._currentIndex - 1;
                this.openByIndex(next);
            });
        },
        openByCard(card) { const idx = this._cards.indexOf(card); if (idx > -1) this.openByIndex(idx); },
        openByIndex(idx) {
            if (idx < 0 || idx >= this._cards.length) return;
            this._currentIndex = idx;
            const card = this._cards[idx];
            const skel = skeletonNode(); Modal.open(skel);
            setTimeout(() => {
                const product = readProductFromCard(card);
                const node = contentNode(product); skel.replaceWith(node);
            }, 200);
        },
        async _addToCart() { return new Promise(res => setTimeout(res, 150)); }
    };

    window.QuickView = QuickView;
})();
