/**
 * Dinamik Düzen Oluşturucu — Profesyonel Versiyon
 * Sol: Kontrol paneli (şablonlar + satır/sütun kontrolleri)
 * Sağ: Canlı önizleme
 * İç içe yapılar açılır-kapanır panellerle yönetilir.
 */
(function () {
    'use strict';

    let layoutData = { rows: [] };
    // Hangi nested panellerin açık olduğunu takip et
    let openPanels = {};

    const MAX_ROWS = 10;
    const MAX_COLS = 10;
    const MAX_DEPTH = 3;

    // === Hazır Şablonlar ===
    const TEMPLATES = [
        { key: 'full', data: { rows: [{ height: 100, columns: [{ width: 100 }] }] } },
        { key: 'twoCols', data: { rows: [{ height: 100, columns: [{ width: 50 }, { width: 50 }] }] } },
        { key: 'twoRows', data: { rows: [{ height: 50, columns: [{ width: 100 }] }, { height: 50, columns: [{ width: 100 }] }] } },
        { key: 'grid2x2', data: { rows: [{ height: 50, columns: [{ width: 50 }, { width: 50 }] }, { height: 50, columns: [{ width: 50 }, { width: 50 }] }] } },
        { key: 'headerContent', data: { rows: [{ height: 25, columns: [{ width: 100 }] }, { height: 75, columns: [{ width: 50 }, { width: 50 }] }] } },
        { key: 'sidebar', data: { rows: [{ height: 100, columns: [{ width: 25 }, { width: 75 }] }] } },
        { key: 'threeCols', data: { rows: [{ height: 100, columns: [{ width: 33 }, { width: 34 }, { width: 33 }] }] } },
        { key: 'threeRows', data: { rows: [{ height: 33, columns: [{ width: 100 }] }, { height: 34, columns: [{ width: 100 }] }, { height: 33, columns: [{ width: 100 }] }] } }
    ];

    // === Başlatma ===
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

    // === Şablon Uygulama ===
    function applyTemplate(index) {
        if (index < 0 || index >= TEMPLATES.length) return;
        layoutData = JSON.parse(JSON.stringify(TEMPLATES[index].data));
        openPanels = {};
        render();
    }

    // === Satır İşlemleri ===
    function addRow(path) {
        var rows = getRowsByPath(path);
        if (!rows || rows.length >= MAX_ROWS) return;
        var newCount = rows.length + 1;
        var eqHeight = Math.floor(100 / newCount);
        rows.forEach(function (r) { r.height = eqHeight; });
        rows.push({ height: 100 - eqHeight * (newCount - 1), columns: [{ width: 100 }] });
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
        row.columns.push({ width: 100 - eqWidth * (newCount - 1) });
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
        if (col.rows) return;
        col.rows = [
            { height: 50, columns: [{ width: 100 }] },
            { height: 50, columns: [{ width: 100 }] }
        ];
        // Yeni oluşan paneli otomatik aç
        var panelKey = (path ? path + '.' : '') + rowIndex + '.' + colIndex;
        openPanels[panelKey] = true;
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

    // === Collapse Toggle ===
    function toggleNested(childPath) {
        openPanels[childPath] = !openPanels[childPath];
        // DOM ID'lerde nokta yerine tire kullanılıyor
        var domId = makeId(childPath);
        var body = document.getElementById('lb-nbody-' + domId);
        var arrow = document.getElementById('lb-narrow-' + domId);
        if (body) body.classList.toggle('open');
        if (arrow) arrow.classList.toggle('open');
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

    // === Güncelleme (re-render olmadan) ===
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

    function makeId(path) {
        if (!path || path === '') return 'root';
        return path.replace(/\./g, '-');
    }

    // === Hidden Field ===
    function updateHiddenField() {
        var field = document.getElementById('LayoutDefinition');
        if (field) field.value = JSON.stringify(layoutData);
    }

    // === Çeviriler ===
    function T(key) {
        return window.layoutBuilderTranslations && window.layoutBuilderTranslations[key]
            ? window.layoutBuilderTranslations[key] : key;
    }

    // === Validation ===
    function updateValidation() {
        var allValid = validateRows(layoutData.rows, 'root');
        var submitBtn = document.getElementById('layout-submit-btn');
        if (submitBtn) submitBtn.disabled = !allValid;
    }

    function validateRows(rows, idPrefix) {
        var totalHeight = 0;
        var allValid = true;

        rows.forEach(function (row, ri) {
            totalHeight += row.height;
            var totalWidth = 0;
            row.columns.forEach(function (col, ci) {
                totalWidth += col.width;
                if (col.rows && col.rows.length > 0) {
                    var childId = idPrefix + '-' + ri + '-' + ci;
                    if (!validateRows(col.rows, childId)) allValid = false;
                }
            });

            var wb = document.getElementById('lb-wb-' + idPrefix + '-' + ri);
            if (wb) {
                if (Math.abs(totalWidth - 100) > 0.5) {
                    wb.className = 'lb-badge invalid';
                    wb.textContent = totalWidth.toFixed(0) + '%';
                    allValid = false;
                } else {
                    wb.className = 'lb-badge valid';
                    wb.textContent = '100%';
                }
            }
        });

        var hb = document.getElementById('lb-hb-' + idPrefix);
        if (hb) {
            if (Math.abs(totalHeight - 100) > 0.5) {
                hb.className = 'lb-badge invalid';
                hb.textContent = totalHeight.toFixed(0) + '%';
                allValid = false;
            } else {
                hb.className = 'lb-badge valid';
                hb.textContent = '100%';
            }
        }

        return allValid;
    }

    // === Preview ===
    function updatePreview() {
        var container = document.getElementById('layout-preview');
        if (!container) return;
        container.innerHTML = renderPreviewRows(layoutData.rows, '');
    }

    function renderPreviewRows(rows, parentPos) {
        var html = '';
        rows.forEach(function (row, ri) {
            var colTpl = row.columns.map(function (c) { return c.width + 'fr'; }).join(' ');
            html += '<div style="display:grid;grid-template-columns:' + colTpl + ';gap:4px;height:' + row.height + '%;min-height:20px;">';
            row.columns.forEach(function (col, ci) {
                var pos = parentPos ? parentPos + '.R' + (ri + 1) + 'C' + (ci + 1) : 'R' + (ri + 1) + 'C' + (ci + 1);
                if (col.rows && col.rows.length > 0) {
                    html += '<div style="display:flex;flex-direction:column;gap:3px;border:2px dashed #ffb74d;border-radius:4px;padding:3px;background:rgba(255,152,0,0.05);">';
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

    // === Ana Render ===
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
        html += renderTemplates();
        html += renderRowControls(layoutData.rows, '', 'root', 0);

        // Footer
        html += '<div class="lb-footer">';
        html += '<button type="button" class="lb-btn lb-btn-text lb-btn-secondary" onclick="LayoutBuilder.equalDistributeHeights(\'\')">';
        html += '<i class="fas fa-equals"></i> ' + T('equalDistribute');
        html += '</button>';
        html += '<div class="lb-footer-label">';
        html += T('totalHeight') + ': <span id="lb-hb-root" class="lb-badge valid">100%</span>';
        html += '</div></div>';

        container.innerHTML = html;
    }

    // === Şablon Render ===
    function renderTemplates() {
        var html = '<div class="lb-templates-section">';
        html += '<div class="lb-templates-label"><i class="fas fa-th-large me-1"></i>' + T('templates') + '</div>';
        html += '<div class="lb-templates">';
        TEMPLATES.forEach(function (tpl, i) {
            html += '<div class="lb-template" onclick="LayoutBuilder.applyTemplate(' + i + ')">';
            html += '<div class="lb-tpl-preview">' + renderTemplatePreview(tpl.data) + '</div>';
            html += '<div class="lb-tpl-name">' + T('tpl_' + tpl.key) + '</div>';
            html += '</div>';
        });
        html += '</div></div>';
        return html;
    }

    function renderTemplatePreview(data) {
        var html = '';
        data.rows.forEach(function (row) {
            html += '<div class="lb-tpl-row" style="flex:' + row.height + ';">';
            row.columns.forEach(function (col) {
                html += '<div class="lb-tpl-cell" style="flex:' + col.width + ';"></div>';
            });
            html += '</div>';
        });
        return html;
    }

    // === Satır Kontrolleri ===
    function renderRowControls(rows, path, idPrefix, depth) {
        var html = '';
        var p = escapeAttr(path);

        rows.forEach(function (row, ri) {
            // Satır kartı
            html += '<div class="lb-row">';

            // Header: badge + height + sağda butonlar
            html += '<div class="lb-row-header">';
            html += '<div class="lb-row-left">';
            html += '<span class="lb-row-badge">' + T('row') + ' ' + (ri + 1) + '</span>';
            html += '<div class="lb-h-group">';
            html += '<label>H:</label>';
            html += '<input type="number" class="lb-h-input" min="1" max="100" value="' + row.height + '" oninput="LayoutBuilder.updateHeight(\'' + p + '\',' + ri + ',this.value)">';
            html += '<span style="font-size:0.7rem;color:#888;font-weight:600;">%</span>';
            html += '</div>';
            html += '</div>';

            html += '<div class="lb-row-right">';
            html += '<span id="lb-wb-' + idPrefix + '-' + ri + '" class="lb-badge valid">100%</span>';
            html += '<button type="button" class="lb-btn lb-btn-sm lb-btn-primary" onclick="LayoutBuilder.addColumn(\'' + p + '\',' + ri + ')" title="' + T('addCol') + '"' + (row.columns.length >= MAX_COLS ? ' disabled' : '') + '><i class="fas fa-plus"></i></button>';
            html += '<button type="button" class="lb-btn lb-btn-sm lb-btn-secondary" onclick="LayoutBuilder.equalDistributeWidths(\'' + p + '\',' + ri + ')" title="' + T('equalDistribute') + '"><i class="fas fa-equals"></i></button>';
            html += '<button type="button" class="lb-btn lb-btn-sm lb-btn-danger" onclick="LayoutBuilder.removeRow(\'' + p + '\',' + ri + ')" title="' + T('removeRow') + '"' + (rows.length <= 1 ? ' disabled' : '') + '><i class="fas fa-trash-alt"></i></button>';
            html += '</div>';
            html += '</div>';

            // Sütun chip'leri (oransal görsel bar)
            html += '<div class="lb-columns">';
            row.columns.forEach(function (col, ci) {
                var hasSplit = col.rows && col.rows.length > 0;
                var chipClass = hasSplit ? 'nested' : 'leaf';

                html += '<div class="lb-col-chip ' + chipClass + '" style="flex:' + col.width + ';">';
                html += '<span class="lb-col-chip-label">C' + (ci + 1);
                if (hasSplit) html += ' <i class="fas fa-layer-group" style="font-size:0.5rem;"></i>';
                html += '</span>';

                html += '<div class="lb-w-group">';
                html += '<input type="number" class="lb-w-input" min="1" max="100" value="' + col.width + '" oninput="LayoutBuilder.updateWidth(\'' + p + '\',' + ri + ',' + ci + ',this.value)">';
                html += '<span class="lb-col-pct">%</span>';
                html += '</div>';

                html += '<div class="lb-col-actions">';
                if (!hasSplit && depth < MAX_DEPTH) {
                    html += '<button type="button" class="lb-btn lb-btn-xs lb-btn-warning" onclick="LayoutBuilder.splitCell(\'' + p + '\',' + ri + ',' + ci + ')" title="' + T('splitCell') + '"><i class="fas fa-th-large"></i></button>';
                }
                if (hasSplit) {
                    html += '<button type="button" class="lb-btn lb-btn-xs lb-btn-danger" onclick="LayoutBuilder.unsplitCell(\'' + p + '\',' + ri + ',' + ci + ')" title="' + T('unsplitCell') + '"><i class="fas fa-compress-alt"></i></button>';
                }
                if (row.columns.length > 1) {
                    html += '<button type="button" class="lb-btn lb-btn-xs lb-btn-danger" onclick="LayoutBuilder.removeColumn(\'' + p + '\',' + ri + ',' + ci + ')" title="' + T('removeCol') + '"><i class="fas fa-times"></i></button>';
                }
                html += '</div>';
                html += '</div>';
            });
            html += '</div>';

            // Nested paneller — sütun bar'ının DIŞINDA, tam genişlikte, COLLAPSE
            row.columns.forEach(function (col, ci) {
                if (col.rows && col.rows.length > 0) {
                    var childPath = path ? path + '.' + ri + '.' + ci : ri + '.' + ci;
                    var childIdPrefix = idPrefix + '-' + ri + '-' + ci;
                    var panelKey = makeId(childPath);
                    var isOpen = openPanels[childPath] === true;

                    html += '<div class="lb-nested-panel">';

                    // Toggle başlık
                    html += '<button type="button" class="lb-nested-toggle" onclick="LayoutBuilder.toggleNested(\'' + escapeAttr(childPath) + '\')">';
                    html += '<span class="lb-nested-toggle-left">';
                    html += '<i class="fas fa-layer-group"></i>';
                    html += T('row') + ' ' + (ri + 1) + ' / C' + (ci + 1) + ' — ' + T('nested');
                    html += '</span>';
                    html += '<span id="lb-narrow-' + panelKey + '" class="lb-nested-toggle-arrow' + (isOpen ? ' open' : '') + '"><i class="fas fa-chevron-right"></i></span>';
                    html += '</button>';

                    // Collapse body
                    html += '<div id="lb-nbody-' + panelKey + '" class="lb-nested-body' + (isOpen ? ' open' : '') + '">';
                    html += renderRowControls(col.rows, childPath, childIdPrefix, depth + 1);

                    // Nested footer
                    html += '<div class="lb-nested-footer">';
                    html += '<button type="button" class="lb-btn lb-btn-text lb-btn-secondary" onclick="LayoutBuilder.equalDistributeHeights(\'' + escapeAttr(childPath) + '\')">';
                    html += '<i class="fas fa-equals"></i> ' + T('equalDistribute');
                    html += '</button>';
                    html += '<div class="lb-footer-label" style="font-size:0.72rem;">';
                    html += T('totalHeight') + ': <span id="lb-hb-' + childIdPrefix + '" class="lb-badge valid">100%</span>';
                    html += '</div></div>';

                    html += '</div>'; // nested-body
                    html += '</div>'; // nested-panel
                }
            });

            html += '</div>'; // lb-row

            // Satır arası "ekle" ayracı
            html += '<div class="lb-add-row-divider">';
            html += '<button type="button" class="lb-add-row-btn" onclick="LayoutBuilder.addRow(\'' + p + '\')"' + (rows.length >= MAX_ROWS ? ' disabled' : '') + '>';
            html += '<i class="fas fa-plus"></i> ' + T('addRow');
            html += '</button></div>';
        });

        return html;
    }

    function escapeAttr(str) {
        return str.replace(/'/g, "\\'").replace(/"/g, '&quot;');
    }

    // === Dışa Açık API ===
    window.LayoutBuilder = {
        init: init,
        applyTemplate: applyTemplate,
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
        toggleNested: toggleNested,
        getData: function () { return layoutData; }
    };
})();
