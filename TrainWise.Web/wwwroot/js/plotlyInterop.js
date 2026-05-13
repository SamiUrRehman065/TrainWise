// ═══════════════════════════════════════════
//  TrainWise — Plotly.js Interop Functions
//  All chart rendering via JS called from Blazor
// ═══════════════════════════════════════════

window.PlotlyInterop = {
    /**
     * Render a confusion matrix heatmap.
     * @param {string} elementId - Target div ID
     * @param {number[][]} matrix - 2D array of values
     * @param {string[]} labels - Class labels
     */
    renderConfusionMatrix: function (elementId, matrix, labels) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const data = [{
            z: matrix,
            x: labels,
            y: labels,
            type: 'heatmap',
            colorscale: [
                [0, '#0F1216'],
                [0.5, '#1A3A2E'],
                [1, '#C8FF00']
            ],
            showscale: true,
            hoverongaps: false,
            text: matrix.map(row => row.map(val => val.toString())),
            texttemplate: '%{text}',
            textfont: { color: '#EEEEE9', size: 14 }
        }];

        const layout = {
            xaxis: { title: 'Predicted', color: '#8899BB', gridcolor: '#1A2030' },
            yaxis: { title: 'Actual', color: '#8899BB', gridcolor: '#1A2030', autorange: 'reversed' },
            paper_bgcolor: '#0F1216',
            plot_bgcolor: '#0F1216',
            font: { family: 'JetBrains Mono, monospace', color: '#EEEEE9' },
            margin: { t: 30, r: 30, b: 60, l: 60 }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a metrics bar chart (accuracy, precision, recall, F1).
     * @param {string} elementId - Target div ID
     * @param {string[]} metricNames - e.g. ["Accuracy", "Precision", "Recall", "F1"]
     * @param {number[]} metricValues - corresponding values
     */
    renderMetricsBar: function (elementId, metricNames, metricValues) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const colors = metricNames.map((_, i) => {
            const palette = ['#C8FF00', '#4FC3F7', '#FF6B6B', '#B8E600'];
            return palette[i % palette.length];
        });

        const data = [{
            x: metricNames,
            y: metricValues,
            type: 'bar',
            marker: {
                color: colors,
                line: { color: '#1A2030', width: 1 }
            },
            text: metricValues.map(v => (v * 100).toFixed(1) + '%'),
            textposition: 'outside',
            textfont: { color: '#EEEEE9', size: 12 }
        }];

        const layout = {
            yaxis: {
                range: [0, 1.15],
                title: 'Score',
                color: '#8899BB',
                gridcolor: '#1A2030',
                tickformat: '.0%'
            },
            xaxis: { color: '#8899BB' },
            paper_bgcolor: '#0F1216',
            plot_bgcolor: '#0F1216',
            font: { family: 'Syne, sans-serif', color: '#EEEEE9' },
            margin: { t: 30, r: 20, b: 50, l: 60 },
            bargap: 0.3
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a feature importance horizontal bar chart.
     * @param {string} elementId - Target div ID
     * @param {string[]} featureNames
     * @param {number[]} importances
     */
    renderFeatureImportance: function (elementId, featureNames, importances) {
        const el = document.getElementById(elementId);
        if (!el) return;

        // Sort by importance descending, take top 15
        const paired = featureNames.map((name, i) => ({ name, value: importances[i] }));
        paired.sort((a, b) => a.value - b.value);
        const top = paired.slice(-15);

        const data = [{
            y: top.map(p => p.name),
            x: top.map(p => p.value),
            type: 'bar',
            orientation: 'h',
            marker: {
                color: top.map(p => p.value),
                colorscale: [[0, '#1A2030'], [1, '#C8FF00']],
                line: { width: 0 }
            },
            text: top.map(p => p.value.toFixed(4)),
            textposition: 'outside',
            textfont: { color: '#8899BB', size: 11 }
        }];

        const layout = {
            xaxis: { title: 'Importance', color: '#8899BB', gridcolor: '#1A2030' },
            yaxis: { color: '#8899BB', automargin: true },
            paper_bgcolor: '#0F1216',
            plot_bgcolor: '#0F1216',
            font: { family: 'JetBrains Mono, monospace', color: '#EEEEE9', size: 11 },
            margin: { t: 20, r: 60, b: 50, l: 10 }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a Pearson correlation heatmap.
     * @param {string} elementId
     * @param {number[][]} matrix
     * @param {string[]} columnNames
     */
    renderCorrelationHeatmap: function (elementId, matrix, columnNames) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const data = [{
            z: matrix,
            x: columnNames,
            y: columnNames,
            type: 'heatmap',
            colorscale: [
                [0, '#FF6B6B'],
                [0.5, '#0F1216'],
                [1, '#C8FF00']
            ],
            zmin: -1,
            zmax: 1,
            showscale: true,
            text: matrix.map(row => row.map(val => val.toFixed(2))),
            texttemplate: '%{text}',
            textfont: { color: '#EEEEE9', size: 10 }
        }];

        const layout = {
            paper_bgcolor: '#0F1216',
            plot_bgcolor: '#0F1216',
            font: { family: 'JetBrains Mono, monospace', color: '#EEEEE9', size: 10 },
            margin: { t: 30, r: 30, b: 80, l: 80 },
            xaxis: { tickangle: -45, color: '#8899BB' },
            yaxis: { color: '#8899BB', autorange: 'reversed' }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a class distribution pie chart.
     * @param {string} elementId
     * @param {string[]} labels - Class names
     * @param {number[]} values - Counts per class
     */
    renderClassDistribution: function (elementId, labels, values) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const colors = ['#C8FF00', '#4FC3F7', '#FF6B6B', '#B8E600', '#9C27B0', '#FF9800', '#00BCD4', '#E91E63'];

        const data = [{
            labels: labels,
            values: values,
            type: 'pie',
            hole: 0.45,
            marker: {
                colors: colors.slice(0, labels.length),
                line: { color: '#0F1216', width: 2 }
            },
            textinfo: 'label+percent',
            textfont: { color: '#EEEEE9', size: 12 },
            hoverinfo: 'label+value+percent'
        }];

        const layout = {
            paper_bgcolor: '#0F1216',
            plot_bgcolor: '#0F1216',
            font: { family: 'Syne, sans-serif', color: '#EEEEE9' },
            margin: { t: 30, r: 30, b: 30, l: 30 },
            showlegend: true,
            legend: { font: { color: '#8899BB', size: 11 } }
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Render a regression metrics comparison bar (R², RMSE, MAE).
     * @param {string} elementId
     * @param {object} metrics - { r2Score, rmse, mae }
     */
    renderRegressionMetrics: function (elementId, metrics) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const names = ['R² Score', 'RMSE', 'MAE'];
        const values = [metrics.r2Score || 0, metrics.rmse || 0, metrics.mae || 0];
        const colors = ['#C8FF00', '#4FC3F7', '#FF6B6B'];

        const data = [{
            x: names,
            y: values,
            type: 'bar',
            marker: { color: colors, line: { color: '#1A2030', width: 1 } },
            text: values.map(v => v.toFixed(4)),
            textposition: 'outside',
            textfont: { color: '#EEEEE9', size: 12 }
        }];

        const layout = {
            yaxis: { title: 'Value', color: '#8899BB', gridcolor: '#1A2030' },
            xaxis: { color: '#8899BB' },
            paper_bgcolor: '#0F1216',
            plot_bgcolor: '#0F1216',
            font: { family: 'Syne, sans-serif', color: '#EEEEE9' },
            margin: { t: 30, r: 20, b: 50, l: 60 },
            bargap: 0.3
        };

        Plotly.newPlot(el, data, layout, { responsive: true, displayModeBar: false });
    },

    /**
     * Destroy a Plotly chart to free memory.
     * @param {string} elementId
     */
    destroy: function (elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            Plotly.purge(el);
        }
    }
};
