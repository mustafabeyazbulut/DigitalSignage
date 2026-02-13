// Advanced Table Features - Sorting, Search, Pagination, Bulk Actions
// Tablo sıralama, arama, sayfalama ve toplu işlemler

const TableFeatures = {
    tables: new Map(), // Her tablo için state

    // Tabloyu başlat
    init: function(tableId, options = {}) {
        const table = document.querySelector(tableId);
        if (!table) return;

        const state = {
            table: table,
            tbody: table.querySelector('tbody'),
            rows: Array.from(table.querySelectorAll('tbody tr')),
            originalRows: Array.from(table.querySelectorAll('tbody tr')),
            currentPage: 1,
            pageSize: options.pageSize || 10,
            sortColumn: null,
            sortDirection: 'asc',
            searchTerm: '',
            selectedRows: new Set(),
            filters: new Map()
        };

        this.tables.set(tableId, state);

        // Özellikleri aktifleştir
        this.initSorting(tableId);
        this.initSearch(tableId);
        this.initBulkActions(tableId);
        this.initPagination(tableId);

        // İlk render
        this.render(tableId);
    },

    // Sıralama özelliğini aktifleştir
    initSorting: function(tableId) {
        const state = this.tables.get(tableId);
        const headers = state.table.querySelectorAll('thead th.sortable');

        headers.forEach((header, index) => {
            // Sort icon ekle
            const icon = document.createElement('i');
            icon.className = 'bi bi-arrow-down sort-icon';
            header.appendChild(icon);
            header.style.position = 'relative';

            header.addEventListener('click', () => {
                this.sort(tableId, index);
            });
        });
    },

    // Sıralama yap
    sort: function(tableId, columnIndex) {
        const state = this.tables.get(tableId);

        // Sıralama yönünü belirle
        if (state.sortColumn === columnIndex) {
            state.sortDirection = state.sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            state.sortColumn = columnIndex;
            state.sortDirection = 'asc';
        }

        // Satırları sırala
        state.rows.sort((a, b) => {
            const aValue = a.cells[columnIndex]?.textContent.trim() || '';
            const bValue = b.cells[columnIndex]?.textContent.trim() || '';

            // Sayı mı kontrol et
            const aNum = parseFloat(aValue.replace(/[^0-9.-]/g, ''));
            const bNum = parseFloat(bValue.replace(/[^0-9.-]/g, ''));

            if (!isNaN(aNum) && !isNaN(bNum)) {
                return state.sortDirection === 'asc' ? aNum - bNum : bNum - aNum;
            }

            // String karşılaştırma
            return state.sortDirection === 'asc'
                ? aValue.localeCompare(bValue, 'tr')
                : bValue.localeCompare(aValue, 'tr');
        });

        // Header'ları güncelle
        state.table.querySelectorAll('thead th.sortable').forEach((th, idx) => {
            th.classList.remove('asc', 'desc');
            if (idx === columnIndex) {
                th.classList.add(state.sortDirection);
            }
        });

        // İlk sayfaya dön
        state.currentPage = 1;
        this.render(tableId);
    },

    // Arama özelliğini aktifleştir
    initSearch: function(tableId) {
        const state = this.tables.get(tableId);
        const searchInput = document.querySelector(`[data-table-search="${tableId}"]`);

        if (!searchInput) return;

        searchInput.addEventListener('input', (e) => {
            state.searchTerm = e.target.value.toLowerCase();
            state.currentPage = 1; // İlk sayfaya dön
            this.applyFilters(tableId);
            this.render(tableId);
        });
    },

    // Filtreleri uygula
    applyFilters: function(tableId) {
        const state = this.tables.get(tableId);

        state.rows = state.originalRows.filter(row => {
            // Arama filtresi
            if (state.searchTerm) {
                const text = row.textContent.toLowerCase();
                if (!text.includes(state.searchTerm)) {
                    return false;
                }
            }

            // Diğer filtreler
            for (const [key, value] of state.filters) {
                const cell = row.querySelector(`[data-filter="${key}"]`);
                if (cell && cell.textContent.trim() !== value) {
                    return false;
                }
            }

            return true;
        });
    },

    // Toplu işlemler özelliğini aktifleştir
    initBulkActions: function(tableId) {
        const state = this.tables.get(tableId);
        const selectAllCheckbox = document.querySelector(`[data-select-all="${tableId}"]`);
        const bulkBar = document.querySelector(`[data-bulk-bar="${tableId}"]`);

        if (!selectAllCheckbox) return;

        // Tümünü seç checkbox
        selectAllCheckbox.addEventListener('change', (e) => {
            const checkboxes = state.table.querySelectorAll('tbody .table-checkbox');
            checkboxes.forEach(cb => {
                cb.checked = e.target.checked;
                const row = cb.closest('tr');
                if (e.target.checked) {
                    state.selectedRows.add(row);
                    row.classList.add('selected');
                } else {
                    state.selectedRows.delete(row);
                    row.classList.remove('selected');
                }
            });
            this.updateBulkBar(tableId);
        });

        // Satır checkbox'ları
        state.originalRows.forEach(row => {
            const checkbox = row.querySelector('.table-checkbox');
            if (checkbox) {
                checkbox.addEventListener('change', (e) => {
                    if (e.target.checked) {
                        state.selectedRows.add(row);
                        row.classList.add('selected');
                    } else {
                        state.selectedRows.delete(row);
                        row.classList.remove('selected');
                        selectAllCheckbox.checked = false;
                    }
                    this.updateBulkBar(tableId);
                });
            }
        });
    },

    // Bulk action bar'ı güncelle
    updateBulkBar: function(tableId) {
        const state = this.tables.get(tableId);
        const bulkBar = document.querySelector(`[data-bulk-bar="${tableId}"]`);
        const infoText = bulkBar?.querySelector('.bulk-actions-info');

        if (!bulkBar) return;

        if (state.selectedRows.size > 0) {
            bulkBar.classList.add('show');
            if (infoText) {
                infoText.textContent = `${state.selectedRows.size} öğe seçildi`;
            }
        } else {
            bulkBar.classList.remove('show');
        }
    },

    // Seçili satırları al
    getSelectedRows: function(tableId) {
        const state = this.tables.get(tableId);
        return Array.from(state.selectedRows);
    },

    // Seçimi temizle
    clearSelection: function(tableId) {
        const state = this.tables.get(tableId);
        const selectAllCheckbox = document.querySelector(`[data-select-all="${tableId}"]`);

        state.selectedRows.forEach(row => {
            const checkbox = row.querySelector('.table-checkbox');
            if (checkbox) checkbox.checked = false;
            row.classList.remove('selected');
        });

        state.selectedRows.clear();
        if (selectAllCheckbox) selectAllCheckbox.checked = false;
        this.updateBulkBar(tableId);
    },

    // Sayfalama özelliğini aktifleştir
    initPagination: function(tableId) {
        const state = this.tables.get(tableId);
        const paginationContainer = document.querySelector(`[data-pagination="${tableId}"]`);

        if (!paginationContainer) return;

        // Event delegation ile sayfa butonları
        paginationContainer.addEventListener('click', (e) => {
            const btn = e.target.closest('.pagination-btn');
            if (!btn || btn.disabled) return;

            const page = parseInt(btn.dataset.page);
            if (!isNaN(page)) {
                state.currentPage = page;
                this.render(tableId);
            }
        });
    },

    // Sayfaları render et
    renderPagination: function(tableId) {
        const state = this.tables.get(tableId);
        const container = document.querySelector(`[data-pagination="${tableId}"]`);

        if (!container) return;

        const totalPages = Math.ceil(state.rows.length / state.pageSize);
        const info = container.querySelector('.pagination-info');
        const controls = container.querySelector('.pagination-controls');

        // Info güncelle
        if (info) {
            const start = (state.currentPage - 1) * state.pageSize + 1;
            const end = Math.min(state.currentPage * state.pageSize, state.rows.length);
            info.textContent = `${start}-${end} / ${state.rows.length} öğe gösteriliyor`;
        }

        // Kontroller oluştur
        if (controls) {
            let html = '';

            // Previous
            html += `<button class="pagination-btn" data-page="${state.currentPage - 1}" ${state.currentPage === 1 ? 'disabled' : ''}>
                <i class="bi bi-chevron-left"></i>
            </button>`;

            // Sayfa numaraları (max 5 göster)
            const maxButtons = 5;
            let startPage = Math.max(1, state.currentPage - Math.floor(maxButtons / 2));
            let endPage = Math.min(totalPages, startPage + maxButtons - 1);

            if (endPage - startPage < maxButtons - 1) {
                startPage = Math.max(1, endPage - maxButtons + 1);
            }

            for (let i = startPage; i <= endPage; i++) {
                html += `<button class="pagination-btn ${i === state.currentPage ? 'active' : ''}" data-page="${i}">
                    ${i}
                </button>`;
            }

            // Next
            html += `<button class="pagination-btn" data-page="${state.currentPage + 1}" ${state.currentPage === totalPages ? 'disabled' : ''}>
                <i class="bi bi-chevron-right"></i>
            </button>`;

            controls.innerHTML = html;
        }
    },

    // Tabloyu render et
    render: function(tableId) {
        const state = this.tables.get(tableId);

        // Sayfalamayı hesapla
        const start = (state.currentPage - 1) * state.pageSize;
        const end = start + state.pageSize;
        const pageRows = state.rows.slice(start, end);

        // Tbody'yi temizle ve render et
        state.tbody.innerHTML = '';
        pageRows.forEach(row => {
            state.tbody.appendChild(row.cloneNode(true));
        });

        // Pagination'ı güncelle
        this.renderPagination(tableId);

        // Checkbox event'lerini yeniden bağla
        this.rebindCheckboxes(tableId);
    },

    // Checkbox event'lerini yeniden bağla
    rebindCheckboxes: function(tableId) {
        const state = this.tables.get(tableId);
        const selectAllCheckbox = document.querySelector(`[data-select-all="${tableId}"]`);

        state.tbody.querySelectorAll('.table-checkbox').forEach(checkbox => {
            const row = checkbox.closest('tr');
            const originalRow = state.originalRows.find(r =>
                r.querySelector('[data-row-id]')?.dataset.rowId === row.querySelector('[data-row-id]')?.dataset.rowId
            );

            if (originalRow && state.selectedRows.has(originalRow)) {
                checkbox.checked = true;
                row.classList.add('selected');
            }

            checkbox.addEventListener('change', (e) => {
                if (e.target.checked) {
                    state.selectedRows.add(originalRow);
                    row.classList.add('selected');
                } else {
                    state.selectedRows.delete(originalRow);
                    row.classList.remove('selected');
                    if (selectAllCheckbox) selectAllCheckbox.checked = false;
                }
                this.updateBulkBar(tableId);
            });
        });
    }
};

// Export for global access
window.TableFeatures = TableFeatures;
