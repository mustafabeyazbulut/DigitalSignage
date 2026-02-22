/**
 * Dinamik Düzen Oluşturucu (İç İçe Bölme Destekli)
 * Canlı önizleme ile interaktif satır/sütun düzen tanımını yönetir.
 * Sütunlar iç içe bölünebilir (recursive).
 */
(function () {
    'use strict';

    let layoutData = { rows: [] };

    const MAX_ROWS = 10;
    const MAX_COLS = 10;
    const MAX_DEPTH = 3; // Maksimum iç içe derinlik

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

    // === Satır İşlemleri ===

    function addRow(path) {
        var rows = getRowsByPath(path);
        if (!rows || rows.length >= MAX_ROWS) return;
        var newCount = rows.length + 1;
        var eqHeight = Math.floor(100 / newCount);
        rows.forEach(function (r) { r.height = eqHeight; });
        var remainder = 100 - (eqHeight * newCount);
        rows.push({ height: eqHeight + remainder, columns: [{ width: 100 }] });
        render();
    }

    function removeRow(path, index) {
        var rows = getRowsByPath(path);
        if (!rows || rows.length <= 1) return;
        rows.splice(index, 1);
        distributeHeightsForRows(rows);
        render();
    }

    // === Sütun İşlemleri ===

    function addColumn(path, rowIndex) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        var row = rows[rowIndex];
        if (!row || row.columns.length >= MAX_COLS) return;
        var newCount = row.columns.length + 1;
        var eqWidth = Math.floor(100 / newCount);
        row.columns.forEach(function (c) { c.width = eqWidth; });
        var remainder = 100 - (eqWidth * newCount);
        row.columns.push({ width: eqWidth + remainder });
        render();
    }

    function removeColumn(path, rowIndex, colIndex) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        var row = rows[rowIndex];
        if (!row || row.columns.length <= 1) return;
        row.columns.splice(colIndex, 1);
        distributeWidthsForRow(row);
        render();
    }

    // === Bölme İşlemleri ===

    function splitCell(path, rowIndex, colIndex) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        var col = rows[rowIndex].columns[colIndex];
        if (col.rows) return; // Zaten bölünmüş
        // Varsayılan: 2 satır, 1 sütun
        col.rows = [
            { height: 50, columns: [{ width: 100 }] },
            { height: 50, columns: [{ width: 100 }] }
        ];
        render();
    }

    function unsplitCell(path, rowIndex, colIndex) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        var col = rows[rowIndex].columns[colIndex];
        if (!col.rows) return;
        delete col.rows;
        render();
    }

    // === Dağıtım ===

    function distributeHeightsForRows(rows) {
        var count = rows.length;
        if (count === 0) return;
        var eq = Math.floor(100 / count);
        rows.forEach(function (r) { r.height = eq; });
        rows[count - 1].height = 100 - eq * (count - 1);
    }

    function distributeWidthsForRow(row) {
        var cols = row.columns;
        var count = cols.length;
        if (count === 0) return;
        var eq = Math.floor(100 / count);
        cols.forEach(function (c) { c.width = eq; });
        cols[count - 1].width = 100 - eq * (count - 1);
    }

    function equalDistributeHeights(path) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        distributeHeightsForRows(rows);
        render();
    }

    function equalDistributeWidths(path, rowIndex) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        distributeWidthsForRow(rows[rowIndex]);
        render();
    }

    // === Güncelleme ===

    function updateHeight(path, rowIndex, value) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        var v = parseFloat(value);
        if (isNaN(v) || v < 1) v = 1;
        if (v > 100) v = 100;
        rows[rowIndex].height = v;
        updateHiddenField();
        updatePreview();
        updateValidation();
    }

    function updateWidth(path, rowIndex, colIndex, value) {
        var rows = getRowsByPath(path);
        if (!rows) return;
        var v = parseFloat(value);
        if (isNaN(v) || v < 1) v = 1;
        if (v > 100) v = 100;
        rows[rowIndex].columns[colIndex].width = v;
        updateHiddenField();
        updatePreview();
        updateValidation();
    }

    // === Yol (Path) Yönetimi ===
    // Path formatı: "" (root), "0.1" (row 0 > col 1'in alt satırları) vb.

    function getRowsByPath(path) {
        if (!path || path === '') return layoutData.rows;
        var parts = path.split('.');
        var current = layoutData.rows;
        for (var i = 0; i < parts.length; i += 2) {
            var ri = parseInt(parts[i]);
            var ci = parseInt(parts[i + 1]);
            if (!current || !current[ri] || !current[ri].columns || !current[ri].columns[ci]) return null;
            current = current[ri].columns[ci].rows;
        }
        return current;
    }

    function getDepth(path) {
        if (!path || path === '') return 0;
        return Math.floor(path.split('.').length / 2);
    }

    // === Hidden Field ===

    function updateHiddenField() {
        var field = document.getElementById('LayoutDefinition');
        if (field) {
            field.value = JSON.stringify(layoutData);
        }
    }

    // === Çeviriler ===

    function getTranslation(key) {
        return window.layoutBuilderTranslations && window.layoutBuilderTranslations[key] ? window.layoutBuilderTranslations[key] : key;
    }

    // === Validation (Recursive) ===

    function updateValidation() {
        var allValid = validateRows(layoutData.rows, '');

        var submitBtn = document.getElementById('layout-submit-btn');
        if (submitBtn) {
            submitBtn.disabled = !allValid;
        }
    }

    function validateRows(rows, pathPrefix) {
        var totalHeight = 0;
        var allValid = true;

        rows.forEach(function (row, ri) {
            totalHeight += row.height;
            var totalWidth = 0;
            row.columns.forEach(function (col, ci) {
                totalWidth += col.width;
                // İç içe satırları da doğrula
                if (col.rows && col.rows.length > 0) {
                    var childPath = pathPrefix ? pathPrefix + '.' + ri + '.' + ci : ri + '.' + ci;
                    if (!validateRows(col.rows, childPath)) {
                        allValid = false;
                    }
                }
            });

            var widthBadge = document.getElementById('row-width-badge-' + pathPrefix + ri);
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

        var heightBadge = document.getElementById('total-height-badge-' + pathPrefix);
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

        return allValid;
    }

    // === Preview (Recursive) ===

    function updatePreview() {
        var container = document.getElementById('layout-preview');
        if (!container) return;
        container.innerHTML = renderPreviewRows(layoutData.rows, '');
    }

    function renderPreviewRows(rows, parentPos) {
        var html = '';
        rows.forEach(function (row, ri) {
            var colTemplate = row.columns.map(function (c) { return c.width + 'fr'; }).join(' ');
            html += '<div style="display:grid;grid-template-columns:' + colTemplate + ';gap:4px;height:' + row.height + '%;min-height:20px;">';
            row.columns.forEach(function (col, ci) {
                var pos = parentPos ? parentPos + '.' + 'R' + (ri + 1) + 'C' + (ci + 1) : 'R' + (ri + 1) + 'C' + (ci + 1);
                if (col.rows && col.rows.length > 0) {
                    // İç içe sütun — alt yapıyı render et
                    html += '<div style="display:flex;flex-direction:column;gap:3px;border:2px dashed #ff9800;border-radius:4px;padding:3px;background:rgba(255,152,0,0.05);">';
                    html += renderPreviewRows(col.rows, pos);
                    html += '</div>';
                } else {
                    html += '<div style="background:linear-gradient(135deg,#e3f2fd,#bbdefb);border:1px solid #90caf9;border-radius:4px;display:flex;align-items:center;justify-content:center;font-size:0.65rem;color:#1565c0;font-weight:600;">';
                    html += pos;
                    html += '</div>';
                }
            });
            html += '</div>';
        });
        return html;
    }

    // === Controls Render (Recursive) ===

    function render() {
        updateHiddenField();
        renderControls();
        updatePreview();
        updateValidation();
    }

    function renderControls() {
        var container = document.getElementById('layout-builder-controls');
        if (!container) return;
        container.innerHTML = renderRowControls(layoutData.rows, '', 0);
    }

    function renderRowControls(rows, path, depth) {
        var html = '';
        var depthColor = depth === 0 ? 'primary' : (depth === 1 ? 'warning' : 'info');
        var pathStr = escapeAttr(path);

        // Toplam yükseklik göstergesi + satır ekle butonu
        html += '<div class="d-flex align-items-center justify-content-between mb-2">';
        html += '<div>';
        html += '<button type="button" class="btn btn-success btn-sm me-2" onclick="LayoutBuilder.addRow(\'' + pathStr + '\')" ' + (rows.length >= MAX_ROWS ? 'disabled' : '') + '>';
        html += '<i class="fas fa-plus me-1"></i>' + getTranslation('addRow');
        html += '</button>';
        html += '<button type="button" class="btn btn-outline-secondary btn-sm" onclick="LayoutBuilder.equalDistributeHeights(\'' + pathStr + '\')">';
        html += '<i class="fas fa-equals me-1"></i>' + getTranslation('equalDistribute');
        html += '</button>';
        html += '</div>';
        html += '<div>' + getTranslation('totalHeight') + ': <span id="total-height-badge-' + pathStr + '" class="badge bg-success">100%</span></div>';
        html += '</div>';

        rows.forEach(function (row, ri) {
            html += '<div class="card mb-2 border-start border-' + depthColor + ' border-3">';
            html += '<div class="card-body p-2">';

            // Satır başlığı
            html += '<div class="d-flex align-items-center justify-content-between mb-2">';
            html += '<div class="d-flex align-items-center gap-2">';
            html += '<span class="badge bg-' + depthColor + '">' + getTranslation('row') + ' ' + (ri + 1);
            if (depth > 0) html += ' <small>(' + getTranslation('nested') + ')</small>';
            html += '</span>';
            html += '<div class="input-group input-group-sm" style="width:200px;">';
            html += '<span class="input-group-text">' + getTranslation('height') + '</span>';
            html += '<input type="number" class="form-control" style="min-width:60px;" min="1" max="100" value="' + row.height + '" oninput="LayoutBuilder.updateHeight(\'' + pathStr + '\',' + ri + ',this.value)" />';
            html += '<span class="input-group-text">%</span>';
            html += '</div>';
            html += '</div>';

            html += '<div class="d-flex align-items-center gap-2">';
            html += getTranslation('colTotal') + ': <span id="row-width-badge-' + pathStr + ri + '" class="badge bg-success">100%</span>';
            html += '<button type="button" class="btn btn-outline-primary btn-sm" onclick="LayoutBuilder.addColumn(\'' + pathStr + '\',' + ri + ')" ' + (row.columns.length >= MAX_COLS ? 'disabled' : '') + '>';
            html += '<i class="fas fa-plus me-1"></i>' + getTranslation('addCol');
            html += '</button>';
            html += '<button type="button" class="btn btn-outline-secondary btn-sm" onclick="LayoutBuilder.equalDistributeWidths(\'' + pathStr + '\',' + ri + ')">';
            html += '<i class="fas fa-equals"></i>';
            html += '</button>';
            html += '<button type="button" class="btn btn-outline-danger btn-sm" onclick="LayoutBuilder.removeRow(\'' + pathStr + '\',' + ri + ')" ' + (rows.length <= 1 ? 'disabled' : '') + '>';
            html += '<i class="fas fa-trash"></i>';
            html += '</button>';
            html += '</div>';
            html += '</div>';

            // Sütunlar
            html += '<div class="d-flex flex-wrap gap-2">';
            row.columns.forEach(function (col, ci) {
                var hasSplit = col.rows && col.rows.length > 0;
                var cellBg = hasSplit ? 'bg-warning bg-opacity-10' : 'bg-light';
                html += '<div class="d-flex align-items-center gap-1 border rounded px-2 py-1 ' + cellBg + '">';
                html += '<small class="text-muted fw-bold">C' + (ci + 1) + '</small>';
                html += '<div class="input-group input-group-sm" style="width:100px;">';
                html += '<input type="number" class="form-control" min="1" max="100" value="' + col.width + '" oninput="LayoutBuilder.updateWidth(\'' + pathStr + '\',' + ri + ',' + ci + ',this.value)" />';
                html += '<span class="input-group-text">%</span>';
                html += '</div>';

                // Böl/Kaldır butonları
                if (!hasSplit && depth < MAX_DEPTH) {
                    html += '<button type="button" class="btn btn-outline-warning btn-sm py-0 px-1" onclick="LayoutBuilder.splitCell(\'' + pathStr + '\',' + ri + ',' + ci + ')" title="' + getTranslation('splitCell') + '">';
                    html += '<i class="fas fa-th-large"></i>';
                    html += '</button>';
                }
                if (hasSplit) {
                    html += '<button type="button" class="btn btn-outline-danger btn-sm py-0 px-1" onclick="LayoutBuilder.unsplitCell(\'' + pathStr + '\',' + ri + ',' + ci + ')" title="' + getTranslation('unsplitCell') + '">';
                    html += '<i class="fas fa-compress-alt"></i>';
                    html += '</button>';
                }

                if (row.columns.length > 1) {
                    html += '<button type="button" class="btn btn-outline-danger btn-sm py-0 px-1" onclick="LayoutBuilder.removeColumn(\'' + pathStr + '\',' + ri + ',' + ci + ')">';
                    html += '<i class="fas fa-times"></i>';
                    html += '</button>';
                }
                html += '</div>';
            });
            html += '</div>';

            // İç içe bölünmüş sütunların alt kontrolleri
            row.columns.forEach(function (col, ci) {
                if (col.rows && col.rows.length > 0) {
                    var childPath = path ? path + '.' + ri + '.' + ci : ri + '.' + ci;
                    html += '<div class="ms-4 mt-2 ps-3" style="border-left: 3px dashed #ff9800;">';
                    html += '<small class="text-warning fw-bold d-block mb-1"><i class="fas fa-layer-group me-1"></i>C' + (ci + 1) + ' — ' + getTranslation('nested') + '</small>';
                    html += renderRowControls(col.rows, childPath, depth + 1);
                    html += '</div>';
                }
            });

            html += '</div></div>';
        });

        return html;
    }

    function escapeAttr(str) {
        return str.replace(/'/g, "\\'").replace(/"/g, '&quot;');
    }

    // Dışa açık API
    window.LayoutBuilder = {
        init: init,
        addRow: addRow,
        removeRow: removeRow,
        addColumn: addColumn,
        removeColumn: removeColumn,
        splitCell: splitCell,
        unsplitCell: unsplitCell,
        updateHeight: updateHeight,
        updateWidth: updateWidth,
        equalDistributeHeights: equalDistributeHeights,
        equalDistributeWidths: equalDistributeWidths,
        getData: function () { return layoutData; }
    };
})();
