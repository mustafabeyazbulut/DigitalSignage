/**
 * Dinamik Düzen Oluşturucu
 * Canlı önizleme ile interaktif satır/sütun düzen tanımını yönetir.
 */
(function () {
    'use strict';

    let layoutData = { rows: [] };

    const MAX_ROWS = 10;
    const MAX_COLS = 10;

    function init(initialJson) {
        try {
            layoutData = JSON.parse(initialJson);
            if (!layoutData.rows || layoutData.rows.length === 0) {
                layoutData = { rows: [{ height: 100, columns: [{ width: 100 }] }] };
            }
        } catch {
            layoutData = { rows: [{ height: 100, columns: [{ width: 100 }] }] };
        }
        render();
    }

    function addRow() {
        if (layoutData.rows.length >= MAX_ROWS) return;
        // Yükseklikleri eşit dağıt
        var newCount = layoutData.rows.length + 1;
        var eqHeight = Math.floor(100 / newCount);
        layoutData.rows.forEach(function (r) { r.height = eqHeight; });
        var remainder = 100 - (eqHeight * newCount);
        layoutData.rows.push({ height: eqHeight + remainder, columns: [{ width: 100 }] });
        render();
    }

    function removeRow(index) {
        if (layoutData.rows.length <= 1) return;
        layoutData.rows.splice(index, 1);
        distributeHeights();
        render();
    }

    function addColumn(rowIndex) {
        var row = layoutData.rows[rowIndex];
        if (!row || row.columns.length >= MAX_COLS) return;
        var newCount = row.columns.length + 1;
        var eqWidth = Math.floor(100 / newCount);
        row.columns.forEach(function (c) { c.width = eqWidth; });
        var remainder = 100 - (eqWidth * newCount);
        row.columns.push({ width: eqWidth + remainder });
        render();
    }

    function removeColumn(rowIndex, colIndex) {
        var row = layoutData.rows[rowIndex];
        if (!row || row.columns.length <= 1) return;
        row.columns.splice(colIndex, 1);
        distributeWidths(rowIndex);
        render();
    }

    function distributeHeights() {
        var count = layoutData.rows.length;
        if (count === 0) return;
        var eq = Math.floor(100 / count);
        layoutData.rows.forEach(function (r) { r.height = eq; });
        layoutData.rows[count - 1].height = 100 - eq * (count - 1);
    }

    function distributeWidths(rowIndex) {
        var cols = layoutData.rows[rowIndex].columns;
        var count = cols.length;
        if (count === 0) return;
        var eq = Math.floor(100 / count);
        cols.forEach(function (c) { c.width = eq; });
        cols[count - 1].width = 100 - eq * (count - 1);
    }

    function equalDistributeHeights() {
        distributeHeights();
        render();
    }

    function equalDistributeWidths(rowIndex) {
        distributeWidths(rowIndex);
        render();
    }

    function updateHeight(rowIndex, value) {
        var v = parseFloat(value);
        if (isNaN(v) || v < 1) v = 1;
        if (v > 100) v = 100;
        layoutData.rows[rowIndex].height = v;
        updateHiddenField();
        updatePreview();
        updateValidation();
    }

    function updateWidth(rowIndex, colIndex, value) {
        var v = parseFloat(value);
        if (isNaN(v) || v < 1) v = 1;
        if (v > 100) v = 100;
        layoutData.rows[rowIndex].columns[colIndex].width = v;
        updateHiddenField();
        updatePreview();
        updateValidation();
    }

    function updateHiddenField() {
        var field = document.getElementById('LayoutDefinition');
        if (field) {
            field.value = JSON.stringify(layoutData);
        }
    }

    function getTranslation(key) {
        return window.layoutBuilderTranslations && window.layoutBuilderTranslations[key] ? window.layoutBuilderTranslations[key] : key;
    }

    function updateValidation() {
        var totalHeight = 0;
        var allValid = true;

        layoutData.rows.forEach(function (row, ri) {
            totalHeight += row.height;
            var totalWidth = 0;
            row.columns.forEach(function (col) { totalWidth += col.width; });
            var widthBadge = document.getElementById('row-width-badge-' + ri);
            if (widthBadge) {
                var diff = Math.abs(totalWidth - 100);
                if (diff > 0.5) {
                    widthBadge.className = 'badge bg-danger';
                    widthBadge.textContent = totalWidth.toFixed(0) + '%';
                    allValid = false;
                } else {
                    widthBadge.className = 'badge bg-success';
                    widthBadge.textContent = '100%';
                }
            }
        });

        var heightBadge = document.getElementById('total-height-badge');
        if (heightBadge) {
            var diff = Math.abs(totalHeight - 100);
            if (diff > 0.5) {
                heightBadge.className = 'badge bg-danger';
                heightBadge.textContent = totalHeight.toFixed(0) + '%';
                allValid = false;
            } else {
                heightBadge.className = 'badge bg-success';
                heightBadge.textContent = '100%';
            }
        }

        var submitBtn = document.getElementById('layout-submit-btn');
        if (submitBtn) {
            submitBtn.disabled = !allValid;
        }
    }

    function updatePreview() {
        var container = document.getElementById('layout-preview');
        if (!container) return;

        var html = '';
        layoutData.rows.forEach(function (row, ri) {
            var colTemplate = row.columns.map(function (c) { return c.width + 'fr'; }).join(' ');
            html += '<div style="display:grid;grid-template-columns:' + colTemplate + ';gap:4px;height:' + row.height + '%;min-height:30px;">';
            row.columns.forEach(function (col, ci) {
                html += '<div style="background:linear-gradient(135deg,#e3f2fd,#bbdefb);border:1px solid #90caf9;border-radius:4px;display:flex;align-items:center;justify-content:center;font-size:0.75rem;color:#1565c0;font-weight:600;">';
                html += 'R' + (ri + 1) + 'C' + (ci + 1);
                html += '</div>';
            });
            html += '</div>';
        });

        container.innerHTML = html;
    }

    function render() {
        updateHiddenField();
        renderControls();
        updatePreview();
        updateValidation();
    }

    function renderControls() {
        var container = document.getElementById('layout-builder-controls');
        if (!container) return;

        var html = '';

        // Toplam yükseklik göstergesi
        html += '<div class="d-flex align-items-center justify-content-between mb-3">';
        html += '<div>';
        html += '<button type="button" class="btn btn-success btn-sm me-2" onclick="LayoutBuilder.addRow()" ' + (layoutData.rows.length >= MAX_ROWS ? 'disabled' : '') + '>';
        html += '<i class="fas fa-plus me-1"></i>' + getTranslation('addRow');
        html += '</button>';
        html += '<button type="button" class="btn btn-outline-secondary btn-sm" onclick="LayoutBuilder.equalDistributeHeights()">';
        html += '<i class="fas fa-equals me-1"></i>' + getTranslation('equalDistribute');
        html += '</button>';
        html += '</div>';
        html += '<div>' + getTranslation('totalHeight') + ': <span id="total-height-badge" class="badge bg-success">100%</span></div>';
        html += '</div>';

        layoutData.rows.forEach(function (row, ri) {
            html += '<div class="card mb-2 border-start border-primary border-3">';
            html += '<div class="card-body p-2">';

            // Satır başlığı
            html += '<div class="d-flex align-items-center justify-content-between mb-2">';
            html += '<div class="d-flex align-items-center gap-2">';
            html += '<span class="badge bg-primary">' + getTranslation('row') + ' ' + (ri + 1) + '</span>';
            html += '<div class="input-group input-group-sm" style="width:200px;">';
            html += '<span class="input-group-text">' + getTranslation('height') + '</span>';
            html += '<input type="number" class="form-control" style="min-width:60px;" min="1" max="100" value="' + row.height + '" oninput="LayoutBuilder.updateHeight(' + ri + ',this.value)" />';
            html += '<span class="input-group-text">%</span>';
            html += '</div>';
            html += '</div>';

            html += '<div class="d-flex align-items-center gap-2">';
            html += getTranslation('colTotal') + ': <span id="row-width-badge-' + ri + '" class="badge bg-success">100%</span>';
            html += '<button type="button" class="btn btn-outline-primary btn-sm" onclick="LayoutBuilder.addColumn(' + ri + ')" ' + (row.columns.length >= MAX_COLS ? 'disabled' : '') + '>';
            html += '<i class="fas fa-plus me-1"></i>' + getTranslation('addCol');
            html += '</button>';
            html += '<button type="button" class="btn btn-outline-secondary btn-sm" onclick="LayoutBuilder.equalDistributeWidths(' + ri + ')">';
            html += '<i class="fas fa-equals"></i>';
            html += '</button>';
            html += '<button type="button" class="btn btn-outline-danger btn-sm" onclick="LayoutBuilder.removeRow(' + ri + ')" ' + (layoutData.rows.length <= 1 ? 'disabled' : '') + '>';
            html += '<i class="fas fa-trash"></i>';
            html += '</button>';
            html += '</div>';
            html += '</div>';

            // Sütunlar
            html += '<div class="d-flex flex-wrap gap-2">';
            row.columns.forEach(function (col, ci) {
                html += '<div class="d-flex align-items-center gap-1 border rounded px-2 py-1 bg-light">';
                html += '<small class="text-muted fw-bold">C' + (ci + 1) + '</small>';
                html += '<div class="input-group input-group-sm" style="width:100px;">';
                html += '<input type="number" class="form-control" min="1" max="100" value="' + col.width + '" oninput="LayoutBuilder.updateWidth(' + ri + ',' + ci + ',this.value)" />';
                html += '<span class="input-group-text">%</span>';
                html += '</div>';
                if (row.columns.length > 1) {
                    html += '<button type="button" class="btn btn-outline-danger btn-sm py-0 px-1" onclick="LayoutBuilder.removeColumn(' + ri + ',' + ci + ')">';
                    html += '<i class="fas fa-times"></i>';
                    html += '</button>';
                }
                html += '</div>';
            });
            html += '</div>';

            html += '</div></div>';
        });

        container.innerHTML = html;
    }

    // Dışa açık API
    window.LayoutBuilder = {
        init: init,
        addRow: addRow,
        removeRow: removeRow,
        addColumn: addColumn,
        removeColumn: removeColumn,
        updateHeight: updateHeight,
        updateWidth: updateWidth,
        equalDistributeHeights: equalDistributeHeights,
        equalDistributeWidths: equalDistributeWidths,
        getData: function () { return layoutData; }
    };
})();
